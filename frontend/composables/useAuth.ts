import type { LoginCredentials, RegisterCredentials, AuthResponse, User } from '~/types/auth'

export const useAuth = () => {
  const authStore = useAuthStore()
  const router = useRouter()
  const config = useRuntimeConfig()

  const login = async (credentials: LoginCredentials) => {
    authStore.setLoading(true)
    authStore.clearError()

    try {
      const { data, error } = await useFetch<AuthResponse>('/api/Auth/login', {
        method: 'POST',
        baseURL: config.public.apiUrl,
        body: credentials
      })

      if (error.value) {
        // Parse error message - check both 'message' and 'errors' array
        let errorMessage = error.value.data?.message || error.value.data?.error || 'Nieprawidłowy email lub hasło'

        // If there's an errors array, use the first error
        if (error.value.data?.errors && Array.isArray(error.value.data.errors) && error.value.data.errors.length > 0) {
          errorMessage = error.value.data.errors[0]
        }

        authStore.setError(errorMessage)
        return { success: false, error: errorMessage }
      }

      if (data.value?.user && data.value?.accessToken && data.value?.refreshToken) {
        // Store user and tokens in Pinia store (persists to localStorage)
        authStore.setUser(data.value.user)
        authStore.setTokens(data.value.accessToken, data.value.refreshToken)

        // Check if email is verified - redirect accordingly
        if (data.value.user.isEmailVerified) {
          await router.push('/')
        } else {
          await router.push(`/auth/verify-email?email=${encodeURIComponent(data.value.user.email)}`)
        }

        return { success: true, error: null }
      }

      authStore.setError('Wystąpił nieoczekiwany błąd')
      return { success: false, error: 'Wystąpił nieoczekiwany błąd' }
    } catch {
      const errorMessage = 'Wystąpił błąd podczas logowania'
      authStore.setError(errorMessage)
      return { success: false, error: errorMessage }
    } finally {
      authStore.setLoading(false)
    }
  }

  const register = async (credentials: RegisterCredentials) => {
    authStore.setLoading(true)
    authStore.clearError()

    try {
      const { data, error } = await useFetch<{ userId: string; email: string; message: string }>('/api/Auth/register', {
        method: 'POST',
        baseURL: config.public.apiUrl,
        body: credentials
      })

      if (error.value) {
        // Parse error message - check both 'message' and 'errors' array
        let errorMessage = error.value.data?.message || error.value.data?.error || 'Rejestracja nie powiodła się'

        // If there's an errors array, use the first error
        if (error.value.data?.errors && Array.isArray(error.value.data.errors) && error.value.data.errors.length > 0) {
          errorMessage = error.value.data.errors[0]
        }

        authStore.setError(errorMessage)
        return { success: false, error: errorMessage }
      }

      if (data.value) {
        // Redirect to verify email page with auto-start timer
        await router.push(`/auth/verify-email?email=${encodeURIComponent(credentials.email)}&autostart=true`)
        return { success: true, error: null }
      }

      authStore.setError('Wystąpił nieoczekiwany błąd')
      return { success: false, error: 'Wystąpił nieoczekiwany błąd' }
    } catch {
      const errorMessage = 'Wystąpił błąd podczas rejestracji'
      authStore.setError(errorMessage)
      return { success: false, error: errorMessage }
    } finally {
      authStore.setLoading(false)
    }
  }

  const logout = async () => {
    authStore.setLoading(true)

    try {
      await useFetch('/api/Auth/logout', {
        method: 'POST',
        baseURL: config.public.apiUrl
      })

      // Clear user and tokens from store (which also clears localStorage)
      authStore.clearUser()

      await router.push('/auth/login')
      return { success: true, error: null }
    } catch {
      const errorMessage = 'Wystąpił błąd podczas wylogowania'
      authStore.setError(errorMessage)
      return { success: false, error: errorMessage }
    } finally {
      authStore.setLoading(false)
    }
  }

  const checkAuth = async () => {
    try {
      const { data } = await useFetch<{ user: User }>('/api/Auth/me', {
        baseURL: config.public.apiUrl
      })

      if (data.value?.user) {
        authStore.setUser(data.value.user)
        return true
      }

      authStore.clearUser()
      return false
    } catch {
      authStore.clearUser()
      return false
    }
  }

  const refreshToken = async () => {
    try {
      const currentRefreshToken = authStore.refreshToken

      if (!currentRefreshToken) {
        throw new Error('No refresh token available')
      }

      const { data, error } = await useFetch<{ accessToken: string; refreshToken: string }>('/api/Auth/refresh-token', {
        method: 'POST',
        baseURL: config.public.apiUrl,
        body: { refreshToken: currentRefreshToken }
      })

      if (error.value || !data.value) {
        authStore.clearUser()
        await router.push('/auth/login')
        return { success: false, error: 'Token refresh failed' }
      }

      // Update tokens in store
      authStore.setTokens(data.value.accessToken, data.value.refreshToken)

      return { success: true, error: null }
    } catch {
      authStore.clearUser()
      await router.push('/auth/login')
      return { success: false, error: 'Token refresh failed' }
    }
  }

  return {
    login,
    register,
    logout,
    checkAuth,
    refreshToken,
    isLoading: computed(() => authStore.isLoading),
    error: computed(() => authStore.error),
    user: computed(() => authStore.user),
    isAuthenticated: computed(() => authStore.isAuthenticated)
  }
}

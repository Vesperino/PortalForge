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
        const errorMessage = error.value.data?.message || 'Nieprawidłowy email lub hasło'
        authStore.setError(errorMessage)
        return { success: false, error: errorMessage }
      }

      if (data.value?.user) {
        authStore.setUser(data.value.user)
        await router.push('/')
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
        const errorMessage = error.value.data?.message || 'Rejestracja nie powiodła się'
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
    } catch (err) {
      const errorMessage = 'Wystąpił błąd podczas rejestracji'
      authStore.setError(errorMessage)
      console.error('Registration error:', err)
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

  return {
    login,
    register,
    logout,
    checkAuth,
    isLoading: computed(() => authStore.isLoading),
    error: computed(() => authStore.error),
    user: computed(() => authStore.user),
    isAuthenticated: computed(() => authStore.isAuthenticated)
  }
}

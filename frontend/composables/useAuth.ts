import type { User } from '~/types/auth'

interface LoginResponse {
  user: User
  accessToken: string
  refreshToken: string
}

export function useAuth() {
  const config = useRuntimeConfig()
  const apiUrl = config.public.apiUrl || 'http://localhost:5155'
  const authStore = useAuthStore()
  const router = useRouter()

  async function login(email: string, password: string) {
    try {
      const response = await $fetch<LoginResponse>(`${apiUrl}/api/auth/login`, {
        method: 'POST',
        body: {
          email,
          password
        }
      })

      // Zapisz tokeny
      authStore.setTokens(response.accessToken, response.refreshToken)

      // Zapisz użytkownika
      authStore.setUser(response.user)

      return response
    } catch (error: any) {
      console.error('Login error:', error)
      throw new Error(error?.data?.message || 'Nieprawidłowy email lub hasło')
    }
  }

  async function logout() {
    try {
      // Wywołaj backend logout jeśli jest token
      if (authStore.accessToken) {
        await $fetch(`${apiUrl}/api/auth/logout`, {
          method: 'POST',
          headers: {
            Authorization: `Bearer ${authStore.accessToken}`
          }
        })
      }
    } catch (error) {
      console.error('Logout error:', error)
    } finally {
      // Zawsze wyczyść lokalnie
      authStore.clearUser()
      await router.push('/auth/login')
    }
  }

  async function refreshToken() {
    try {
      if (!authStore.refreshToken) {
        throw new Error('No refresh token available')
      }

      const response = await $fetch<{ accessToken: string; refreshToken: string }>(`${apiUrl}/api/auth/refresh-token`, {
        method: 'POST',
        body: {
          refreshToken: authStore.refreshToken
        }
      })

      authStore.setTokens(response.accessToken, response.refreshToken)

      return response
    } catch (error) {
      console.error('Refresh token error:', error)
      authStore.clearUser()
      await router.push('/auth/login')
      throw error
    }
  }

  return {
    login,
    logout,
    refreshToken
  }
}

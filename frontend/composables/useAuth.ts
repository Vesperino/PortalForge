import type { User } from '~/types/auth'
import { UserRole } from '~/types/auth'

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
      const response = await $fetch(`${apiUrl}/api/auth/login`, {
        method: 'POST',
        body: {
          email,
          password
        }
      }) as LoginResponse

      // Zapisz tokeny
      authStore.setTokens(response.accessToken, response.refreshToken)

      // Zapisz użytkownika
      authStore.setUser(response.user)

      // Sprawdź czy użytkownik musi zmienić hasło
      if (response.user.mustChangePassword) {
        await router.push('/auth/change-password')
      }

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
      // Avoid redundant navigation
      if (router.currentRoute.value.path !== '/auth/login') {
        await router.push('/auth/login')
      }
    }
  }

  async function refreshToken() {
    try {
      if (!authStore.refreshToken) {
        throw new Error('No refresh token available')
      }

      const response = await $fetch(`${apiUrl}/api/auth/refresh-token`, {
        method: 'POST',
        body: {
          refreshToken: authStore.refreshToken
        }
      }) as { accessToken: string; refreshToken: string }

      authStore.setTokens(response.accessToken, response.refreshToken)

      return response
    } catch (error) {
      console.error('Refresh token error:', error)
      authStore.clearUser()
      // Avoid redundant navigation
      if (router.currentRoute.value.path !== '/auth/login') {
        await router.push('/auth/login')
      }
      throw error
    }
  }

  function getAuthHeaders() {
    return {
      Authorization: `Bearer ${authStore.accessToken}`
    }
  }

  function isTokenExpired(token: string): boolean {
    try {
      // Decode JWT token (format: header.payload.signature)
      const parts = token.split('.')
      if (parts.length !== 3 || !parts[1]) {
        return true
      }

      // Decode payload (base64url)
      const payload = JSON.parse(atob(parts[1].replace(/-/g, '+').replace(/_/g, '/')))

      // Check if token has exp claim
      if (!payload.exp) {
        return false // No expiration claim, assume valid
      }

      // Compare with current time (exp is in seconds, Date.now() is in milliseconds)
      const currentTime = Math.floor(Date.now() / 1000)
      return payload.exp < currentTime
    } catch (error) {
      console.error('Error decoding token:', error)
      return true // If we can't decode, assume expired
    }
  }

  function checkTokenExpiration(): boolean {
    if (!authStore.accessToken) {
      return false // No token, not expired (will be handled by auth check)
    }

    return isTokenExpired(authStore.accessToken)
  }

  async function hasPermission(_permissionName: string): Promise<boolean> {
    // For now, return true for admins and HR, false for others
    // In production, this should check user's actual permissions
    const userRole = authStore.user?.role
    return userRole === UserRole.Admin || userRole === UserRole.HR
  }

  return {
    login,
    logout,
    refreshToken,
    getAuthHeaders,
    hasPermission,
    isTokenExpired,
    checkTokenExpiration
  }
}

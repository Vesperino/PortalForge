import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuth } from '~/composables/useAuth'
import { useAuthStore } from '~/stores/auth'
import { UserRole } from '~/types/auth'
import type { User } from '~/types/auth'

describe('useAuth', () => {
  let mockRouter: { push: ReturnType<typeof vi.fn>; currentRoute: { value: { path: string } } }

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()

    mockRouter = {
      push: vi.fn(),
      currentRoute: { value: { path: '/dashboard' } }
    }
    globalThis.useRouter = vi.fn(() => mockRouter)
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  describe('login', () => {
    it('should login successfully and store tokens', async () => {
      const mockUser: User = {
        id: '1',
        userId: 1,
        email: 'test@example.com',
        firstName: 'John',
        lastName: 'Doe',
        isEmailVerified: true,
        role: UserRole.Employee
      }

      const mockResponse = {
        user: mockUser,
        accessToken: 'access-token-123',
        refreshToken: 'refresh-token-456'
      }

      global.$fetch = vi.fn().mockResolvedValue(mockResponse)

      const { login } = useAuth()
      const result = await login('test@example.com', 'password123')

      expect(result).toEqual(mockResponse)
      expect(global.$fetch).toHaveBeenCalledWith(
        'http://localhost:5155/api/auth/login',
        expect.objectContaining({
          method: 'POST',
          body: {
            email: 'test@example.com',
            password: 'password123'
          }
        })
      )

      const authStore = useAuthStore()
      expect(authStore.accessToken).toBe('access-token-123')
      expect(authStore.refreshToken).toBe('refresh-token-456')
      expect(authStore.user).toEqual(mockUser)
    })

    it('should redirect to change password page if mustChangePassword is true', async () => {
      const mockUser: User = {
        id: '1',
        userId: 1,
        email: 'test@example.com',
        firstName: 'John',
        lastName: 'Doe',
        isEmailVerified: true,
        role: UserRole.Employee,
        mustChangePassword: true
      }

      const mockResponse = {
        user: mockUser,
        accessToken: 'access-token-123',
        refreshToken: 'refresh-token-456'
      }

      global.$fetch = vi.fn().mockResolvedValue(mockResponse)

      const { login } = useAuth()
      await login('test@example.com', 'password123')

      expect(mockRouter.push).toHaveBeenCalledWith('/auth/change-password')
    })

    it('should throw error on invalid credentials', async () => {
      const mockError = {
        data: {
          message: 'Invalid email or password'
        }
      }

      global.$fetch = vi.fn().mockRejectedValue(mockError)

      const { login } = useAuth()

      await expect(login('test@example.com', 'wrongpassword')).rejects.toThrow(
        'Invalid email or password'
      )
    })

    it('should throw default error message when no message in response', async () => {
      global.$fetch = vi.fn().mockRejectedValue({})

      const { login } = useAuth()

      await expect(login('test@example.com', 'password')).rejects.toThrow(
        'Nieprawidłowy email lub hasło'
      )
    })
  })

  describe('logout', () => {
    it('should clear user and redirect to login page', async () => {
      const authStore = useAuthStore()
      authStore.accessToken = 'test-token'
      authStore.setUser({
        id: '1',
        userId: 1,
        email: 'test@example.com',
        firstName: 'John',
        lastName: 'Doe',
        isEmailVerified: true,
        role: UserRole.Employee
      })

      global.$fetch = vi.fn().mockResolvedValue(undefined)

      const { logout } = useAuth()
      await logout()

      expect(global.$fetch).toHaveBeenCalledWith(
        'http://localhost:5155/api/auth/logout',
        expect.objectContaining({
          method: 'POST',
          headers: expect.objectContaining({
            Authorization: 'Bearer test-token'
          })
        })
      )

      expect(authStore.user).toBeNull()
      expect(authStore.accessToken).toBeNull()
      expect(mockRouter.push).toHaveBeenCalledWith('/auth/login')
    })

    it('should clear user even if backend logout fails', async () => {
      const authStore = useAuthStore()
      authStore.accessToken = 'test-token'
      authStore.setUser({
        id: '1',
        userId: 1,
        email: 'test@example.com',
        firstName: 'John',
        lastName: 'Doe',
        isEmailVerified: true,
        role: UserRole.Employee
      })

      global.$fetch = vi.fn().mockRejectedValue(new Error('Network error'))

      const { logout } = useAuth()
      await logout()

      expect(authStore.user).toBeNull()
      expect(authStore.accessToken).toBeNull()
    })

    it('should not redirect if already on login page', async () => {
      mockRouter.currentRoute.value.path = '/auth/login'
      const authStore = useAuthStore()
      authStore.accessToken = 'test-token'

      global.$fetch = vi.fn().mockResolvedValue(undefined)

      const { logout } = useAuth()
      await logout()

      expect(mockRouter.push).not.toHaveBeenCalled()
    })

    it('should not call backend logout if no token exists', async () => {
      const authStore = useAuthStore()
      authStore.accessToken = null

      global.$fetch = vi.fn().mockResolvedValue(undefined)

      const { logout } = useAuth()
      await logout()

      expect(global.$fetch).not.toHaveBeenCalled()
    })
  })

  describe('refreshToken', () => {
    it('should refresh tokens successfully', async () => {
      const authStore = useAuthStore()
      authStore.setTokens('old-access', 'old-refresh')

      const mockResponse = {
        accessToken: 'new-access-token',
        refreshToken: 'new-refresh-token'
      }

      global.$fetch = vi.fn().mockResolvedValue(mockResponse)

      const { refreshToken } = useAuth()
      const result = await refreshToken()

      expect(result).toEqual(mockResponse)
      expect(global.$fetch).toHaveBeenCalledWith(
        'http://localhost:5155/api/auth/refresh-token',
        expect.objectContaining({
          method: 'POST',
          body: {
            refreshToken: 'old-refresh'
          }
        })
      )

      expect(authStore.accessToken).toBe('new-access-token')
      expect(authStore.refreshToken).toBe('new-refresh-token')
    })

    it('should throw error if no refresh token available', async () => {
      const authStore = useAuthStore()
      authStore.refreshToken = null

      const { refreshToken } = useAuth()

      await expect(refreshToken()).rejects.toThrow('No refresh token available')
    })

    it('should clear user and redirect on refresh failure', async () => {
      const authStore = useAuthStore()
      authStore.setTokens('old-access', 'old-refresh')
      authStore.setUser({
        id: '1',
        userId: 1,
        email: 'test@example.com',
        firstName: 'John',
        lastName: 'Doe',
        isEmailVerified: true,
        role: UserRole.Employee
      })

      global.$fetch = vi.fn().mockRejectedValue(new Error('Token expired'))

      const { refreshToken } = useAuth()

      await expect(refreshToken()).rejects.toThrow('Token expired')
      expect(authStore.user).toBeNull()
      expect(mockRouter.push).toHaveBeenCalledWith('/auth/login')
    })
  })

  describe('getAuthHeaders', () => {
    it('should return authorization header with token', () => {
      const authStore = useAuthStore()
      authStore.accessToken = 'test-token-xyz'

      const { getAuthHeaders } = useAuth()
      const headers = getAuthHeaders()

      expect(headers).toEqual({
        Authorization: 'Bearer test-token-xyz'
      })
    })

    it('should return authorization header with null token if not set', () => {
      const { getAuthHeaders } = useAuth()
      const headers = getAuthHeaders()

      expect(headers).toEqual({
        Authorization: 'Bearer null'
      })
    })
  })

  describe('isTokenExpired', () => {
    it('should return false for valid non-expired token', () => {
      const futureExp = Math.floor(Date.now() / 1000) + 3600 // 1 hour from now
      const payload = { exp: futureExp }
      const token = `header.${btoa(JSON.stringify(payload))}.signature`

      const { isTokenExpired } = useAuth()
      const result = isTokenExpired(token)

      expect(result).toBe(false)
    })

    it('should return true for expired token', () => {
      const pastExp = Math.floor(Date.now() / 1000) - 3600 // 1 hour ago
      const payload = { exp: pastExp }
      const token = `header.${btoa(JSON.stringify(payload))}.signature`

      const { isTokenExpired } = useAuth()
      const result = isTokenExpired(token)

      expect(result).toBe(true)
    })

    it('should return false for token without exp claim', () => {
      const payload = { sub: '123' }
      const token = `header.${btoa(JSON.stringify(payload))}.signature`

      const { isTokenExpired } = useAuth()
      const result = isTokenExpired(token)

      expect(result).toBe(false)
    })

    it('should return true for malformed token', () => {
      const { isTokenExpired } = useAuth()

      expect(isTokenExpired('invalid-token')).toBe(true)
      expect(isTokenExpired('')).toBe(true)
      expect(isTokenExpired('only.two')).toBe(true)
    })
  })

  describe('checkTokenExpiration', () => {
    it('should return false if no access token', () => {
      const { checkTokenExpiration } = useAuth()
      const result = checkTokenExpiration()

      expect(result).toBe(false)
    })

    it('should return true if token is expired', () => {
      const authStore = useAuthStore()
      const pastExp = Math.floor(Date.now() / 1000) - 3600
      const payload = { exp: pastExp }
      authStore.accessToken = `header.${btoa(JSON.stringify(payload))}.signature`

      const { checkTokenExpiration } = useAuth()
      const result = checkTokenExpiration()

      expect(result).toBe(true)
    })

    it('should return false if token is not expired', () => {
      const authStore = useAuthStore()
      const futureExp = Math.floor(Date.now() / 1000) + 3600
      const payload = { exp: futureExp }
      authStore.accessToken = `header.${btoa(JSON.stringify(payload))}.signature`

      const { checkTokenExpiration } = useAuth()
      const result = checkTokenExpiration()

      expect(result).toBe(false)
    })
  })

  describe('hasPermission', () => {
    it('should return true for Admin role', async () => {
      const authStore = useAuthStore()
      authStore.setUser({
        id: '1',
        userId: 1,
        email: 'admin@example.com',
        firstName: 'Admin',
        lastName: 'User',
        isEmailVerified: true,
        role: UserRole.Admin
      })

      const { hasPermission } = useAuth()
      const result = await hasPermission('any-permission')

      expect(result).toBe(true)
    })

    it('should return true for HR role', async () => {
      const authStore = useAuthStore()
      authStore.setUser({
        id: '2',
        userId: 2,
        email: 'hr@example.com',
        firstName: 'HR',
        lastName: 'User',
        isEmailVerified: true,
        role: UserRole.HR
      })

      const { hasPermission } = useAuth()
      const result = await hasPermission('any-permission')

      expect(result).toBe(true)
    })

    it('should return false for Employee role', async () => {
      const authStore = useAuthStore()
      authStore.setUser({
        id: '3',
        userId: 3,
        email: 'employee@example.com',
        firstName: 'Employee',
        lastName: 'User',
        isEmailVerified: true,
        role: UserRole.Employee
      })

      const { hasPermission } = useAuth()
      const result = await hasPermission('any-permission')

      expect(result).toBe(false)
    })

    it('should return false for Manager role', async () => {
      const authStore = useAuthStore()
      authStore.setUser({
        id: '4',
        userId: 4,
        email: 'manager@example.com',
        firstName: 'Manager',
        lastName: 'User',
        isEmailVerified: true,
        role: UserRole.Manager
      })

      const { hasPermission } = useAuth()
      const result = await hasPermission('any-permission')

      expect(result).toBe(false)
    })

    it('should return false when no user is set', async () => {
      const { hasPermission } = useAuth()
      const result = await hasPermission('any-permission')

      expect(result).toBe(false)
    })
  })
})

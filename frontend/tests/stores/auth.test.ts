import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '~/stores/auth'
import type { User } from '~/types/auth'
import { UserRole } from '~/types/auth'

describe('useAuthStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('should initialize with null user', () => {
    const authStore = useAuthStore()

    expect(authStore.user).toBeNull()
    expect(authStore.isAuthenticated).toBe(false)
    expect(authStore.isLoading).toBe(false)
    expect(authStore.error).toBeNull()
  })

  it('should set user correctly', () => {
    const authStore = useAuthStore()
    const mockUser: User = {
      id: '1',
      userId: 1,
      email: 'test@example.com',
      firstName: 'John',
      lastName: 'Doe',
      isEmailVerified: true,
      role: UserRole.Employee,
    }

    authStore.setUser(mockUser)
    authStore.setTokens('mock-access-token', 'mock-refresh-token')

    expect(authStore.user).toEqual(mockUser)
    expect(authStore.isAuthenticated).toBe(true)
  })

  it('should set loading state', () => {
    const authStore = useAuthStore()

    authStore.setLoading(true)
    expect(authStore.isLoading).toBe(true)

    authStore.setLoading(false)
    expect(authStore.isLoading).toBe(false)
  })

  it('should set error message', () => {
    const authStore = useAuthStore()
    const errorMessage = 'Invalid credentials'

    authStore.setError(errorMessage)

    expect(authStore.error).toBe(errorMessage)
  })

  it('should clear user', () => {
    const authStore = useAuthStore()
    const mockUser: User = {
      id: '1',
      userId: 1,
      email: 'test@example.com',
      firstName: 'John',
      lastName: 'Doe',
      isEmailVerified: true,
      role: UserRole.Employee,
    }

    authStore.setUser(mockUser)
    authStore.setError('Some error')

    authStore.clearUser()

    expect(authStore.user).toBeNull()
    expect(authStore.error).toBeNull()
    expect(authStore.isAuthenticated).toBe(false)
  })

  it('should clear error', () => {
    const authStore = useAuthStore()
    authStore.setError('Some error')

    authStore.clearError()

    expect(authStore.error).toBeNull()
  })
})

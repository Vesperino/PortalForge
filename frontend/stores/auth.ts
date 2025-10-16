import { defineStore } from 'pinia'
import type { User } from '~/types/auth'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const isAuthenticated = computed(() => user.value !== null)

  const setUser = (userData: User | null) => {
    user.value = userData
  }

  const setLoading = (loading: boolean) => {
    isLoading.value = loading
  }

  const setError = (errorMessage: string | null) => {
    error.value = errorMessage
  }

  const clearUser = () => {
    user.value = null
    error.value = null
  }

  const clearError = () => {
    error.value = null
  }

  return {
    user: readonly(user),
    isLoading: readonly(isLoading),
    error: readonly(error),
    isAuthenticated,
    setUser,
    setLoading,
    setError,
    clearUser,
    clearError
  }
})

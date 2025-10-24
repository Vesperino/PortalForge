import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { User } from '~/types/auth'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)
  const accessToken = ref<string | null>(null)
  const refreshToken = ref<string | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const isAuthenticated = computed(() => user.value !== null && accessToken.value !== null)

  const setUser = (userData: User | null) => {
    user.value = userData

    // Persist user to localStorage for hydration on app restart
    if (userData) {
      if (typeof window !== 'undefined') {
        localStorage.setItem('user', JSON.stringify(userData))
      }
    } else {
      if (typeof window !== 'undefined') {
        localStorage.removeItem('user')
      }
    }
  }

  const setTokens = (access: string, refresh: string) => {
    accessToken.value = access
    refreshToken.value = refresh

    if (typeof window !== 'undefined') {
      localStorage.setItem('accessToken', access)
      localStorage.setItem('refreshToken', refresh)
    }
  }

  const setLoading = (loading: boolean) => {
    isLoading.value = loading
  }

  const setError = (errorMessage: string | null) => {
    error.value = errorMessage
  }

  const clearUser = () => {
    user.value = null
    accessToken.value = null
    refreshToken.value = null
    error.value = null

    if (typeof window !== 'undefined') {
      localStorage.removeItem('user')
      localStorage.removeItem('accessToken')
      localStorage.removeItem('refreshToken')
    }
  }

  const clearError = () => {
    error.value = null
  }

  const hydrateFromStorage = () => {
    if (typeof window !== 'undefined') {
      try {
        const storedUser = localStorage.getItem('user')
        const storedAccessToken = localStorage.getItem('accessToken')
        const storedRefreshToken = localStorage.getItem('refreshToken')

        if (storedUser) {
          user.value = JSON.parse(storedUser) as User
        }
        if (storedAccessToken) {
          accessToken.value = storedAccessToken
        }
        if (storedRefreshToken) {
          refreshToken.value = storedRefreshToken
        }
      } catch (error) {
        console.error('Failed to hydrate from localStorage:', error)
        localStorage.removeItem('user')
        localStorage.removeItem('accessToken')
        localStorage.removeItem('refreshToken')
      }
    }
  }

  const logout = async () => {
    try {
      // TODO: Integrate with Supabase when @nuxtjs/supabase is installed
      // const supabase = useSupabaseClient()
      // await supabase.auth.signOut()
      clearUser()
    } catch (err) {
      console.error('Logout error:', err)
      clearUser()
    }
  }

  return {
    user,
    accessToken,
    refreshToken,
    isLoading,
    error,
    isAuthenticated,
    setUser,
    setTokens,
    setLoading,
    setError,
    clearUser,
    clearError,
    hydrateFromStorage,
    logout
  }
})

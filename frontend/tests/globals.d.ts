import type { Mock } from 'vitest'
import type { Ref, ComputedRef } from 'vue'
import type { User, UserRole } from '../types/auth'
import type { useAuthStore } from '../stores/auth'

declare global {
  // Nuxt composables
  function useRuntimeConfig(): {
    public: {
      apiUrl: string
      supabaseUrl: string
      supabaseKey: string
    }
  }

  function useRouter(): {
    push: Mock
    replace: Mock
    back: Mock
    forward: Mock
    currentRoute: { value: { path: string } }
  }

  function useRoute(): {
    params: Record<string, string>
    query: Record<string, string>
    path: string
  }

  function navigateTo(path: string): void

  function useAuthStore(): ReturnType<typeof useAuthStore>

  function useState<T>(key: string, init?: () => T): Ref<T>

  function clearNuxtState(): void

  function useAuth(): {
    login: Mock
    logout: Mock
    refreshToken: Mock
    getAuthHeaders: Mock
    hasPermission: Mock
    isTokenExpired: Mock
    checkTokenExpiration: Mock
  }

  function useApiError(): {
    handleError: Mock
  }

  function useIconMapping(): {
    iconMapping: Record<string, string>
    getIconifyName: (name: string) => string
  }

  function useNotificationToast(): {
    showSuccess: (message: string) => void
    showError: (message: string) => void
    showWarning: (message: string) => void
    showInfo: (message: string) => void
  }

  // Vue reactivity
  const readonly: typeof import('vue').readonly
  const ref: typeof import('vue').ref
  const computed: typeof import('vue').computed

  // Nuxt $fetch
  var $fetch: Mock

  // Extend globalThis
  interface Window {
    useRuntimeConfig: typeof useRuntimeConfig
    useRouter: typeof useRouter
    useRoute: typeof useRoute
    navigateTo: typeof navigateTo
    useAuthStore: typeof useAuthStore
    useState: typeof useState
    clearNuxtState: typeof clearNuxtState
    useAuth: typeof useAuth
    useApiError: typeof useApiError
    useIconMapping: typeof useIconMapping
    useNotificationToast: typeof useNotificationToast
    readonly: typeof readonly
    ref: typeof ref
    computed: typeof computed
  }
}

export {}

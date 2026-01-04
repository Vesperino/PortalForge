import { vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { readonly, ref, computed } from 'vue'
import { useAuthStore as authStoreComposable } from '../stores/auth'
import { useNotificationToast as actualUseNotificationToast } from '../composables/useNotificationToast'

// Create and activate a Pinia instance for tests
setActivePinia(createPinia())

// Mock Nuxt composables globally
globalThis.useRuntimeConfig = vi.fn(() => ({
  public: {
    apiUrl: 'http://localhost:5155',
    supabaseUrl: 'https://test.supabase.co',
    supabaseKey: 'test-key'
  }
}))

globalThis.useRouter = vi.fn(() => ({
  push: vi.fn(),
  replace: vi.fn(),
  back: vi.fn(),
  forward: vi.fn(),
  currentRoute: { value: { path: '/' } }
}))

globalThis.useRoute = vi.fn(() => ({
  params: {},
  query: {},
  path: '/'
}))

globalThis.navigateTo = vi.fn()

// Mock useAuthStore to return the actual Pinia store
globalThis.useAuthStore = () => authStoreComposable()

// Mock global $fetch
;(globalThis as Record<string, unknown>).$fetch = vi.fn()

// Mock Vue reactivity functions for composables that use them
globalThis.readonly = readonly
globalThis.ref = ref
globalThis.computed = computed

// Mock Nuxt's useState - stores state by key to simulate global state sharing
const stateStore = new Map<string, ReturnType<typeof ref>>()
globalThis.useState = <T>(key: string, init?: () => T) => {
  if (!stateStore.has(key)) {
    stateStore.set(key, ref(init ? init() : undefined))
  }
  return stateStore.get(key) as ReturnType<typeof ref<T>>
}

// Function to clear useState store between tests
globalThis.clearNuxtState = () => {
  stateStore.clear()
}

// Mock useAuth composable
globalThis.useAuth = vi.fn(() => ({
  login: vi.fn(),
  logout: vi.fn(),
  refreshToken: vi.fn(),
  getAuthHeaders: vi.fn(() => ({
    Authorization: `Bearer ${authStoreComposable().accessToken || 'mock-token'}`
  })),
  hasPermission: vi.fn(),
  isTokenExpired: vi.fn(),
  checkTokenExpiration: vi.fn()
}))

// Mock useApiError composable
globalThis.useApiError = vi.fn(() => ({
  handleError: vi.fn()
}))

// Mock useIconMapping composable
globalThis.useIconMapping = vi.fn(() => ({
  iconMapping: {},
  getIconifyName: vi.fn((name: string) => `icon-${name}`)
}))

// Mock useNotificationToast composable - this is imported directly in useApiError
globalThis.useNotificationToast = actualUseNotificationToast

// Mock localStorage
const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
  length: 0,
  key: vi.fn()
}
Object.defineProperty(global, 'localStorage', {
  value: localStorageMock,
  writable: true
})

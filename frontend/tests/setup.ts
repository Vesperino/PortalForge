import { vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore as authStoreComposable } from '~/stores/auth'

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
  forward: vi.fn()
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
global.$fetch = vi.fn()

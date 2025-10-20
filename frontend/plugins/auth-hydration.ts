/**
 * Auth Hydration Plugin
 * Restores authentication state from localStorage when the app initializes
 * This ensures users remain logged in after page refresh
 */
export default defineNuxtPlugin(() => {
  const authStore = useAuthStore()

  // Hydrate auth store from localStorage on app start
  authStore.hydrateFromStorage()
})

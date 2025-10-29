/**
 * Token Refresh Plugin
 * Automatically refreshes access token before it expires (every 50 minutes)
 * Supabase tokens are valid for 1 hour, so we refresh at 50 min mark
 */
export default defineNuxtPlugin(() => {
  const authStore = useAuthStore()
  const { refreshToken } = useAuth()

  // Refresh token every 50 minutes (50 * 60 * 1000 ms)
  const REFRESH_INTERVAL = 50 * 60 * 1000

  // Only run on client-side and if user is authenticated
  if (typeof window !== 'undefined' && authStore.isAuthenticated) {
    const intervalId = setInterval(async () => {
      // Check if still authenticated before refreshing
      if (authStore.isAuthenticated) {
        try {
          console.log('[Token Refresh] Refreshing access token...')
          await refreshToken()
          console.log('[Token Refresh] Token refreshed successfully')
        } catch (error) {
          console.error('[Token Refresh] Failed to refresh token', error)
          clearInterval(intervalId)
        }
      } else {
        // User logged out, stop the interval
        clearInterval(intervalId)
      }
    }, REFRESH_INTERVAL)

    // Clean up interval on app unmount
    window.addEventListener('beforeunload', () => {
      clearInterval(intervalId)
    })
  }
})

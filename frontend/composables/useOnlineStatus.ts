/**
 * Composable to detect online/offline status
 */
export function useOnlineStatus() {
  const isOnline = ref(true)

  onMounted(() => {
    if (import.meta.client) {
      // Set initial status
      isOnline.value = navigator.onLine

      // Listen for status changes
      const handleOnline = () => {
        isOnline.value = true
      }

      const handleOffline = () => {
        isOnline.value = false
      }

      window.addEventListener('online', handleOnline)
      window.addEventListener('offline', handleOffline)

      // Cleanup on unmount
      onUnmounted(() => {
        window.removeEventListener('online', handleOnline)
        window.removeEventListener('offline', handleOffline)
      })
    }
  })

  return {
    isOnline: readonly(isOnline)
  }
}





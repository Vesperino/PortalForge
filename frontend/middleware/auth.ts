export default defineNuxtRouteMiddleware(async () => {
  const authStore = useAuthStore()
  const { checkTokenExpiration, logout } = useAuth()

  // Check if user is authenticated
  if (!authStore.isAuthenticated) {
    return navigateTo('/auth/login')
  }

  // Check if token is expired
  if (checkTokenExpiration()) {
    console.warn('Token expired, logging out...')
    await logout()
    return navigateTo('/auth/login')
  }
})

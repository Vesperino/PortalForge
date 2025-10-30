export default defineNuxtRouteMiddleware(async (to) => {
  const authStore = useAuthStore()
  const { checkTokenExpiration, logout } = useAuth()

  // Skip auth check for login page
  if (to.path === '/auth/login') {
    return
  }

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

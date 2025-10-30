export default defineNuxtRouteMiddleware(async (to) => {
  const authStore = useAuthStore()
  const { checkTokenExpiration, logout } = useAuth()

  // Public routes that don't require authentication
  const publicRoutes = [
    '/auth/login',
    '/auth/register',
    '/auth/reset-password',
    '/auth/callback',
    '/auth/verify-email'
  ]

  // Skip auth check for public routes
  if (publicRoutes.includes(to.path)) {
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

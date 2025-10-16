export default defineNuxtRouteMiddleware((to) => {
  const authStore = useAuthStore()

  // Check if user is authenticated
  if (!authStore.isAuthenticated) {
    return navigateTo('/auth/login')
  }

  // Check if email is verified
  if (!authStore.user?.isEmailVerified && to.path !== '/auth/verify-email') {
    return navigateTo('/auth/verify-email?email=' + authStore.user?.email)
  }
})

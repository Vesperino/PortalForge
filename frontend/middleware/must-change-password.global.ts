export default defineNuxtRouteMiddleware((to) => {
  const authStore = useAuthStore()

  // Check if user is authenticated
  if (!authStore.isAuthenticated) {
    return
  }

  // Check if user must change password
  const mustChangePassword = authStore.user?.mustChangePassword === true

  // If user must change password and is not on the change password page
  if (mustChangePassword && to.path !== '/auth/change-password' && to.path !== '/auth/logout') {
    return navigateTo('/auth/change-password')
  }

  // If user doesn't need to change password and is on the change password page (manual navigation)
  // allow them to access it (they may want to change password voluntarily)
})

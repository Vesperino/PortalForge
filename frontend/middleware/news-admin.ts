export default defineNuxtRouteMiddleware(() => {
  const authStore = useAuthStore()

  const role = authStore.user?.role
    ? authStore.user.role.toString().toLowerCase()
    : null

  const canManage = role === 'admin' || role === 'marketing'

  if (!authStore.isAuthenticated || !canManage) {
    return navigateTo('/dashboard/news')
  }
})

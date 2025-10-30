import { UserRole } from '~/types/auth'

export default defineNuxtRouteMiddleware(async (to, from) => {
  const authStore = useAuthStore()

  // Check if user is authenticated
  if (!authStore.isAuthenticated) {
    return navigateTo('/auth/login')
  }

  // Check if user has admin role
  if (authStore.user?.role !== UserRole.Admin) {
    return navigateTo('/dashboard')
  }
})


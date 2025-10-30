export default defineNuxtRouteMiddleware(async () => {
  const { hasPermission } = useAuth()

  const canManageTemplates = await hasPermission('requests.manage_templates')

  if (!canManageTemplates) {
    return navigateTo('/dashboard')
  }
})


import type { FetchContext } from 'ofetch'

export default defineNuxtPlugin(() => {
  const authStore = useAuthStore()
  const router = useRouter()
  const { refreshToken, logout } = useAuth()

  // Track if we're currently refreshing to avoid multiple refresh attempts
  let isRefreshing = false
  let refreshPromise: Promise<void> | null = null
  const authEndpoints = ['/api/auth/login', '/api/auth/logout', '/api/auth/refresh-token']

  const getRequestUrl = (request: FetchContext['request']): string => {
    if (typeof request === 'string') {
      return request
    }
    if (request && typeof request === 'object' && 'url' in request) {
      const url = (request as { url?: string }).url
      return typeof url === 'string' ? url : ''
    }
    return ''
  }

  const isAuthEndpoint = (request: FetchContext['request']): boolean => {
    const url = getRequestUrl(request)
    return authEndpoints.some((endpoint) => url.includes(endpoint))
  }

  // Add global fetch interceptor
  globalThis.$fetch = $fetch.create({
    async onRequest({ options }: FetchContext) {
      // Add auth token to all requests if available
      if (authStore.accessToken) {
        options.headers = options.headers || {}
        ;(options.headers as Record<string, string>)['Authorization'] = `Bearer ${authStore.accessToken}`
      }
    },

    async onResponseError({ request, response }: FetchContext) {
      // Handle 401 Unauthorized errors
      if (response && response.status === 401) {
        if (isAuthEndpoint(request)) {
          return
        }

        // If we have a refresh token, try to refresh
        if (authStore.refreshToken && !isRefreshing) {
          try {
            isRefreshing = true

            // If already refreshing, wait for that promise
            if (!refreshPromise) {
              refreshPromise = refreshToken()
            }

            await refreshPromise

            // Token refreshed successfully, retry the original request
            // Note: The retry will happen automatically with the new token
            return
          } catch (error) {
            console.error('Token refresh failed:', error)
            // Refresh failed, logout user
            await logout()
            if (router.currentRoute.value.path !== '/auth/login') {
              await router.push('/auth/login')
            }
          } finally {
            isRefreshing = false
            refreshPromise = null
          }
        } else {
          // No refresh token or already refreshing, just logout
          await logout()
          if (router.currentRoute.value.path !== '/auth/login') {
            await router.push('/auth/login')
          }
        }
      }
    }
  })
})


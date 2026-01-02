import type { SystemSetting } from '~/types'

export function useSystemSettings() {
  const config = useRuntimeConfig()
  const apiUrl = config.public.apiUrl || 'http://localhost:5155'
  const authStore = useAuthStore()

  const settings = ref<SystemSetting[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const getAuthHeaders = (): Record<string, string> | undefined => {
    const token = authStore.accessToken
    if (token) {
      return { Authorization: `Bearer ${token}` }
    }
    return undefined
  }

  async function getSettings() {
    isLoading.value = true
    error.value = null

    try {
      const headers = getAuthHeaders()
      const response = await $fetch(`${apiUrl}/api/admin/system-settings`, {
        headers
      }) as SystemSetting[]
      
      settings.value = response
      return response
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch settings'
      console.error('getSettings error:', err)
      throw err
    } finally {
      isLoading.value = false
    }
  }

  async function updateSettings(updates: Array<{ key: string; value: string }>) {
    isLoading.value = true
    error.value = null

    try {
      const headers = getAuthHeaders()
      await $fetch(`${apiUrl}/api/admin/system-settings`, {
        method: 'PUT',
        headers,
        body: updates
      })
      
      // Refresh settings after update
      await getSettings()
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'Failed to update settings'
      console.error('updateSettings error:', err)
      throw err
    } finally {
      isLoading.value = false
    }
  }

  async function testStorage(): Promise<unknown> {
    isLoading.value = true
    error.value = null

    try {
      const headers = getAuthHeaders()
      const response = await $fetch(`${apiUrl}/api/admin/system-settings/test-storage`, {
        method: 'POST',
        headers
      })

      return response
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'Failed to test storage'
      console.error('testStorage error:', err)
      throw err
    } finally {
      isLoading.value = false
    }
  }

  const storageSettings = computed(() =>
    settings.value.filter(s => s.category === 'Storage')
  )

  const aiSettings = computed(() =>
    settings.value.filter(s => s.category === 'AI')
  )

  const getSettingValue = (key: string, defaultValue: string = '') => {
    return settings.value.find(s => s.key === key)?.value || defaultValue
  }

  const getSettingsByCategory = (category: string) => {
    return settings.value.filter(s => s.category === category)
  }

  return {
    settings,
    isLoading,
    error,
    getSettings,
    updateSettings,
    testStorage,
    storageSettings,
    aiSettings,
    getSettingValue,
    getSettingsByCategory
  }
}





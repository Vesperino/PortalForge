import { APP_VERSION, BUILD_DATE } from '~/config/version'

interface BackendVersionInfo {
  version: string
  buildDate: string
  environment: string
}

export function useVersion() {
  const config = useRuntimeConfig()

  const frontendVersion = APP_VERSION
  const frontendBuildDate = BUILD_DATE

  const backendVersion = ref<string | null>(null)
  const backendBuildDate = ref<string | null>(null)
  const backendEnvironment = ref<string | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const fetchBackendVersion = async (): Promise<void> => {
    if (backendVersion.value) return

    isLoading.value = true
    error.value = null

    try {
      const response = await $fetch<BackendVersionInfo>(
        `${config.public.apiUrl}/api/version`,
        {
          timeout: 5000
        }
      )

      backendVersion.value = response.version
      backendBuildDate.value = response.buildDate
      backendEnvironment.value = response.environment
    } catch (err) {
      console.error('Failed to fetch backend version:', err)
      error.value = 'Failed to fetch backend version'
      backendVersion.value = 'N/A'
    } finally {
      isLoading.value = false
    }
  }

  const versionString = computed(() => {
    const fe = `FE: ${frontendVersion}`
    const be = backendVersion.value ? `BE: ${backendVersion.value}` : 'BE: ...'
    return `${fe} | ${be}`
  })

  return {
    frontendVersion,
    frontendBuildDate,
    backendVersion: readonly(backendVersion),
    backendBuildDate: readonly(backendBuildDate),
    backendEnvironment: readonly(backendEnvironment),
    isLoading: readonly(isLoading),
    error: readonly(error),
    fetchBackendVersion,
    versionString
  }
}

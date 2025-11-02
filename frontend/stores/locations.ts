import { defineStore } from 'pinia'
import type { CachedLocation } from '~/types'

export const useLocationsStore = defineStore('locations', () => {
  const config = useRuntimeConfig()
  const apiUrl = config.public.apiUrl || 'http://localhost:5155'
  const authStore = useAuthStore()

  const cachedLocations = ref<CachedLocation[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const getAuthHeaders = (): Record<string, string> | undefined => {
    const token = authStore.accessToken
    if (token) {
      return { Authorization: `Bearer ${token}` }
    }
    return undefined
  }

  async function fetchCachedLocations() {
    isLoading.value = true
    error.value = null

    try {
      const headers = getAuthHeaders()
      const response = await $fetch(`${apiUrl}/api/locations/cached`, {
        headers
      }) as CachedLocation[]
      
      cachedLocations.value = response
    } catch (err: any) {
      error.value = err?.message || 'Failed to fetch cached locations'
      console.error('fetchCachedLocations error:', err)
    } finally {
      isLoading.value = false
    }
  }

  async function geocodeAddress(address: string): Promise<{ latitude: number; longitude: number; address: string } | null> {
    try {
      const headers = getAuthHeaders()
      const response = await $fetch(`${apiUrl}/api/locations/geocode`, {
        method: 'POST',
        headers,
        body: { address }
      }) as { latitude: number; longitude: number; address: string }
      
      return response
    } catch (err) {
      console.error('Geocode error:', err)
      return null
    }
  }

  async function addCachedLocation(location: {
    name: string
    address: string
    latitude: number
    longitude: number
    type: string
  }): Promise<CachedLocation | null> {
    try {
      const headers = getAuthHeaders()
      const response = await $fetch(`${apiUrl}/api/locations/admin/cached`, {
        method: 'POST',
        headers,
        body: location
      }) as CachedLocation
      
      cachedLocations.value.push(response)
      return response
    } catch (err) {
      console.error('Add cached location error:', err)
      return null
    }
  }

  async function deleteCachedLocation(id: number): Promise<boolean> {
    try {
      const headers = getAuthHeaders()
      await $fetch(`${apiUrl}/api/locations/admin/cached/${id}`, {
        method: 'DELETE',
        headers
      })
      
      cachedLocations.value = cachedLocations.value.filter(l => l.id !== id)
      return true
    } catch (err) {
      console.error('Delete cached location error:', err)
      return false
    }
  }

  const officeLocations = computed(() => 
    cachedLocations.value.filter(l => l.type === 'Office')
  )

  const conferenceRoomLocations = computed(() => 
    cachedLocations.value.filter(l => l.type === 'ConferenceRoom')
  )

  const popularLocations = computed(() => 
    cachedLocations.value.filter(l => l.type === 'Popular')
  )

  return {
    cachedLocations,
    isLoading,
    error,
    fetchCachedLocations,
    geocodeAddress,
    addCachedLocation,
    deleteCachedLocation,
    officeLocations,
    conferenceRoomLocations,
    popularLocations
  }
})





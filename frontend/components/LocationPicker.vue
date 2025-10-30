<script setup lang="ts">
import { GoogleMap, Marker } from 'vue3-google-map'

interface Props {
  modelValue: string
  placeId?: string
  label?: string
  required?: boolean
  error?: string
}

const props = defineProps<Props>()
const emit = defineEmits<{
  'update:modelValue': [value: string]
  'update:placeId': [value: string]
}>()

const config = useRuntimeConfig()
const apiKey = config.public.googleMapsApiKey || ''

const location = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

// Default center (Warsaw, Poland)
const center = ref({ lat: 52.2297, lng: 21.0122 })
const markerPosition = ref<{ lat: number; lng: number } | null>(null)
const zoom = ref(12)
const showMap = ref(false)

// Parse location string to get coordinates if available
onMounted(() => {
  if (props.modelValue) {
    // Try to extract coordinates from location string
    // Format: "Address (lat: XX.XXXX, lng: YY.YYYY)"
    const coordsMatch = props.modelValue.match(/lat:\s*([-\d.]+),\s*lng:\s*([-\d.]+)/)
    if (coordsMatch && coordsMatch[1] && coordsMatch[2]) {
      const lat = Number.parseFloat(coordsMatch[1])
      const lng = Number.parseFloat(coordsMatch[2])
      center.value = { lat, lng }
      markerPosition.value = { lat, lng }
    }
  }
})

function toggleMap() {
  showMap.value = !showMap.value
}

function handleMapClick(event: any) {
  if (event.latLng) {
    const lat = event.latLng.lat()
    const lng = event.latLng.lng()
    markerPosition.value = { lat, lng }
    
    // Reverse geocode to get address
    reverseGeocode(lat, lng)
  }
}

async function reverseGeocode(lat: number, lng: number) {
  try {
    const response = await fetch(
      `https://maps.googleapis.com/maps/api/geocode/json?latlng=${lat},${lng}&key=${apiKey}`
    )
    const data = await response.json()

    if (data.results && data.results[0]) {
      const result = data.results[0]
      const address = result.formatted_address
      const placeId = result.place_id

      location.value = `${address} (lat: ${lat.toFixed(6)}, lng: ${lng.toFixed(6)})`
      emit('update:placeId', placeId)
    } else {
      location.value = `lat: ${lat.toFixed(6)}, lng: ${lng.toFixed(6)}`
      emit('update:placeId', '')
    }
  } catch (error) {
    console.error('Reverse geocoding error:', error)
    location.value = `lat: ${lat.toFixed(6)}, lng: ${lng.toFixed(6)}`
    emit('update:placeId', '')
  }
}

async function searchLocation() {
  const query = location.value
  if (!query) return

  try {
    const response = await fetch(
      `https://maps.googleapis.com/maps/api/geocode/json?address=${encodeURIComponent(query)}&key=${apiKey}`
    )
    const data = await response.json()

    if (data.results && data.results[0]) {
      const result = data.results[0]
      const lat = result.geometry.location.lat
      const lng = result.geometry.location.lng
      const placeId = result.place_id

      center.value = { lat, lng }
      markerPosition.value = { lat, lng }
      zoom.value = 15
      showMap.value = true

      location.value = `${result.formatted_address} (lat: ${lat.toFixed(6)}, lng: ${lng.toFixed(6)})`
      emit('update:placeId', placeId)
    }
  } catch (error) {
    console.error('Geocoding error:', error)
  }
}
</script>

<template>
  <div class="space-y-2">
    <label v-if="label" class="block text-sm font-medium text-gray-700 dark:text-gray-300">
      {{ label }}
      <span v-if="required" class="text-red-500">*</span>
    </label>
    
    <div class="flex gap-2">
      <input
        v-model="location"
        type="text"
        class="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
        :class="{ 'border-red-500 dark:border-red-500': error }"
        placeholder="Wprowadź adres lub kliknij na mapie"
        @keyup.enter="searchLocation"
      />
      <button
        type="button"
        class="px-4 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600 transition-colors"
        @click="searchLocation"
      >
        🔍 Szukaj
      </button>
      <button
        type="button"
        class="px-4 py-2 bg-gray-500 text-white rounded-md hover:bg-gray-600 transition-colors"
        @click="toggleMap"
      >
        {{ showMap ? '📍 Ukryj mapę' : '🗺️ Pokaż mapę' }}
      </button>
    </div>
    
    <p v-if="error" class="text-sm text-red-600 dark:text-red-400">{{ error }}</p>
    
    <div v-if="!apiKey" class="p-4 bg-yellow-100 dark:bg-yellow-900 text-yellow-800 dark:text-yellow-200 rounded-lg">
      ⚠️ Google Maps API key nie jest skonfigurowany. Dodaj <code>NUXT_PUBLIC_GOOGLE_MAPS_API_KEY</code> do pliku <code>.env</code>.
    </div>
    
    <div v-if="showMap && apiKey" class="border border-gray-300 dark:border-gray-600 rounded-lg overflow-hidden">
      <GoogleMap
        :api-key="apiKey"
        :center="center"
        :zoom="zoom"
        style="width: 100%; height: 400px"
        @click="handleMapClick"
      >
        <Marker v-if="markerPosition" :options="{ position: markerPosition }" />
      </GoogleMap>
    </div>
    
    <p class="text-sm text-gray-500 dark:text-gray-400">
      💡 Wskazówka: Wpisz adres i kliknij "Szukaj" lub kliknij bezpośrednio na mapie, aby wybrać lokalizację.
    </p>
  </div>
</template>


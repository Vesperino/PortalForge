<script setup lang="ts">
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'

interface Props {
  modelValue: string
  latitude?: number
  longitude?: number
  label?: string
}

interface Emits {
  (e: 'update:modelValue', value: string): void
  (e: 'update:latitude', value: number | undefined): void
  (e: 'update:longitude', value: number | undefined): void
}

const props = withDefaults(defineProps<Props>(), {
  latitude: undefined,
  longitude: undefined,
  label: 'Lokalizacja wydarzenia'
})

const emit = defineEmits<Emits>()

interface NominatimResult {
  place_id: number
  display_name: string
  lat: string
  lon: string
}

const mapContainer = ref<HTMLDivElement | null>(null)
const addressSearch = ref('')
const suggestions = ref<NominatimResult[]>([])
const showSuggestions = ref(false)
const isSearching = ref(false)

let map: L.Map | null = null
let marker: L.Marker | null = null
let searchTimer: ReturnType<typeof setTimeout> | null = null

const isDark = computed(() => {
  if (import.meta.client) {
    return document.documentElement.classList.contains('dark')
  }
  return false
})

onMounted(() => {
  if (!import.meta.client || !mapContainer.value) return

  // Default to Warsaw or provided coordinates
  const lat = props.latitude || 52.2297
  const lng = props.longitude || 21.0122
  const zoom = props.latitude ? 15 : 6

  map = L.map(mapContainer.value).setView([lat, lng], zoom)

  const tileUrl = isDark.value
    ? 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png'
    : 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png'

  L.tileLayer(tileUrl, {
    attribution: '© OpenStreetMap',
    maxZoom: 19
  }).addTo(map)

  // Add marker if coordinates exist
  if (props.latitude && props.longitude) {
    addMarker(props.latitude, props.longitude)
    addressSearch.value = props.modelValue
  }

  // Click on map to place marker
  map.on('click', async (e: L.LeafletMouseEvent) => {
    const { lat, lng } = e.latlng
    addMarker(lat, lng)
    emit('update:latitude', lat)
    emit('update:longitude', lng)

    // Reverse geocode to get address
    try {
      const response = await $fetch<{ display_name: string }>(
        `https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json`,
        { headers: { 'Accept-Language': 'pl' } }
      )
      if (response?.display_name) {
        addressSearch.value = response.display_name
        emit('update:modelValue', response.display_name)
      }
    } catch {
      // Use coordinates as fallback
      const addr = `${lat.toFixed(6)}, ${lng.toFixed(6)}`
      addressSearch.value = addr
      emit('update:modelValue', addr)
    }
  })
})

onBeforeUnmount(() => {
  if (map) {
    map.remove()
    map = null
  }
})

function addMarker(lat: number, lng: number) {
  if (!map) return

  if (marker) {
    map.removeLayer(marker)
  }

  const icon = L.icon({
    iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
    iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
    shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
    iconSize: [25, 41],
    iconAnchor: [12, 41],
    popupAnchor: [1, -34],
    shadowSize: [41, 41]
  })

  marker = L.marker([lat, lng], { icon }).addTo(map)
  map.setView([lat, lng], 15)
}

function handleInput() {
  if (searchTimer) clearTimeout(searchTimer)

  const query = addressSearch.value.trim()
  if (query.length < 3) {
    suggestions.value = []
    showSuggestions.value = false
    return
  }

  searchTimer = setTimeout(() => fetchSuggestions(query), 300)
}

async function fetchSuggestions(query: string) {
  isSearching.value = true
  try {
    const response = await $fetch<NominatimResult[]>(
      'https://nominatim.openstreetmap.org/search',
      {
        params: { q: query, format: 'json', countrycodes: 'pl', limit: 5 },
        headers: { 'Accept-Language': 'pl' }
      }
    )
    suggestions.value = response
    showSuggestions.value = response.length > 0
  } catch {
    suggestions.value = []
    showSuggestions.value = false
  } finally {
    isSearching.value = false
  }
}

function selectSuggestion(suggestion: NominatimResult) {
  const lat = parseFloat(suggestion.lat)
  const lng = parseFloat(suggestion.lon)

  addressSearch.value = suggestion.display_name
  emit('update:modelValue', suggestion.display_name)
  emit('update:latitude', lat)
  emit('update:longitude', lng)

  addMarker(lat, lng)
  suggestions.value = []
  showSuggestions.value = false
}

function hideSuggestions() {
  setTimeout(() => { showSuggestions.value = false }, 200)
}

// Update tiles on dark mode change
watch(isDark, (dark) => {
  if (!map) return
  map.eachLayer((layer) => {
    if (layer instanceof L.TileLayer) map?.removeLayer(layer)
  })
  const tileUrl = dark
    ? 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png'
    : 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png'
  L.tileLayer(tileUrl, { attribution: '© OpenStreetMap', maxZoom: 19 }).addTo(map)
})
</script>

<template>
  <div class="space-y-3">
    <label v-if="label" class="block text-sm font-medium text-gray-700 dark:text-gray-300">
      {{ label }}
    </label>

    <!-- Search Input -->
    <div class="relative">
      <input
        v-model="addressSearch"
        type="text"
        class="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        placeholder="Wpisz adres, np. Rynek Główny 1, Kraków..."
        autocomplete="off"
        @input="handleInput"
        @blur="hideSuggestions"
        @focus="showSuggestions = suggestions.length > 0"
      >

      <!-- Loading indicator -->
      <div v-if="isSearching" class="absolute right-3 top-1/2 -translate-y-1/2">
        <svg class="animate-spin h-5 w-5 text-gray-400" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
        </svg>
      </div>

      <!-- Suggestions Dropdown -->
      <div
        v-if="showSuggestions && suggestions.length > 0"
        class="absolute z-50 w-full mt-1 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-600 rounded-lg shadow-lg max-h-60 overflow-y-auto"
      >
        <button
          v-for="s in suggestions"
          :key="s.place_id"
          type="button"
          class="w-full px-4 py-3 text-left hover:bg-blue-50 dark:hover:bg-gray-700 border-b border-gray-100 dark:border-gray-700 last:border-0"
          @mousedown.prevent="selectSuggestion(s)"
        >
          <div class="flex items-start gap-2">
            <svg class="w-5 h-5 text-red-500 mt-0.5 flex-shrink-0" fill="currentColor" viewBox="0 0 24 24">
              <path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z"/>
            </svg>
            <span class="text-sm text-gray-700 dark:text-gray-200">{{ s.display_name }}</span>
          </div>
        </button>
      </div>
    </div>

    <!-- Map -->
    <div
      ref="mapContainer"
      class="h-72 rounded-lg border border-gray-300 dark:border-gray-600 overflow-hidden"
    />

    <p class="text-xs text-gray-500 dark:text-gray-400">
      Wpisz adres i wybierz z listy, lub kliknij na mapie aby ustawić lokalizację
    </p>

    <!-- Selected coordinates info -->
    <div
      v-if="latitude && longitude"
      class="flex items-center gap-2 p-2 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg text-sm text-green-700 dark:text-green-300"
    >
      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
      </svg>
      Lokalizacja ustawiona
    </div>
  </div>
</template>

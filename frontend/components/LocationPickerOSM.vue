<script setup lang="ts">
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'

interface Props {
  modelValue: string // address
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

const locationsStore = useLocationsStore()
const { isOnline } = useOnlineStatus()

const mapContainer = ref<HTMLDivElement | null>(null)
const selectedCachedLocation = ref<number | null>(null)
const coordinatesInput = ref('')
const coordinatesError = ref<string | null>(null)
const addressSearch = ref('')
const isSearching = ref(false)
const searchError = ref<string | null>(null)

interface NominatimResult {
  place_id: number
  display_name: string
  lat: string
  lon: string
}

const suggestions = ref<NominatimResult[]>([])
const showSuggestions = ref(false)
const isLoadingSuggestions = ref(false)
let searchDebounceTimer: ReturnType<typeof setTimeout> | null = null

let map: L.Map | null = null
let marker: L.Marker | null = null

const isDark = computed(() => {
  if (import.meta.client) {
    return document.documentElement.classList.contains('dark')
  }
  return false
})

onMounted(async () => {
  if (!import.meta.client || !mapContainer.value) return

  // Load cached locations
  await locationsStore.fetchCachedLocations()

  // Initialize map
  const defaultLat = props.latitude || 52.2297 // Warsaw
  const defaultLng = props.longitude || 21.0122
  
  map = L.map(mapContainer.value).setView([defaultLat, defaultLng], 13)

  // Add tile layer with dark mode support
  const tileUrl = isDark.value
    ? 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png'
    : 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png'

  L.tileLayer(tileUrl, {
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
    maxZoom: 19
  }).addTo(map)

  // Add marker if coordinates are provided
  if (props.latitude && props.longitude) {
    addMarker(props.latitude, props.longitude)
  }

  // Handle map clicks
  map.on('click', async (e: L.LeafletMouseEvent) => {
    const lat = e.latlng.lat
    const lng = e.latlng.lng
    
    addMarker(lat, lng)
    emit('update:latitude', lat)
    emit('update:longitude', lng)

    // Reverse geocode if online
    if (isOnline.value) {
      await reverseGeocode(lat, lng)
    } else {
      emit('update:modelValue', `${lat.toFixed(6)}, ${lng.toFixed(6)}`)
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

  // Remove existing marker
  if (marker) {
    map.removeLayer(marker)
  }

  // Create custom icon to fix marker display issue
  const defaultIcon = L.icon({
    iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
    iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
    shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
    iconSize: [25, 41],
    iconAnchor: [12, 41],
    popupAnchor: [1, -34],
    shadowSize: [41, 41]
  })

  // Add new marker
  marker = L.marker([lat, lng], { icon: defaultIcon }).addTo(map)
  map.setView([lat, lng], 15)
}

async function reverseGeocode(lat: number, lng: number) {
  try {
    const result = await locationsStore.geocodeAddress(`${lat},${lng}`)
    if (result) {
      emit('update:modelValue', result.address)
    }
  } catch (error) {
    console.error('Reverse geocode error:', error)
  }
}

function parseCoordinates(input: string): { lat: number; lng: number } | null {
  coordinatesError.value = null
  
  // Try different formats:
  // 52.403204, 16.892712
  // 52.403204 16.892712
  // lat: 52.403204, lng: 16.892712
  
  // Remove common prefixes
  let cleaned = input.toLowerCase()
    .replace(/lat(itude)?:/g, '')
    .replace(/lng|lon(gitude)?:/g, '')
    .trim()
  
  // Split by comma or space
  const parts = cleaned.split(/[,\s]+/).filter(p => p.length > 0)
  
  if (parts.length !== 2) {
    coordinatesError.value = 'Nieprawidłowy format. Użyj: "52.403204, 16.892712"'
    return null
  }
  
  const lat = parseFloat(parts[0] || '0')
  const lng = parseFloat(parts[1] || '0')
  
  if (isNaN(lat) || isNaN(lng)) {
    coordinatesError.value = 'Nieprawidłowe współrzędne'
    return null
  }
  
  if (lat < -90 || lat > 90 || lng < -180 || lng > 180) {
    coordinatesError.value = 'Współrzędne poza zakresem'
    return null
  }
  
  return { lat, lng }
}

async function handleCoordinatesInput() {
  if (!coordinatesInput.value.trim()) return

  const coords = parseCoordinates(coordinatesInput.value)

  if (coords) {
    emit('update:latitude', coords.lat)
    emit('update:longitude', coords.lng)

    addMarker(coords.lat, coords.lng)
    coordinatesInput.value = ''

    // Try to get address from coordinates (reverse geocoding)
    if (isOnline.value) {
      await reverseGeocode(coords.lat, coords.lng)
    } else {
      // Offline: just use coordinates as address
      emit('update:modelValue', `${coords.lat.toFixed(6)}, ${coords.lng.toFixed(6)}`)
    }
  }
}

function selectCachedLocation(locationId: number) {
  const location = locationsStore.cachedLocations.find(l => l.id === locationId)

  if (location) {
    emit('update:modelValue', location.address)
    emit('update:latitude', location.latitude)
    emit('update:longitude', location.longitude)

    addMarker(location.latitude, location.longitude)
    selectedCachedLocation.value = locationId
  }
}

async function searchAddress() {
  if (!addressSearch.value.trim() || !isOnline.value) return

  isSearching.value = true
  searchError.value = null
  showSuggestions.value = false

  try {
    const result = await locationsStore.geocodeAddress(addressSearch.value)

    if (result) {
      emit('update:modelValue', result.address)
      emit('update:latitude', result.latitude)
      emit('update:longitude', result.longitude)

      addMarker(result.latitude, result.longitude)
      addressSearch.value = result.address
    } else {
      searchError.value = 'Nie znaleziono podanego adresu'
    }
  } catch (error) {
    searchError.value = 'Błąd podczas wyszukiwania adresu'
    console.error('Address search error:', error)
  } finally {
    isSearching.value = false
  }
}

function handleAddressInput() {
  if (searchDebounceTimer) {
    clearTimeout(searchDebounceTimer)
  }

  const query = addressSearch.value.trim()
  if (query.length < 3 || !isOnline.value) {
    suggestions.value = []
    showSuggestions.value = false
    return
  }

  searchDebounceTimer = setTimeout(() => {
    fetchSuggestions(query)
  }, 300)
}

async function fetchSuggestions(query: string) {
  isLoadingSuggestions.value = true

  try {
    const response = await $fetch<NominatimResult[]>(
      'https://nominatim.openstreetmap.org/search',
      {
        params: {
          q: query,
          format: 'json',
          countrycodes: 'pl',
          limit: 5,
          addressdetails: 1
        },
        headers: {
          'Accept-Language': 'pl'
        }
      }
    )

    suggestions.value = response
    showSuggestions.value = response.length > 0
  } catch (error) {
    console.error('Nominatim search error:', error)
    suggestions.value = []
    showSuggestions.value = false
  } finally {
    isLoadingSuggestions.value = false
  }
}

function selectSuggestion(suggestion: NominatimResult) {
  const lat = parseFloat(suggestion.lat)
  const lng = parseFloat(suggestion.lon)

  emit('update:modelValue', suggestion.display_name)
  emit('update:latitude', lat)
  emit('update:longitude', lng)

  addressSearch.value = suggestion.display_name
  addMarker(lat, lng)

  suggestions.value = []
  showSuggestions.value = false
}

function hideSuggestions() {
  setTimeout(() => {
    showSuggestions.value = false
  }, 200)
}

// Watch dark mode changes and update map tiles
watch(isDark, (newDark) => {
  if (!map) return

  // Remove all tile layers
  map.eachLayer((layer) => {
    if (layer instanceof L.TileLayer) {
      map?.removeLayer(layer)
    }
  })

  // Add new tile layer
  const tileUrl = newDark
    ? 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png'
    : 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png'

  L.tileLayer(tileUrl, {
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
    maxZoom: 19
  }).addTo(map)
})
</script>

<template>
  <div class="space-y-4">
    <label v-if="label" class="block text-sm font-medium text-gray-700 dark:text-gray-300">
      {{ label }}
    </label>

    <!-- Online/Offline Status -->
    <div
      v-if="!isOnline"
      class="flex items-center gap-2 p-3 bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-lg text-yellow-800 dark:text-yellow-200 text-sm"
    >
      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
      </svg>
      Tryb offline - dostępne tylko zapisane lokalizacje
    </div>

    <!-- Address Search with Autocomplete (Nominatim OSM) -->
    <div v-if="isOnline" class="relative">
      <div class="flex items-center justify-between mb-2">
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">
          Wyszukaj adres
        </label>
        <span class="text-xs text-gray-500 dark:text-gray-400 flex items-center gap-1">
          <svg v-if="isLoadingSuggestions" class="animate-spin h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          OpenStreetMap
        </span>
      </div>
      <div class="relative">
        <div class="flex gap-2">
          <div class="relative flex-1">
            <input
              v-model="addressSearch"
              type="text"
              class="w-full px-4 py-2.5 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="Wpisz adres, np. Rynek Główny 1, Kraków..."
              :disabled="isSearching"
              autocomplete="off"
              @input="handleAddressInput"
              @keyup.enter="searchAddress"
              @blur="hideSuggestions"
              @focus="showSuggestions = suggestions.length > 0"
            >

            <!-- Autocomplete Dropdown -->
            <div
              v-if="showSuggestions && suggestions.length > 0"
              class="absolute z-50 w-full mt-1 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-600 rounded-lg shadow-lg max-h-60 overflow-y-auto"
            >
              <button
                v-for="suggestion in suggestions"
                :key="suggestion.place_id"
                type="button"
                class="w-full px-4 py-3 text-left hover:bg-blue-50 dark:hover:bg-gray-700 border-b border-gray-100 dark:border-gray-700 last:border-b-0 transition-colors"
                @mousedown.prevent="selectSuggestion(suggestion)"
              >
                <div class="flex items-start gap-2">
                  <svg class="w-5 h-5 text-gray-400 dark:text-gray-500 mt-0.5 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                  <span class="text-sm text-gray-700 dark:text-gray-200 leading-tight">
                    {{ suggestion.display_name }}
                  </span>
                </div>
              </button>
            </div>
          </div>

          <button
            type="button"
            class="px-4 py-2.5 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors font-medium disabled:bg-gray-400 disabled:cursor-not-allowed flex items-center gap-2"
            :disabled="!addressSearch.trim() || isSearching"
            @click="searchAddress"
          >
            <svg v-if="isSearching" class="animate-spin h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
            <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            <span class="hidden sm:inline">{{ isSearching ? 'Szukam...' : 'Szukaj' }}</span>
          </button>
        </div>
      </div>
      <p class="mt-1.5 text-xs text-gray-500 dark:text-gray-400">
        Zacznij pisać - pojawią się podpowiedzi adresów z całej Polski
      </p>
      <p v-if="searchError" class="mt-1 text-xs text-red-600 dark:text-red-400">
        {{ searchError }}
      </p>
    </div>

    <!-- Cached Locations Dropdown -->
    <div>
      <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
        Wybierz z zapisanych lokalizacji
      </label>
      <select
        v-model="selectedCachedLocation"
        class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
        @change="selectedCachedLocation && selectCachedLocation(selectedCachedLocation)"
      >
        <option :value="null">-- Wybierz lokalizację --</option>
        <optgroup v-if="locationsStore.officeLocations.length > 0" label="Biura">
          <option v-for="loc in locationsStore.officeLocations" :key="loc.id" :value="loc.id">
            {{ loc.name }} - {{ loc.address }}
          </option>
        </optgroup>
        <optgroup v-if="locationsStore.conferenceRoomLocations.length > 0" label="Sale konferencyjne">
          <option v-for="loc in locationsStore.conferenceRoomLocations" :key="loc.id" :value="loc.id">
            {{ loc.name }} - {{ loc.address }}
          </option>
        </optgroup>
        <optgroup v-if="locationsStore.popularLocations.length > 0" label="Popularne">
          <option v-for="loc in locationsStore.popularLocations" :key="loc.id" :value="loc.id">
            {{ loc.name }} - {{ loc.address }}
          </option>
        </optgroup>
      </select>
    </div>

    <!-- Coordinates Input -->
    <div>
      <div class="flex items-center justify-between mb-2">
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">
          Wklej współrzędne GPS
        </label>
        <a
          href="https://www.latlong.net/"
          target="_blank"
          rel="noopener noreferrer"
          class="flex items-center gap-1 text-xs text-blue-600 dark:text-blue-400 hover:underline"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
          </svg>
          Znajdź współrzędne
        </a>
      </div>
      <div class="flex gap-2">
        <input
          v-model="coordinatesInput"
          type="text"
          class="flex-1 px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white font-mono text-sm"
          placeholder="52.403204, 16.892712"
          @keyup.enter="handleCoordinatesInput"
        >
        <button
          type="button"
          class="px-4 py-2 bg-green-500 text-white rounded-lg hover:bg-green-600 transition-colors font-medium disabled:bg-gray-400 disabled:cursor-not-allowed"
          :disabled="!coordinatesInput.trim()"
          @click="handleCoordinatesInput"
        >
          Ustaw
        </button>
      </div>
      <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
        Akceptowane formaty: "52.403204, 16.892712" lub "52.403204 16.892712" lub "lat: 52.403204, lng: 16.892712"
      </p>
      <p v-if="coordinatesError" class="mt-1 text-xs text-red-600 dark:text-red-400">
        {{ coordinatesError }}
      </p>
    </div>


    <!-- Map -->
    <div class="relative">
      <div
        ref="mapContainer"
        class="h-80 rounded-lg border border-gray-300 dark:border-gray-600 overflow-hidden"
      />
      
      <div class="mt-2 text-xs text-gray-500 dark:text-gray-400">
        Kliknij na mapie aby oznaczyć lokalizację
      </div>
    </div>

    <!-- Current Coordinates -->
    <div
      v-if="latitude && longitude"
      class="flex items-center gap-2 p-3 bg-gray-50 dark:bg-gray-800 rounded-lg text-sm"
    >
      <svg class="w-5 h-5 text-gray-600 dark:text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
      </svg>
      <span class="text-gray-700 dark:text-gray-300">
        Współrzędne: {{ latitude.toFixed(6) }}, {{ longitude.toFixed(6) }}
      </span>
    </div>
  </div>
</template>

<style scoped>
/* Fix Leaflet marker icons (they don't load correctly with bundlers) */
:deep(.leaflet-marker-icon) {
  background-image: url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==');
}
</style>


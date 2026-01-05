<script setup lang="ts">
import type { News } from '~/types'
import { useAuthStore } from '~/stores/auth'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'

const { sanitizeHtml } = useSanitize()

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const { fetchNewsById, deleteNews } = useNewsApi()

const newsId = computed(() => Number.parseInt(route.params.id as string, 10))
const news = ref<News | null>(null)
const isLoading = ref(false)
const isDeleting = ref(false)
const error = ref<string | null>(null)
const showDeleteModal = ref(false)
const mapContainer = ref<HTMLDivElement | null>(null)

// Local coordinates for map display (can be fetched from Nominatim if not in news)
const mapLat = ref<number | null>(null)
const mapLng = ref<number | null>(null)

let map: L.Map | null = null

const resolveErrorMessage = (input: unknown, fallback: string) => {
  if (input && typeof input === 'object' && 'message' in input) {
    const message = (input as { message?: unknown }).message
    if (typeof message === 'string' && message.trim().length > 0) {
      return message
    }
  }
  return fallback
}

const canManageNews = computed(() => {
  const role = authStore.user?.role
  if (!role) return false
  const normalized = role.toString().toLowerCase()
  return normalized === 'admin' || normalized === 'marketing'
})

async function loadNews() {
  isLoading.value = true
  error.value = null

  try {
    news.value = await fetchNewsById(newsId.value)

    // Try to get coordinates for map
    if (news.value.eventLatitude && news.value.eventLongitude) {
      // Use coordinates from news
      mapLat.value = news.value.eventLatitude
      mapLng.value = news.value.eventLongitude
    } else if (news.value.eventLocation) {
      // Fetch coordinates from Nominatim if only address is available
      await fetchCoordinatesFromAddress(news.value.eventLocation)
    }

    // Initialize map if we have coordinates
    if (mapLat.value && mapLng.value) {
      await nextTick()
      initializeMap()
    }
  } catch (err: unknown) {
    error.value = resolveErrorMessage(err, 'Nie udało się załadować aktualności.')
    console.error('fetchNewsById error:', err)
  } finally {
    isLoading.value = false
  }
}

async function fetchCoordinatesFromAddress(address: string) {
  try {
    const response = await $fetch<Array<{ lat: string; lon: string }>>(
      'https://nominatim.openstreetmap.org/search',
      {
        params: { q: address, format: 'json', limit: 1 },
        headers: { 'Accept-Language': 'pl' }
      }
    )
    if (response && response.length > 0) {
      mapLat.value = Number.parseFloat(response[0].lat)
      mapLng.value = Number.parseFloat(response[0].lon)
    }
  } catch (err) {
    console.error('Failed to geocode address:', err)
  }
}

function initializeMap() {
  if (!import.meta.client || !mapContainer.value || !mapLat.value || !mapLng.value) {
    return
  }

  const lat = mapLat.value
  const lng = mapLng.value

  map = L.map(mapContainer.value).setView([lat, lng], 15)

  const isDark = document.documentElement.classList.contains('dark')
  const tileUrl = isDark
    ? 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png'
    : 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png'

  L.tileLayer(tileUrl, {
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
    maxZoom: 19
  }).addTo(map)

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

  L.marker([lat, lng], { icon: defaultIcon }).addTo(map)
}

async function handleDelete() {
  if (!news.value || !canManageNews.value) {
    showDeleteModal.value = false
    return
  }

  isDeleting.value = true

  try {
    await deleteNews(news.value.id)
    router.push('/dashboard/news')
  } catch (err: unknown) {
    error.value = resolveErrorMessage(err, 'Nie udało się usunąć aktualności.')
    console.error('deleteNews error:', err)
  } finally {
    isDeleting.value = false
    showDeleteModal.value = false
  }
}

function handleEdit() {
  if (!canManageNews.value) return
  router.push(`/dashboard/news/edit/${newsId.value}`)
}

function handleBack() {
  router.push('/dashboard/news')
}

const formatDate = (dateString: string | Date) => {
  const date = typeof dateString === 'string' ? new Date(dateString) : dateString
  return new Intl.DateTimeFormat('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(date)
}

const getCategoryColor = (category: string) => {
  const colors: Record<string, string> = {
    announcement: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
    product: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
    hr: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
    tech: 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200',
    event: 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200'
  }
  return colors[category.toLowerCase()] || 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200'
}

const getCategoryLabel = (category: string) => {
  const labels: Record<string, string> = {
    announcement: 'Announcement',
    product: 'Product',
    hr: 'HR',
    tech: 'Tech',
    event: 'Event'
  }
  return labels[category.toLowerCase()] ?? category
}

const getAuthorName = (currentNews: News) => {
  if (currentNews.authorName && currentNews.authorName.trim().length > 0) {
    return currentNews.authorName
  }
  if (currentNews.author) {
    return `${currentNews.author.firstName} ${currentNews.author.lastName}`
  }
  return 'Unknown author'
}

const sanitizedContent = computed(() => news.value ? sanitizeHtml(news.value.content) : '')

onMounted(() => {
  loadNews()
})

onBeforeUnmount(() => {
  if (map) {
    map.remove()
    map = null
  }
})
</script>

<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <!-- Loading State -->
    <div v-if="isLoading" class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto" />
        <p class="mt-4 text-gray-600 dark:text-gray-400">
          Ładowanie szczegółów aktualności...
        </p>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div class="bg-red-100 dark:bg-red-900 rounded-lg shadow-md p-6">
        <p class="text-red-800 dark:text-red-200">{{ error }}</p>
        <div class="mt-4 flex gap-2">
          <button class="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 font-medium" @click="loadNews">
            Spróbuj ponownie
          </button>
          <button class="px-4 py-2 bg-gray-600 text-white rounded hover:bg-gray-700 font-medium" @click="handleBack">
            Powrót do listy
          </button>
        </div>
      </div>
    </div>

    <!-- News Content -->
    <template v-else-if="news">
      <!-- Hero Image (Full Width) -->
      <div v-if="news.imageUrl" class="w-full h-96 overflow-hidden bg-gray-200 dark:bg-gray-800">
        <img
          :src="news.imageUrl"
          :alt="news.title"
          class="w-full h-full object-cover"
        >
      </div>

      <!-- Breadcrumb -->
      <div class="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <button
            class="flex items-center gap-2 text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white transition text-sm"
            @click="handleBack"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
            </svg>
            Powrót do listy newsów
          </button>
        </div>
      </div>

      <!-- Main Content Area -->
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div class="flex flex-col lg:flex-row gap-8">
          <!-- Sidebar (Mobile: Top, Desktop: Left) -->
          <aside class="w-full lg:w-80 flex-shrink-0 space-y-6">
            <!-- Meta Card -->
            <div class="bg-white dark:bg-gray-800 rounded-xl shadow-md p-6 space-y-4 lg:sticky lg:top-6">
              <h3 class="text-lg font-semibold text-gray-900 dark:text-white border-b border-gray-200 dark:border-gray-700 pb-3">
                Informacje
              </h3>

              <!-- Category & Event Badge -->
              <div class="flex flex-wrap gap-2">
                <span
                  :class="getCategoryColor(news.category)"
                  class="px-3 py-1 text-xs font-medium rounded-full"
                >
                  {{ getCategoryLabel(news.category) }}
                </span>
                <span
                  v-if="news.isEvent"
                  class="px-3 py-1 text-xs font-medium rounded-full bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200"
                >
                  📅 Wydarzenie
                </span>
              </div>

              <!-- Author -->
              <div class="flex items-start gap-3">
                <svg class="w-5 h-5 text-gray-400 dark:text-gray-500 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
                <div>
                  <p class="text-xs text-gray-500 dark:text-gray-400">Autor</p>
                  <p class="text-sm font-medium text-gray-900 dark:text-white">{{ getAuthorName(news) }}</p>
                </div>
              </div>

              <!-- Created Date -->
              <div class="flex items-start gap-3">
                <svg class="w-5 h-5 text-gray-400 dark:text-gray-500 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                <div>
                  <p class="text-xs text-gray-500 dark:text-gray-400">Opublikowano</p>
                  <p class="text-sm font-medium text-gray-900 dark:text-white">{{ formatDate(news.createdAt) }}</p>
                </div>
              </div>

              <!-- Updated Date -->
              <div v-if="news.updatedAt" class="flex items-start gap-3">
                <svg class="w-5 h-5 text-gray-400 dark:text-gray-500 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                </svg>
                <div>
                  <p class="text-xs text-gray-500 dark:text-gray-400">Zaktualizowano</p>
                  <p class="text-sm font-medium text-gray-900 dark:text-white">{{ formatDate(news.updatedAt) }}</p>
                </div>
              </div>

              <!-- Views -->
              <div class="flex items-start gap-3">
                <svg class="w-5 h-5 text-gray-400 dark:text-gray-500 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                </svg>
                <div>
                  <p class="text-xs text-gray-500 dark:text-gray-400">Wyświetlenia</p>
                  <p class="text-sm font-medium text-gray-900 dark:text-white">{{ news.views }}</p>
                </div>
              </div>

              <!-- Hashtags -->
              <div v-if="news.hashtags && news.hashtags.length > 0">
                <p class="text-xs text-gray-500 dark:text-gray-400 mb-2">Hashtagi</p>
                <HashtagDisplay :hashtags="news.hashtags" :clickable="true" size="sm" />
              </div>
            </div>
          </aside>

          <!-- Main Content -->
          <article class="flex-1 min-w-0">
            <div class="bg-white dark:bg-gray-800 rounded-xl shadow-md p-8 lg:p-12 space-y-8">
              <!-- Title -->
              <h1 class="text-4xl lg:text-5xl font-bold text-gray-900 dark:text-white leading-tight">
                {{ news.title }}
              </h1>

              <!-- Excerpt -->
              <div v-if="news.excerpt" class="text-xl text-gray-700 dark:text-gray-300 leading-relaxed italic border-l-4 border-blue-500 pl-6">
                {{ news.excerpt }}
              </div>

              <!-- Content -->
              <div
                class="prose prose-lg dark:prose-invert max-w-none text-gray-900 dark:text-gray-100"
                v-html="sanitizedContent"
              />

              <!-- Event Details -->
              <div v-if="news.isEvent" class="pt-8 border-t border-gray-200 dark:border-gray-700">
                <div class="bg-gradient-to-br from-blue-50 to-indigo-50 dark:from-blue-900/20 dark:to-indigo-900/20 rounded-xl p-6 space-y-6">
                  <div class="flex items-center gap-3 mb-4">
                    <div class="p-3 bg-blue-500 dark:bg-blue-600 rounded-lg">
                      <svg class="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                      </svg>
                    </div>
                    <h2 class="text-2xl font-bold text-blue-700 dark:text-blue-400">
                      Szczegóły wydarzenia
                    </h2>
                  </div>

                  <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <!-- Event DateTime -->
                    <div v-if="news.eventDateTime" class="flex items-start gap-3">
                      <svg class="w-6 h-6 text-blue-600 dark:text-blue-400 mt-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                      <div>
                        <p class="text-sm font-medium text-gray-700 dark:text-gray-300">Data i godzina</p>
                        <p class="text-lg font-semibold text-gray-900 dark:text-gray-100">{{ formatDate(news.eventDateTime) }}</p>
                      </div>
                    </div>

                    <!-- Event Hashtag -->
                    <div v-if="news.eventHashtag" class="flex items-start gap-3">
                      <svg class="w-6 h-6 text-blue-600 dark:text-blue-400 mt-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 20l4-16m2 16l4-16M6 9h14M4 15h14" />
                      </svg>
                      <div>
                        <p class="text-sm font-medium text-gray-700 dark:text-gray-300">Hashtag wydarzenia</p>
                        <p class="text-lg font-semibold text-blue-600 dark:text-blue-400">{{ news.eventHashtag }}</p>
                      </div>
                    </div>
                  </div>

                  <!-- Location with Map -->
                  <div v-if="news.eventLocation || mapLat" class="space-y-4">
                    <!-- Address -->
                    <div v-if="news.eventLocation" class="flex items-start gap-3">
                      <svg class="w-6 h-6 text-blue-600 dark:text-blue-400 mt-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                      </svg>
                      <div class="flex-1">
                        <p class="text-sm font-medium text-gray-700 dark:text-gray-300">Lokalizacja</p>
                        <p class="text-lg font-semibold text-gray-900 dark:text-gray-100">{{ news.eventLocation }}</p>

                        <!-- Google Maps Link -->
                        <a
                          v-if="mapLat && mapLng"
                          :href="`https://www.google.com/maps/search/?api=1&query=${mapLat},${mapLng}`"
                          target="_blank"
                          rel="noopener noreferrer"
                          class="inline-flex items-center gap-1 mt-2 text-sm text-blue-600 dark:text-blue-400 hover:underline"
                        >
                          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
                          </svg>
                          Otwórz w Google Maps
                        </a>
                      </div>
                    </div>

                    <!-- Map -->
                    <div
                      v-if="mapLat && mapLng"
                      ref="mapContainer"
                      class="h-80 rounded-lg overflow-hidden border border-gray-300 dark:border-gray-600"
                    />
                  </div>

                  <!-- Event Link -->
                  <div v-if="news.eventId" class="pt-4">
                    <button
                      class="w-full flex items-center justify-center gap-2 px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg transition-colors shadow-md"
                      @click="router.push(`/dashboard/calendar?eventId=${news.eventId}`)"
                    >
                      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                      </svg>
                      Zobacz powiązane wydarzenie w kalendarzu
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </article>
        </div>
      </div>

      <!-- Floating Action Buttons (FAB) -->
      <div v-if="canManageNews" class="fixed bottom-8 right-8 flex flex-col gap-3 z-40">
        <!-- Edit Button -->
        <button
          class="group relative flex items-center justify-center w-14 h-14 bg-gradient-to-br from-blue-500 to-blue-600 hover:from-blue-600 hover:to-blue-700 text-white rounded-full shadow-lg hover:shadow-xl transition-all duration-200 transform hover:scale-110"
          @click="handleEdit"
        >
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
          </svg>
          <!-- Tooltip -->
          <span class="absolute right-full mr-3 px-3 py-1 bg-gray-900 text-white text-sm rounded-lg opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap pointer-events-none">
            Edytuj
          </span>
        </button>

        <!-- Delete Button -->
        <button
          class="group relative flex items-center justify-center w-14 h-14 bg-gradient-to-br from-red-500 to-red-600 hover:from-red-600 hover:to-red-700 text-white rounded-full shadow-lg hover:shadow-xl transition-all duration-200 transform hover:scale-110"
          @click="showDeleteModal = true"
        >
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
          </svg>
          <!-- Tooltip -->
          <span class="absolute right-full mr-3 px-3 py-1 bg-gray-900 text-white text-sm rounded-lg opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap pointer-events-none">
            Usuń
          </span>
        </button>
      </div>
    </template>

    <!-- Delete Confirmation Modal -->
    <Teleport to="body">
      <div
        v-if="showDeleteModal"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm"
        @click.self="showDeleteModal = false"
      >
        <div class="bg-white dark:bg-gray-800 rounded-xl shadow-2xl max-w-md w-full p-6 transform transition-all">
          <div class="flex items-center gap-3 mb-4">
            <div class="p-3 bg-red-100 dark:bg-red-900 rounded-full">
              <svg class="w-6 h-6 text-red-600 dark:text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
              </svg>
            </div>
            <h3 class="text-xl font-bold text-gray-900 dark:text-white">
              Potwierdź usunięcie
            </h3>
          </div>
          
          <p class="text-gray-600 dark:text-gray-400 mb-6">
            Czy na pewno chcesz usunąć tę aktualność? Ta operacja jest nieodwracalna.
          </p>
          
          <div class="flex gap-3 justify-end">
            <button
              class="px-4 py-2 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-300 dark:hover:bg-gray-600 transition font-medium"
              :disabled="isDeleting"
              @click="showDeleteModal = false"
            >
              Anuluj
            </button>
            <button
              class="px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 disabled:bg-gray-400 disabled:cursor-not-allowed transition font-medium"
              :disabled="isDeleting"
              @click="handleDelete"
            >
              {{ isDeleting ? 'Usuwanie...' : 'Usuń' }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

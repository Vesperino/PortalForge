 <script setup lang="ts">
import type { News, NewsCategory } from '~/types'
import { useAuthStore } from '~/stores/auth'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

const authStore = useAuthStore()
const { fetchAllNews, deleteNews } = useNewsApi()

const selectedCategory = ref<string>('all')
const selectedDepartment = ref<number | undefined>(undefined)
const showOnlyEvents = ref<boolean>(false)
const selectedHashtags = ref<string[]>([])
const searchQuery = ref<string>('')
const currentPage = ref(1)
const allNews = ref<News[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)
const deleteError = ref<string | null>(null)
const showDeleteModal = ref(false)
const isDeleting = ref(false)
const newsPendingDeletion = ref<News | null>(null)
const showFilters = ref(true)
const windowWidth = ref(0)

// Responsive items per page
const itemsPerPage = computed(() => {
  if (import.meta.client) {
    if (windowWidth.value < 768) return 6  // Mobile: 2 rows × 3 columns
    if (windowWidth.value < 1024) return 8  // Tablet: 4 rows × 2 columns
    return 12  // Desktop: 4 rows × 3 columns
  }
  return 12
})

const categories = [
  { value: 'all', label: 'Wszystkie' },
  { value: 'announcement', label: 'Ogłoszenia' },
  { value: 'hr', label: 'HR' },
  { value: 'product', label: 'Produkt' },
  { value: 'tech', label: 'Technologia' },
  { value: 'event', label: 'Wydarzenia' }
]

const resolveErrorMessage = (input: unknown, fallback: string) => {
  if (input && typeof input === 'object' && 'message' in input) {
    const message = (input as { message?: unknown }).message
    if (typeof message === 'string' && message.trim().length > 0) {
      return message
    }
  }

  return fallback
}

async function loadNews() {
  isLoading.value = true
  error.value = null
  deleteError.value = null

  try {
    const category = selectedCategory.value !== 'all' ? selectedCategory.value as NewsCategory : undefined
    const hashtagsArray = selectedHashtags.value.length > 0 ? selectedHashtags.value : undefined
    const newsData = await fetchAllNews(category, selectedDepartment.value, showOnlyEvents.value ? true : undefined, hashtagsArray)
    allNews.value = newsData
  } catch (err: unknown) {
    error.value = resolveErrorMessage(err, 'Nie udało się załadować aktualności.')
    console.error('fetchAllNews error:', err)
  } finally {
    isLoading.value = false
  }
}

const updateWindowWidth = () => {
  if (import.meta.client) {
    windowWidth.value = window.innerWidth
  }
}

onMounted(() => {
  updateWindowWidth()
  if (import.meta.client) {
    window.addEventListener('resize', updateWindowWidth)
  }
  loadNews()
})

onUnmounted(() => {
  if (import.meta.client) {
    window.removeEventListener('resize', updateWindowWidth)
  }
})

watch([selectedCategory, selectedDepartment, showOnlyEvents, selectedHashtags], () => {
  loadNews()
})

const filteredNews = computed(() => {
  if (!searchQuery.value) {
    return allNews.value
  }

  const query = searchQuery.value.toLowerCase()
  return allNews.value.filter(news =>
    news.title.toLowerCase().includes(query) ||
    news.excerpt?.toLowerCase().includes(query) ||
    news.content.toLowerCase().includes(query)
  )
})

const canManageNews = computed(() => {
  const role = authStore.user?.role
  if (!role) {
    return false
  }

  const normalized = role.toString().toLowerCase()
  return normalized === 'admin' || normalized === 'marketing'
})

const totalPages = computed(() => Math.ceil(filteredNews.value.length / itemsPerPage.value))

const paginatedNews = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage.value
  const end = start + itemsPerPage.value
  return filteredNews.value.slice(start, end)
})

const goToPage = (page: number) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }
}

watch([searchQuery], () => {
  currentPage.value = 1
})

const formatDate = (dateString: string | Date) => {
  const date = typeof dateString === 'string' ? new Date(dateString) : dateString
  return new Intl.DateTimeFormat('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  }).format(date)
}

const getCategoryColor = (category: string) => {
  const colors: Record<string, string> = {
    'announcement': 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
    'Announcement': 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
    'product': 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
    'Product': 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
    'hr': 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
    'Hr': 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
    'tech': 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200',
    'Tech': 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200',
    'event': 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200',
    'Event': 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200'
  }
  return colors[category] || 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200'
}

const getCategoryLabel = (category: string) => {
  const labels: Record<string, string> = {
    'announcement': 'Announcement',
    'Announcement': 'Announcement',
    'product': 'Product',
    'Product': 'Product',
    'hr': 'HR',
    'Hr': 'HR',
    'tech': 'Tech',
    'Tech': 'Tech',
    'event': 'Event',
    'Event': 'Event'
  }
  return labels[category] || category
}

const thisMonthCount = computed(() => {
  const now = new Date()
  return allNews.value.filter(n => {
    const createdDate = new Date(n.createdAt)
    return createdDate.getMonth() === now.getMonth() && createdDate.getFullYear() === now.getFullYear()
  }).length
})

const getAuthorName = (news: News) => {
  if (news.author) {
    return `${news.author.firstName} ${news.author.lastName}`
  }
  return 'Unknown'
}

const confirmDelete = (news: News) => {
  deleteError.value = null
  newsPendingDeletion.value = news
  showDeleteModal.value = true
}

const closeDeleteModal = () => {
  showDeleteModal.value = false
  newsPendingDeletion.value = null
}

const handleDelete = async () => {
  if (!newsPendingDeletion.value || !canManageNews.value) {
    closeDeleteModal()
    return
  }

  isDeleting.value = true
  deleteError.value = null

  try {
    await deleteNews(newsPendingDeletion.value.id)
    const deletedId = newsPendingDeletion.value.id
    allNews.value = allNews.value.filter(n => n.id !== deletedId)

    if (filteredNews.value.length === 0 && currentPage.value > 1) {
      currentPage.value = Math.max(1, currentPage.value - 1)
    }

    closeDeleteModal()
  } catch (err: unknown) {
    deleteError.value = resolveErrorMessage(err, 'Nie udało się usunąć aktualności.')
    console.error('deleteNews error:', err)
  } finally {
    isDeleting.value = false
  }
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="sticky top-0 z-10 bg-gray-50 dark:bg-gray-900 py-4 -mx-6 px-6 border-b border-gray-200 dark:border-gray-700 mb-6">
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div class="flex-1">
          <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-2">
            News
          </h1>
          <!-- Compact Statistics -->
          <div class="flex flex-wrap items-center gap-3 sm:gap-4 text-xs sm:text-sm">
            <span class="flex items-center gap-1.5 text-gray-600 dark:text-gray-400">
              <span class="font-semibold text-gray-900 dark:text-white">{{ allNews.length }}</span>
              <span class="hidden sm:inline">wszystkich</span>
              <span class="sm:hidden">wsz.</span>
            </span>
            <span class="hidden sm:inline text-gray-300 dark:text-gray-600">|</span>
            <span class="flex items-center gap-1.5 text-gray-600 dark:text-gray-400">
              <span class="font-semibold text-gray-900 dark:text-white">{{ filteredNews.length }}</span>
              <span class="hidden md:inline">przefiltrowanych</span>
              <span class="hidden sm:inline md:hidden">filt.</span>
              <span class="sm:hidden">filt.</span>
            </span>
            <span class="hidden sm:inline text-gray-300 dark:text-gray-600">|</span>
            <span class="flex items-center gap-1.5 text-gray-600 dark:text-gray-400">
              <span class="font-semibold text-gray-900 dark:text-white">{{ thisMonthCount }}</span>
              <span class="hidden lg:inline">w tym miesiącu</span>
              <span class="hidden sm:inline lg:hidden">ten m.</span>
              <span class="sm:hidden">m.</span>
            </span>
          </div>
        </div>
        <NuxtLink
          v-if="canManageNews"
          to="/dashboard/news/create"
          class="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors text-center text-sm font-medium whitespace-nowrap"
        >
          + Dodaj aktualność
        </NuxtLink>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto"/>
      <p class="mt-4 text-gray-600 dark:text-gray-400">Ładowanie aktualności...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="bg-red-100 dark:bg-red-900 rounded-lg shadow-md p-6">
      <p class="text-red-800 dark:text-red-200">{{ error }}</p>
      <button class="mt-4 px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700" @click="loadNews">
        Spróbuj ponownie
      </button>
    </div>

    <div v-else>
      <div
        v-if="deleteError"
        class="mb-4 bg-red-100 dark:bg-red-900 rounded-lg shadow-md p-4 text-red-800 dark:text-red-200"
      >
        {{ deleteError }}
      </div>

      <!-- Filters -->
      <div class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
        <!-- Filter Header (Mobile Collapsible) -->
        <button
          class="md:hidden w-full px-4 py-3 flex items-center justify-between text-left hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
          @click="showFilters = !showFilters"
        >
          <span class="font-medium text-gray-900 dark:text-white">Filtry</span>
          <svg
            class="w-5 h-5 text-gray-500 dark:text-gray-400 transition-transform"
            :class="{ 'rotate-180': showFilters }"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
          </svg>
        </button>
        
        <!-- Filter Content -->
        <div :class="['p-4', { 'hidden md:block': !showFilters }, 'md:block']">
          <div class="space-y-4">
            <!-- First Row: Search, Category, Department, Events Toggle -->
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              <!-- Search -->
              <div>
                <label for="search" class="flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                  </svg>
                  Szukaj
                </label>
                <div class="relative">
                  <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                    </svg>
                  </div>
                  <input
                    id="search"
                    v-model="searchQuery"
                    type="text"
                    placeholder="Szukaj newsów..."
                    class="pl-10 w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                  >
                </div>
              </div>

              <!-- Category Filter -->
              <div>
                <label for="category" class="flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
                  </svg>
                  Kategoria
                </label>
                <select
                  id="category"
                  v-model="selectedCategory"
                  class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                >
                  <option v-for="cat in categories" :key="cat.value" :value="cat.value">
                    {{ cat.label }}
                  </option>
                </select>
              </div>

              <!-- Department Filter -->
              <div>
                <label for="department" class="flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                  </svg>
                  Dział
                </label>
                <select
                  id="department"
                  v-model="selectedDepartment"
                  class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                >
                  <option :value="undefined">Wszystkie działy</option>
                  <option :value="1">Zarząd</option>
                  <option :value="2">IT</option>
                  <option :value="3">HR</option>
                  <option :value="4">Marketing</option>
                  <option :value="5">Finanse</option>
                  <option :value="6">Produkt</option>
                </select>
              </div>

              <!-- Events Only Toggle -->
              <div class="flex items-end">
                <label class="flex items-center gap-2 cursor-pointer">
                  <input
                    v-model="showOnlyEvents"
                    type="checkbox"
                    class="w-5 h-5 text-blue-600 bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600 rounded focus:ring-blue-500 focus:ring-2"
                  >
                  <span class="text-sm font-medium text-gray-700 dark:text-gray-300 flex items-center gap-1.5">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                    </svg>
                    Tylko wydarzenia
                  </span>
                </label>
              </div>
            </div>

            <!-- Second Row: Hashtags Filter -->
            <div>
              <HashtagInput
                v-model="selectedHashtags"
                label="Filtruj po hashtagach"
                placeholder="Wpisz hashtagi do filtrowania..."
                :allow-custom="true"
              />
            </div>
          </div>
        </div>
      </div>

      <!-- News List -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mt-6">
        <!-- Empty state -->
        <div v-if="paginatedNews.length === 0" class="col-span-full bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center">
          <svg class="w-16 h-16 mx-auto text-gray-400 dark:text-gray-600 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 20H5a2 2 0 01-2-2V6a2 2 0 012-2h10a2 2 0 012 2v1m2 13a2 2 0 01-2-2V7m2 13a2 2 0 002-2V9a2 2 0 00-2-2h-2m-4-3H9M7 16h6M7 8h6v4H7V8z" />
          </svg>
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
            Brak aktualności
          </h3>
          <p class="text-gray-600 dark:text-gray-400">
            Brak aktualności spełniających wybrane filtry.
          </p>
        </div>

        <article
          v-for="news in paginatedNews"
          :key="news.id"
          class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden hover:shadow-xl hover:-translate-y-1 transition-all cursor-pointer flex flex-col"
          @click="navigateTo(`/dashboard/news/${news.id}`)"
        >
          <!-- News Image -->
          <div v-if="news.imageUrl" class="h-48 overflow-hidden bg-gray-100 dark:bg-gray-700">
            <img
              :src="news.imageUrl"
              :alt="news.title"
              class="w-full h-full object-cover"
            >
          </div>

          <div class="p-4 flex-1 flex flex-col">
            <!-- News Header -->
            <div class="flex-1">
              <div class="flex items-center gap-2 mb-2 flex-wrap">
                <span
                  :class="getCategoryColor(news.category)"
                  class="px-2 py-0.5 text-xs font-medium rounded-full"
                >
                  {{ getCategoryLabel(news.category) }}
                </span>
                <span v-if="news.isEvent" class="px-2 py-0.5 text-xs font-medium rounded-full bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                  📅 Wydarzenie
                </span>
                <span v-if="news.eventHashtag" class="px-2 py-0.5 text-xs font-medium rounded-full bg-blue-50 text-blue-700 dark:bg-blue-950 dark:text-blue-300">
                  {{ news.eventHashtag }}
                </span>
              </div>
              <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-2 line-clamp-2">
                {{ news.title }}
              </h2>
              
              <!-- News Excerpt -->
              <p class="text-sm text-gray-600 dark:text-gray-400 leading-relaxed mb-3 line-clamp-2">
                {{ news.excerpt }}
              </p>
              
              <!-- Compact Metadata -->
              <div class="flex items-center gap-3 text-xs text-gray-500 dark:text-gray-400 flex-wrap">
                <span class="flex items-center gap-1">
                  <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                  </svg>
                  <span class="truncate">{{ getAuthorName(news) }}</span>
                </span>
                <span class="flex items-center gap-1">
                  <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                  {{ formatDate(news.createdAt) }}
                </span>
                <span class="flex items-center gap-1">
                  <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                  </svg>
                  {{ news.views }}
                </span>
              </div>
            </div>

            <!-- Event Details Preview -->
            <div v-if="news.isEvent && (news.eventDateTime || news.eventLocation)" class="mt-3 p-2 bg-blue-50 dark:bg-blue-900/20 rounded-md border border-blue-200 dark:border-blue-800">
              <div class="space-y-1.5 text-xs">
                <div v-if="news.eventDateTime" class="flex items-center gap-1.5 text-gray-700 dark:text-gray-300">
                  <svg class="w-3.5 h-3.5 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <span class="truncate">{{ formatDate(news.eventDateTime) }}</span>
                </div>
                <div v-if="news.eventLocation" class="flex items-center gap-1.5 text-gray-700 dark:text-gray-300">
                  <svg class="w-3.5 h-3.5 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                  <span class="truncate">{{ news.eventLocation }}</span>
                </div>
              </div>
            </div>

            <!-- News Actions -->
            <div class="flex items-center justify-between pt-3 mt-auto border-t border-gray-200 dark:border-gray-700">
              <NuxtLink
                :to="`/dashboard/news/${news.id}`"
                class="text-xs font-medium text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300 transition-colors focus:outline-none focus:ring-2 focus:ring-blue-500 rounded px-2 py-1"
                @click.stop
              >
                Czytaj więcej →
              </NuxtLink>
              <div v-if="canManageNews" class="flex items-center gap-2">
                <NuxtLink
                  :to="`/dashboard/news/edit/${news.id}`"
                  class="text-xs text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200 transition-colors px-2 py-1 rounded"
                  @click.stop
                >
                  Edytuj
                </NuxtLink>
                <button
                  type="button"
                  class="text-xs text-red-600 dark:text-red-400 hover:text-red-700 dark:hover:text-red-300 transition-colors px-2 py-1 rounded"
                  @click.stop="confirmDelete(news)"
                >
                  Usuń
                </button>
              </div>
            </div>
          </div>
        </article>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4">
        <div class="flex items-center justify-between">
          <!-- Page info -->
          <div class="text-sm text-gray-700 dark:text-gray-300">
            Strona <span class="font-medium">{{ currentPage }}</span> z <span class="font-medium">{{ totalPages }}</span>
            <span class="text-gray-500 dark:text-gray-400 ml-2">
              ({{ paginatedNews.length }} z {{ filteredNews.length }} aktualnosci)
            </span>
          </div>

          <!-- Pagination controls -->
          <div class="flex items-center gap-2">
            <!-- Previous button -->
            <button
              :disabled="currentPage === 1"
              class="px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 text-sm font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 dark:hover:bg-gray-700"
              :class="currentPage === 1 ? 'text-gray-400 dark:text-gray-600' : 'text-gray-700 dark:text-gray-300'"
              @click="goToPage(currentPage - 1)"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
              </svg>
            </button>

            <!-- Page numbers -->
            <div class="flex items-center gap-1">
              <button
                v-for="page in totalPages"
                :key="page"
                class="px-4 py-2 rounded-lg text-sm font-medium transition-colors"
                :class="page === currentPage
                  ? 'bg-blue-600 text-white'
                  : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'"
                @click="goToPage(page)"
              >
                {{ page }}
              </button>
            </div>

            <!-- Next button -->
            <button
              :disabled="currentPage === totalPages"
              class="px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 text-sm font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 dark:hover:bg-gray-700"
              :class="currentPage === totalPages ? 'text-gray-400 dark:text-gray-600' : 'text-gray-700 dark:text-gray-300'"
              @click="goToPage(currentPage + 1)"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>

    <Teleport to="body">
      <div
        v-if="showDeleteModal"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50"
        @click.self="closeDeleteModal"
      >
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full p-6">
          <h3 class="text-xl font-bold text-gray-900 dark:text-white mb-4">
            Potwierdź usunięcie
          </h3>
          <p class="text-gray-600 dark:text-gray-400 mb-6">
            Czy na pewno chcesz usunąć aktualność "{{ newsPendingDeletion?.title }}"? Ta operacja nie może być cofnięta.
          </p>
          <div class="flex gap-4 justify-end">
            <button
              type="button"
              class="px-4 py-2 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-300 dark:hover:bg-gray-600 transition font-medium"
              :disabled="isDeleting"
              @click="closeDeleteModal"
            >
              Anuluj
            </button>
            <button
              type="button"
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



















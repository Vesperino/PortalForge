<script setup lang="ts">
import type { InternalService } from '~/types/internal-services'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

const { fetchMyServices } = useInternalServicesApi()

const servicesSearch = ref('')
const selectedScope = ref<'all' | 'global' | 'department'>('all')
const selectedCategory = ref('')

const services = ref<InternalService[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

const categories = computed(() => {
  const cats = new Set<string>()
  services.value.forEach(s => {
    if (s.categoryName) cats.add(s.categoryName)
  })
  return Array.from(cats).sort()
})

const filteredServices = computed(() => {
  let filtered = services.value

  // Filter by search term
  if (servicesSearch.value.trim()) {
    const query = servicesSearch.value.toLowerCase()
    filtered = filtered.filter(service =>
      service.name.toLowerCase().includes(query) ||
      service.description.toLowerCase().includes(query) ||
      (service.categoryName && service.categoryName.toLowerCase().includes(query))
    )
  }

  // Filter by scope
  if (selectedScope.value === 'global') {
    filtered = filtered.filter(s => s.isGlobal)
  } else if (selectedScope.value === 'department') {
    filtered = filtered.filter(s => !s.isGlobal)
  }

  // Filter by category
  if (selectedCategory.value) {
    filtered = filtered.filter(s => s.categoryName === selectedCategory.value)
  }

  // Sort: pinned first, then by display order, then by name
  return filtered.sort((a, b) => {
    if (a.isPinned !== b.isPinned) return a.isPinned ? -1 : 1
    if (a.displayOrder !== b.displayOrder) return a.displayOrder - b.displayOrder
    return a.name.localeCompare(b.name)
  })
})

const groupedByCategory = computed(() => {
  const groups: Record<string, InternalService[]> = {}

  filteredServices.value.forEach(service => {
    const category = service.categoryName || 'Inne'
    if (!groups[category]) {
      groups[category] = []
    }
    groups[category].push(service)
  })

  return groups
})

async function loadServices() {
  loading.value = true
  error.value = null
  try {
    services.value = await fetchMyServices()
  } catch (err: unknown) {
    error.value = err instanceof Error ? err.message : 'Nie udało się załadować serwisów'
    console.error('Error loading services:', err)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadServices()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Serwisy wewnętrzne</h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          Szybki dostęp do wszystkich narzędzi i systemów firmowych
        </p>
      </div>
      <div v-if="!loading" class="text-sm text-gray-500 dark:text-gray-400">
        Dostępnych serwisów: <span class="font-bold text-gray-900 dark:text-white">{{ services.length }}</span>
      </div>
    </div>

    <!-- Search & Filters -->
    <div class="rounded-xl border-2 border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-6">
      <div class="space-y-4">
        <!-- Search -->
        <div class="relative">
          <svg class="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
          <input
            v-model="servicesSearch"
            type="text"
            placeholder="Szukaj serwisu po nazwie lub opisie..."
            class="w-full pl-12 pr-4 py-3 rounded-lg border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 text-base text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
          >
        </div>

        <!-- Filters -->
        <div class="flex flex-wrap gap-3">
          <!-- Scope Filter -->
          <div class="flex rounded-lg border border-gray-300 dark:border-gray-600 overflow-hidden">
            <button
              :class="selectedScope === 'all' ? 'bg-blue-600 text-white' : 'bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700'"
              class="px-4 py-2 text-sm font-medium transition-colors"
              @click="selectedScope = 'all'"
            >
              Wszystkie
            </button>
            <button
              :class="selectedScope === 'department' ? 'bg-blue-600 text-white' : 'bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700'"
              class="px-4 py-2 text-sm font-medium border-l border-gray-300 dark:border-gray-600 transition-colors"
              @click="selectedScope = 'department'"
            >
              Mój dział
            </button>
            <button
              :class="selectedScope === 'global' ? 'bg-blue-600 text-white' : 'bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700'"
              class="px-4 py-2 text-sm font-medium border-l border-gray-300 dark:border-gray-600 transition-colors"
              @click="selectedScope = 'global'"
            >
              Globalne
            </button>
          </div>

          <!-- Category Filter -->
          <select
            v-if="categories.length > 0"
            v-model="selectedCategory"
            class="px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 text-sm font-medium focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          >
            <option value="">Wszystkie kategorie</option>
            <option v-for="cat in categories" :key="cat" :value="cat">{{ cat }}</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="rounded-xl border-2 border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-12 text-center">
      <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 dark:border-blue-400"/>
      <p class="mt-4 text-gray-600 dark:text-gray-400">Ładowanie serwisów...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="rounded-xl border-2 border-red-200 dark:border-red-800 bg-red-50 dark:bg-red-900/30 p-6">
      <p class="text-red-800 dark:text-red-300">{{ error }}</p>
      <button
        class="mt-4 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
        @click="loadServices"
      >
        Spróbuj ponownie
      </button>
    </div>

    <!-- Empty State -->
    <div v-else-if="filteredServices.length === 0" class="rounded-xl border-2 border-dashed border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/50 p-12 text-center">
      <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9 3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
      </svg>
      <h3 class="mt-4 text-lg font-semibold text-gray-900 dark:text-white">Brak serwisów</h3>
      <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">
        {{ servicesSearch || selectedCategory ? 'Nie znaleziono serwisów spełniających kryteria wyszukiwania.' : 'Brak dostępnych serwisów dla Twojego działu.' }}
      </p>
    </div>

    <!-- Services Grid (Grouped by Category) -->
    <div v-else class="space-y-8">
      <div v-for="(categoryServices, categoryName) in groupedByCategory" :key="categoryName">
        <h2 class="text-xl font-bold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
          <span>{{ categoryName }}</span>
          <span class="text-sm font-normal text-gray-500 dark:text-gray-400">({{ categoryServices.length }})</span>
        </h2>

        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <a
            v-for="service in categoryServices"
            :key="service.id"
            :href="service.url"
            target="_blank"
            rel="noopener noreferrer"
            class="group rounded-xl border-2 border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-6 transition-all hover:border-blue-500 hover:shadow-xl hover:-translate-y-1"
          >
            <div class="flex items-start gap-4">
              <!-- Icon -->
              <div class="flex-shrink-0 w-14 h-14 rounded-xl bg-gradient-to-br from-blue-50 to-blue-100 dark:from-blue-900/30 dark:to-blue-800/30 flex items-center justify-center text-3xl group-hover:scale-110 transition-transform">
                <span v-if="service.iconType === 'emoji'">{{ service.icon || '🔗' }}</span>
                <img v-else-if="service.iconType === 'image'" :src="service.icon" alt="" class="w-10 h-10 rounded" >
                <i v-else :class="service.icon" class="text-blue-600 dark:text-blue-400"/>
              </div>

              <!-- Content -->
              <div class="flex-1 min-w-0">
                <div class="flex items-start justify-between gap-2 mb-2">
                  <div class="flex items-center gap-2">
                    <h3 class="text-lg font-bold text-gray-900 dark:text-white group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">
                      {{ service.name }}
                    </h3>
                    <span v-if="service.isPinned" class="text-lg" title="Przypięty">📌</span>
                  </div>
                  <svg class="flex-shrink-0 h-5 w-5 text-gray-400 group-hover:text-blue-500 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
                  </svg>
                </div>

                <p class="text-sm text-gray-600 dark:text-gray-400 mb-3 line-clamp-2">
                  {{ service.description }}
                </p>

                <div class="flex items-center gap-2">
                  <span
                    v-if="service.isGlobal"
                    class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300"
                  >
                    🌐 Globalny
                  </span>
                </div>

                <div class="mt-3 text-xs text-gray-500 dark:text-gray-400 truncate">
                  {{ service.url }}
                </div>
              </div>
            </div>
          </a>
        </div>
      </div>
    </div>
  </div>
</template>

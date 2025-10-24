<script setup lang="ts">
definePageMeta({
  layout: 'default',
  middleware: ['auth']
})

const { getNews, getNewsByCategory } = useMockData()

const selectedCategory = ref<string>('all')
const searchQuery = ref<string>('')

const allNews = getNews()

const categories = [
  { value: 'all', label: 'Wszystkie' },
  { value: 'announcement', label: 'Ogłoszenia' },
  { value: 'hr', label: 'HR' },
  { value: 'product', label: 'Produkt' },
  { value: 'tech', label: 'Technologia' },
  { value: 'event', label: 'Wydarzenia' }
]

const filteredNews = computed(() => {
  let filtered = selectedCategory.value === 'all'
    ? allNews
    : getNewsByCategory(selectedCategory.value)

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(news =>
      news.title.toLowerCase().includes(query) ||
      news.excerpt?.toLowerCase().includes(query) ||
      news.content.toLowerCase().includes(query)
    )
  }

  return filtered
})

const formatDate = (date: Date) => {
  return new Intl.DateTimeFormat('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  }).format(date)
}

const getCategoryColor = (category: string) => {
  const colors: Record<string, string> = {
    'announcement': 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
    'product': 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
    'hr': 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
    'tech': 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200',
    'event': 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200'
  }
  return colors[category] || 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200'
}

const getCategoryLabel = (category: string) => {
  const labels: Record<string, string> = {
    'announcement': 'Ogłoszenie',
    'product': 'Produkt',
    'hr': 'HR',
    'tech': 'Tech',
    'event': 'Event'
  }
  return labels[category] || category
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
        Aktualności
      </h1>
      <BaseButton variant="primary">
        Dodaj aktualność
      </BaseButton>
    </div>

    <!-- Filters -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <!-- Search -->
        <div>
          <label for="search" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
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
              placeholder="Szukaj aktualności..."
              class="pl-10 w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            >
          </div>
        </div>

        <!-- Category Filter -->
        <div>
          <label for="category" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
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
      </div>
    </div>

    <!-- News Stats -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4">
        <p class="text-sm text-gray-600 dark:text-gray-400">Wszystkie aktualności</p>
        <p class="text-2xl font-bold text-gray-900 dark:text-white">{{ allNews.length }}</p>
      </div>
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4">
        <p class="text-sm text-gray-600 dark:text-gray-400">Wyświetlane</p>
        <p class="text-2xl font-bold text-gray-900 dark:text-white">{{ filteredNews.length }}</p>
      </div>
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4">
        <p class="text-sm text-gray-600 dark:text-gray-400">Ten miesiąc</p>
        <p class="text-2xl font-bold text-gray-900 dark:text-white">
          {{ allNews.filter(n => n.createdAt.getMonth() === new Date().getMonth()).length }}
        </p>
      </div>
    </div>

    <!-- News List -->
    <div class="space-y-4">
      <article
        v-for="news in filteredNews"
        :key="news.id"
        class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow cursor-pointer"
        @click="navigateTo(`/dashboard/news/${news.id}`)"
      >
        <!-- News Image -->
        <div v-if="news.imageUrl" class="h-56 overflow-hidden">
          <img
            :src="news.imageUrl"
            :alt="news.title"
            class="w-full h-full object-cover"
          >
        </div>

        <div class="p-6">
          <!-- News Header -->
          <div class="flex items-start justify-between mb-4">
            <div class="flex-1">
              <div class="flex items-center gap-2 mb-2">
                <span
                  :class="getCategoryColor(news.category)"
                  class="px-3 py-1 text-xs font-medium rounded-full"
                >
                  {{ getCategoryLabel(news.category) }}
                </span>
                <span v-if="news.event" class="px-3 py-1 text-xs font-medium rounded-full bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200">
                  Wydarzenie
                </span>
              </div>
              <h2 class="text-2xl font-semibold text-gray-900 dark:text-white mb-2">
                {{ news.title }}
              </h2>
              <div class="flex items-center gap-4 text-sm text-gray-500 dark:text-gray-400">
                <span class="flex items-center gap-1">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                  </svg>
                  {{ news.author?.firstName }} {{ news.author?.lastName }}
                </span>
                <span class="flex items-center gap-1">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                  {{ formatDate(news.createdAt) }}
                </span>
              </div>
            </div>
          </div>

          <!-- News Excerpt -->
          <p class="text-gray-700 dark:text-gray-300 leading-relaxed mb-4">
            {{ news.excerpt }}
          </p>

          <!-- News Actions -->
          <div class="flex items-center justify-between pt-4 border-t border-gray-200 dark:border-gray-700">
            <NuxtLink
              :to="`/dashboard/news/${news.id}`"
              class="text-sm text-blue-600 dark:text-blue-400 hover:underline focus:outline-none focus:ring-2 focus:ring-blue-500 rounded px-2 py-1"
            >
              Czytaj więcej →
            </NuxtLink>
            <div v-if="news.event" class="text-xs text-gray-500 dark:text-gray-400">
              Powiązane wydarzenie: {{ news.event.title }}
            </div>
          </div>
        </div>
      </article>

      <!-- Empty State -->
      <div
        v-if="filteredNews.length === 0"
        class="text-center py-12 bg-white dark:bg-gray-800 rounded-lg shadow-md"
      >
        <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 20H5a2 2 0 01-2-2V6a2 2 0 012-2h10a2 2 0 012 2v1m2 13a2 2 0 01-2-2V7m2 13a2 2 0 002-2V9a2 2 0 00-2-2h-2m-4-3H9M7 16h6M7 8h6v4H7V8z" />
        </svg>
        <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">
          Brak aktualności
        </h3>
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
          Nie znaleziono aktualności spełniających kryteria wyszukiwania.
        </p>
      </div>
    </div>
  </div>
</template>

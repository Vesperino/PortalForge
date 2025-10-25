<script setup lang="ts">
definePageMeta({
  layout: 'default'
  // middleware: ['auth'] // Disabled for testing
})

const route = useRoute()
const router = useRouter()
const { getNewsById } = useMockData()

const newsId = parseInt(route.params.id as string)
const news = getNewsById(newsId)

if (!news) {
  // Redirect to news list if news not found
  router.push('/dashboard/news')
}

const formatDate = (date: Date) => {
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

const goBack = () => {
  router.push('/dashboard/news')
}
</script>

<template>
  <div v-if="news" class="space-y-6">
    <!-- Back Button -->
    <div>
      <button
        type="button"
        class="flex items-center gap-2 text-blue-600 dark:text-blue-400 hover:underline focus:outline-none"
        @click="goBack"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
        </svg>
        Powrót do aktualności
      </button>
    </div>

    <!-- News Article -->
    <article class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden">
      <!-- Featured Image -->
      <div v-if="news.imageUrl" class="h-96 overflow-hidden">
        <img
          :src="news.imageUrl"
          :alt="news.title"
          class="w-full h-full object-cover"
        >
      </div>

      <!-- Article Content -->
      <div class="p-8">
        <!-- Meta Information -->
        <div class="flex items-center gap-3 mb-6">
          <span
            :class="getCategoryColor(news.category)"
            class="px-3 py-1 text-sm font-medium rounded-full"
          >
            {{ getCategoryLabel(news.category) }}
          </span>
          <span v-if="news.event" class="px-3 py-1 text-sm font-medium rounded-full bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200">
            Wydarzenie
          </span>
        </div>

        <!-- Title -->
        <h1 class="text-4xl font-bold text-gray-900 dark:text-white mb-4">
          {{ news.title }}
        </h1>

        <!-- Author and Date -->
        <div class="flex items-center gap-6 mb-8 pb-8 border-b border-gray-200 dark:border-gray-700">
          <div class="flex items-center gap-3">
            <div class="w-12 h-12 rounded-full bg-blue-500 flex items-center justify-center text-white font-semibold">
              {{ news.author?.firstName?.[0] }}{{ news.author?.lastName?.[0] }}
            </div>
            <div>
              <p class="font-medium text-gray-900 dark:text-white">
                {{ news.author?.firstName }} {{ news.author?.lastName }}
              </p>
              <p class="text-sm text-gray-600 dark:text-gray-400">
                {{ news.author?.position?.name }}
              </p>
            </div>
          </div>
          <div class="flex items-center gap-2 text-gray-600 dark:text-gray-400">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            <span class="text-sm">{{ formatDate(news.createdAt) }}</span>
          </div>
        </div>

        <!-- Article Content -->
        <div class="prose prose-lg dark:prose-invert max-w-none">
          <p class="text-xl text-gray-700 dark:text-gray-300 leading-relaxed mb-6">
            {{ news.excerpt }}
          </p>
          <div class="text-gray-700 dark:text-gray-300 leading-relaxed whitespace-pre-line">
            {{ news.content }}
          </div>
        </div>

        <!-- Related Event -->
        <div v-if="news.event" class="mt-8 pt-8 border-t border-gray-200 dark:border-gray-700">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
            Powiązane wydarzenie
          </h3>
          <div class="bg-gray-50 dark:bg-gray-700 rounded-lg p-6">
            <div class="flex items-start justify-between">
              <div class="flex-1">
                <h4 class="text-xl font-semibold text-gray-900 dark:text-white mb-2">
                  {{ news.event.title }}
                </h4>
                <p class="text-gray-700 dark:text-gray-300 mb-3">
                  {{ news.event.description }}
                </p>
                <div class="flex items-center gap-4 text-sm text-gray-600 dark:text-gray-400">
                  <div class="flex items-center gap-2">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                    </svg>
                    <span>{{ formatDate(news.event.startDate) }}</span>
                  </div>
                  <div v-if="news.event.location" class="flex items-center gap-2">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                    </svg>
                    <span>{{ news.event.location }}</span>
                  </div>
                </div>
                <div class="flex gap-2 mt-3">
                  <span
                    v-for="tag in news.event.tags"
                    :key="tag"
                    class="px-2 py-1 text-xs font-medium rounded-full bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200"
                  >
                    #{{ tag }}
                  </span>
                </div>
              </div>
              <NuxtLink
                to="/dashboard/calendar"
                class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors text-sm font-medium"
              >
                Zobacz w kalendarzu
              </NuxtLink>
            </div>
          </div>
        </div>
      </div>
    </article>

    <!-- Share & Actions -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
      <div class="flex items-center justify-between">
        <div>
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
            Udostępnij
          </h3>
          <p class="text-sm text-gray-600 dark:text-gray-400">
            Podziel się tą aktualnością z zespołem
          </p>
        </div>
        <div class="flex gap-2">
          <button
            type="button"
            class="px-4 py-2 bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors"
          >
            Kopiuj link
          </button>
          <button
            type="button"
            class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            Wyślij email
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

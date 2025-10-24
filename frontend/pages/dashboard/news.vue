<script setup lang="ts">
definePageMeta({
  layout: 'default',
  middleware: ['auth']
})

const newsItems = ref([
  {
    id: 1,
    title: 'Witamy w PortalForge',
    content: 'To jest pierwsza wiadomość w naszym nowym portalu.',
    author: 'Administrator',
    createdAt: new Date('2024-01-15'),
    category: 'Ogłoszenia'
  },
  {
    id: 2,
    title: 'Nowa funkcjonalność',
    content: 'Dodaliśmy możliwość przeglądania struktury organizacyjnej.',
    author: 'HR Team',
    createdAt: new Date('2024-01-14'),
    category: 'Nowości'
  }
])

const formatDate = (date: Date) => {
  return new Intl.DateTimeFormat('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  }).format(date)
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
        Aktualności
      </h1>
      <BaseButton variant="primary">
        Dodaj aktualność
      </BaseButton>
    </div>

    <!-- News List -->
    <div class="space-y-4">
      <article
        v-for="news in newsItems"
        :key="news.id"
        class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow"
      >
        <!-- News Header -->
        <div class="flex items-start justify-between mb-4">
          <div class="flex-1">
            <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-2">
              {{ news.title }}
            </h2>
            <div class="flex items-center gap-4 text-sm text-gray-500 dark:text-gray-400">
              <span class="flex items-center gap-1">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
                {{ news.author }}
              </span>
              <span class="flex items-center gap-1">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                {{ formatDate(news.createdAt) }}
              </span>
            </div>
          </div>
          <span class="px-3 py-1 text-xs font-medium rounded-full bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200">
            {{ news.category }}
          </span>
        </div>

        <!-- News Content -->
        <p class="text-gray-700 dark:text-gray-300 leading-relaxed">
          {{ news.content }}
        </p>

        <!-- News Actions -->
        <div class="flex items-center gap-2 mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
          <button
            type="button"
            class="text-sm text-blue-600 dark:text-blue-400 hover:underline focus:outline-none focus:ring-2 focus:ring-blue-500 rounded px-2 py-1"
          >
            Czytaj więcej
          </button>
        </div>
      </article>

      <!-- Empty State -->
      <div
        v-if="newsItems.length === 0"
        class="text-center py-12 bg-white dark:bg-gray-800 rounded-lg shadow-md"
      >
        <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 20H5a2 2 0 01-2-2V6a2 2 0 012-2h10a2 2 0 012 2v1m2 13a2 2 0 01-2-2V7m2 13a2 2 0 002-2V9a2 2 0 00-2-2h-2m-4-3H9M7 16h6M7 8h6v4H7V8z" />
        </svg>
        <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">
          Brak aktualności
        </h3>
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
          Nie ma jeszcze żadnych aktualności do wyświetlenia.
        </p>
      </div>
    </div>
  </div>
</template>

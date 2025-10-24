<script setup lang="ts">
definePageMeta({
  layout: 'default',
  // middleware: ['auth'] // Disabled for testing
})

const { getLatestNews, getUpcomingEvents } = useMockData()

const latestNews = getLatestNews(3)
const upcomingEvents = getUpcomingEvents(5)

const quickLinks = [
  {
    name: 'organization',
    label: 'Struktura organizacyjna',
    to: '/dashboard/organization',
    icon: 'org',
    iconColor: 'text-blue-600 dark:text-blue-400',
    iconBackground: 'bg-blue-100 dark:bg-blue-900'
  },
  {
    name: 'documents',
    label: 'Dokumenty',
    to: '/dashboard/documents',
    icon: 'documents',
    iconColor: 'text-green-600 dark:text-green-400',
    iconBackground: 'bg-green-100 dark:bg-green-900'
  },
  {
    name: 'account',
    label: 'Moje konto',
    to: '/dashboard/account',
    icon: 'account',
    iconColor: 'text-purple-600 dark:text-purple-400',
    iconBackground: 'bg-purple-100 dark:bg-purple-900'
  },
  {
    name: 'calendar',
    label: 'Kalendarz',
    to: '/dashboard/calendar',
    icon: 'calendar',
    iconColor: 'text-orange-600 dark:text-orange-400',
    iconBackground: 'bg-orange-100 dark:bg-orange-900'
  },
  {
    name: 'requests',
    label: 'Wnioski',
    to: '/dashboard/requests',
    icon: 'requests',
    iconColor: 'text-teal-600 dark:text-teal-400',
    iconBackground: 'bg-teal-100 dark:bg-teal-900'
  }
]

const formatDate = (date: Date) => {
  return new Intl.DateTimeFormat('pl-PL', {
    day: 'numeric',
    month: 'long',
    year: 'numeric'
  }).format(date)
}

const formatEventDate = (date: Date) => {
  return new Intl.DateTimeFormat('pl-PL', {
    day: 'numeric',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit'
  }).format(date)
}

const getEventTagColor = (tag: string) => {
  const colors: Record<string, string> = {
    'szkolenie': 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
    'impreza': 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
    'spotkanie': 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200',
    'meeting': 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200',
    'all-hands': 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
    'urodziny': 'bg-pink-100 text-pink-800 dark:bg-pink-900 dark:text-pink-200',
    'święto': 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200'
  }
  return colors[tag] || 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200'
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
    <!-- Welcome Section -->
    <div class="bg-gradient-to-r from-blue-600 to-blue-800 rounded-lg shadow-lg p-8 text-white">
      <h1 class="text-3xl font-bold mb-2">
        Witaj w PortalForge! 👋
      </h1>
      <p class="text-blue-100">
        {{ formatDate(new Date()) }}
      </p>
    </div>

    <!-- Quick Links -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
      <h2 class="text-xl font-bold text-gray-900 dark:text-white mb-4">
        Szybkie linki
      </h2>
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <NuxtLink
          v-for="link in quickLinks"
          :key="link.name"
          :to="link.to"
          class="flex flex-col items-center gap-3 p-4 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors text-center"
        >
          <span
            class="w-12 h-12 rounded-full flex items-center justify-center"
            :class="link.iconBackground"
          >
            <svg
              v-if="link.icon === 'org'"
              class="w-6 h-6"
              :class="link.iconColor"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
            </svg>
            <svg
              v-else-if="link.icon === 'documents'"
              class="w-6 h-6"
              :class="link.iconColor"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            <svg
              v-else-if="link.icon === 'account'"
              class="w-6 h-6"
              :class="link.iconColor"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
            <svg
              v-else-if="link.icon === 'calendar'"
              class="w-6 h-6"
              :class="link.iconColor"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            <svg
              v-else-if="link.icon === 'requests'"
              class="w-6 h-6"
              :class="link.iconColor"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m2-6h-3.586a1 1 0 01-.707-.293L12 3l-1.707 1.707A1 1 0 019.586 5H6a2 2 0 00-2 2v11a2 2 0 002 2h12a2 2 0 002-2V7a2 2 0 00-2-2z" />
            </svg>
          </span>
          <span class="text-sm font-medium text-gray-900 dark:text-white">
            {{ link.label }}
          </span>
        </NuxtLink>
      </div>
    </div>

    <!-- Main Content Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <!-- Latest News - 2 columns -->
      <div class="lg:col-span-2 space-y-4">
        <div class="flex items-center justify-between">
          <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
            Najnowsze aktualności
          </h2>
          <NuxtLink
            to="/dashboard/news"
            class="text-sm text-blue-600 dark:text-blue-400 hover:underline"
          >
            Zobacz wszystkie →
          </NuxtLink>
        </div>

        <div class="space-y-4">
          <article
            v-for="news in latestNews"
            :key="news.id"
            class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow"
          >
            <div v-if="news.imageUrl" class="h-48 overflow-hidden">
              <img
                :src="news.imageUrl"
                :alt="news.title"
                class="w-full h-full object-cover"
              >
            </div>
            <div class="p-6">
              <div class="flex items-center gap-2 mb-3">
                <span
                  :class="getCategoryColor(news.category)"
                  class="px-2 py-1 text-xs font-medium rounded-full"
                >
                  {{ getCategoryLabel(news.category) }}
                </span>
                <span class="text-xs text-gray-500 dark:text-gray-400">
                  {{ formatDate(news.createdAt) }}
                </span>
              </div>
              <h3 class="text-xl font-semibold text-gray-900 dark:text-white mb-2">
                {{ news.title }}
              </h3>
              <p class="text-gray-600 dark:text-gray-300 mb-4 line-clamp-2">
                {{ news.excerpt }}
              </p>
              <div class="flex items-center justify-between">
                <div class="flex items-center gap-2 text-sm text-gray-500 dark:text-gray-400">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                  </svg>
                  <span>{{ news.author?.firstName }} {{ news.author?.lastName }}</span>
                </div>
                <NuxtLink
                  :to="`/dashboard/news/${news.id}`"
                  class="text-sm text-blue-600 dark:text-blue-400 hover:underline"
                >
                  Czytaj więcej
                </NuxtLink>
              </div>
            </div>
          </article>
        </div>
      </div>

      <!-- Upcoming Events - 1 column -->
      <div class="space-y-4">
        <div class="flex items-center justify-between">
          <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
            Nadchodzące wydarzenia
          </h2>
          <NuxtLink
            to="/dashboard/calendar"
            class="text-sm text-blue-600 dark:text-blue-400 hover:underline"
          >
            Kalendarz →
          </NuxtLink>
        </div>

        <div class="space-y-3">
          <div
            v-for="event in upcomingEvents"
            :key="event.id"
            class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4 hover:shadow-lg transition-shadow cursor-pointer"
          >
            <div class="flex gap-4">
              <!-- Date Badge -->
              <div class="flex-shrink-0 w-14 h-14 bg-blue-600 rounded-lg flex flex-col items-center justify-center text-white">
                <span class="text-xs font-medium">
                  {{ new Intl.DateTimeFormat('pl-PL', { month: 'short' }).format(event.startDate) }}
                </span>
                <span class="text-xl font-bold">
                  {{ event.startDate.getDate() }}
                </span>
              </div>

              <!-- Event Info -->
              <div class="flex-1 min-w-0">
                <h3 class="font-semibold text-gray-900 dark:text-white mb-1 truncate">
                  {{ event.title }}
                </h3>
                <p class="text-xs text-gray-600 dark:text-gray-400 mb-2">
                  {{ formatEventDate(event.startDate) }}
                </p>
                <div class="flex flex-wrap gap-1">
                  <span
                    v-for="tag in event.tags.slice(0, 2)"
                    :key="tag"
                    :class="getEventTagColor(tag)"
                    class="px-2 py-0.5 text-xs font-medium rounded-full"
                  >
                    #{{ tag }}
                  </span>
                </div>
              </div>
            </div>
          </div>

          <div v-if="upcomingEvents.length === 0" class="text-center py-8 text-gray-500 dark:text-gray-400">
            <svg class="mx-auto h-12 w-12 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            <p>Brak nadchodzących wydarzeń</p>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

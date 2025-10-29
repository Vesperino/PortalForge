<script setup lang="ts">
import type { Event, News } from '~/types'

definePageMeta({
  layout: 'default',
  middleware: 'auth'
})

const { fetchAllNews } = useNewsApi()
const router = useRouter()

const currentDate = ref(new Date())
const selectedEvent = ref<Event | null>(null)
const selectedNews = ref<News | null>(null)
const showEventModal = ref(false)
const showNewsEventModal = ref(false)
const allNewsEvents = ref<News[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)

const currentMonth = computed(() => currentDate.value.getMonth())
const currentYear = computed(() => currentDate.value.getFullYear())

const monthNames = [
  'Styczeń', 'Luty', 'Marzec', 'Kwiecień', 'Maj', 'Czerwiec',
  'Lipiec', 'Sierpień', 'Wrzesień', 'Październik', 'Listopad', 'Grudzień'
]

const daysInMonth = computed(() => {
  return new Date(currentYear.value, currentMonth.value + 1, 0).getDate()
})

const firstDayOfMonth = computed(() => {
  const day = new Date(currentYear.value, currentMonth.value, 1).getDay()
  return day === 0 ? 6 : day - 1
})

const calendarDays = computed(() => {
  const days = []
  for (let i = 0; i < firstDayOfMonth.value; i++) {
    days.push(null)
  }
  for (let i = 1; i <= daysInMonth.value; i++) {
    days.push(i)
  }
  return days
})

const monthEvents = computed(() => {
  return allNewsEvents.value.filter(news => {
    if (!news.eventDateTime) return false
    const eventDate = new Date(news.eventDateTime)
    return eventDate.getFullYear() === currentYear.value &&
           eventDate.getMonth() === currentMonth.value
  })
})

const getEventsForDay = (day: number | null) => {
  if (!day) return []

  return monthEvents.value.filter(news => {
    if (!news.eventDateTime) return false
    const eventDate = new Date(news.eventDateTime)
    return eventDate.getDate() === day &&
           eventDate.getMonth() === currentMonth.value &&
           eventDate.getFullYear() === currentYear.value
  })
}

async function loadNewsEvents() {
  isLoading.value = true
  error.value = null

  try {
    // Fetch only news items that are events
    const newsData = await fetchAllNews(undefined, undefined, true)
    allNewsEvents.value = newsData
  } catch (err: any) {
    error.value = err?.message || 'Nie udało się załadować wydarzeń'
    console.error('loadNewsEvents error:', err)
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  loadNewsEvents()
})

const isToday = (day: number | null) => {
  if (!day) return false
  const today = new Date()
  return day === today.getDate() &&
         currentMonth.value === today.getMonth() &&
         currentYear.value === today.getFullYear()
}

const previousMonth = () => {
  currentDate.value = new Date(currentYear.value, currentMonth.value - 1, 1)
}

const nextMonth = () => {
  currentDate.value = new Date(currentYear.value, currentMonth.value + 1, 1)
}

const goToToday = () => {
  currentDate.value = new Date()
}

const openEventModal = (event: Event) => {
  selectedEvent.value = event
  showEventModal.value = true
}

const openNewsEventModal = (news: News) => {
  selectedNews.value = news
  showNewsEventModal.value = true
}

const closeEventModal = () => {
  showEventModal.value = false
  selectedEvent.value = null
}

const closeNewsEventModal = () => {
  showNewsEventModal.value = false
  selectedNews.value = null
}

const getEventTagColor = (tag: string) => {
  const colors: Record<string, string> = {
    'szkolenie': 'bg-green-500',
    'impreza': 'bg-blue-500',
    'spotkanie': 'bg-orange-500',
    'meeting': 'bg-yellow-500',
    'all-hands': 'bg-red-500',
    'urodziny': 'bg-pink-500',
    'święto': 'bg-purple-500'
  }
  return colors[tag] || 'bg-gray-500'
}

const formatEventTime = (date: Date) => {
  return new Intl.DateTimeFormat('pl-PL', {
    hour: '2-digit',
    minute: '2-digit'
  }).format(date)
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
        Kalendarz wydarzeń
      </h1>

      <!-- Navigation -->
      <div class="flex items-center gap-2">
        <button
          class="px-3 py-2 text-sm font-medium text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-800 hover:bg-gray-50 dark:hover:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-md transition"
          @click="previousMonth"
        >
          ← Poprzedni
        </button>
        <button
          class="px-3 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-md transition"
          @click="goToToday"
        >
          Dziś
        </button>
        <button
          class="px-3 py-2 text-sm font-medium text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-800 hover:bg-gray-50 dark:hover:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-md transition"
          @click="nextMonth"
        >
          Następny →
        </button>
      </div>
    </div>

    <!-- Calendar -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden">
      <div class="p-6">
        <!-- Month/Year Header -->
        <div class="text-center mb-6">
          <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
            {{ monthNames[currentMonth] }} {{ currentYear }}
          </h2>
        </div>

        <!-- Days of Week -->
        <div class="grid grid-cols-7 gap-2 mb-2">
          <div
            v-for="day in ['Pn', 'Wt', 'Śr', 'Cz', 'Pt', 'So', 'Nd']"
            :key="day"
            class="text-center font-semibold text-gray-700 dark:text-gray-300 py-2 text-sm"
          >
            {{ day }}
          </div>
        </div>

        <!-- Calendar Grid -->
        <div class="grid grid-cols-7 gap-2">
          <div
            v-for="(day, index) in calendarDays"
            :key="index"
            class="min-h-[100px] relative"
          >
            <div
              v-if="day"
              :class="[
                'h-full p-2 rounded-lg border transition cursor-pointer',
                isToday(day)
                  ? 'bg-blue-50 dark:bg-blue-900/20 border-blue-500'
                  : 'bg-gray-50 dark:bg-gray-700 border-gray-200 dark:border-gray-600 hover:bg-gray-100 dark:hover:bg-gray-600'
              ]"
            >
              <!-- Day Number -->
              <div
                :class="[
                  'text-sm font-semibold mb-1',
                  isToday(day)
                    ? 'text-blue-600 dark:text-blue-400'
                    : 'text-gray-900 dark:text-gray-100'
                ]"
              >
                {{ day }}
              </div>

              <!-- Events for this day -->
              <div class="space-y-1">
                <div
                  v-for="news in getEventsForDay(day).slice(0, 3)"
                  :key="news.id"
                  class="bg-blue-500 dark:bg-blue-600 text-white text-xs px-2 py-1 rounded truncate hover:opacity-80 transition cursor-pointer"
                  :title="news.title"
                  @click="openNewsEventModal(news)"
                >
                  {{ news.title }}
                </div>
                <div
                  v-if="getEventsForDay(day).length > 3"
                  class="text-xs text-gray-600 dark:text-gray-400 pl-2"
                >
                  +{{ getEventsForDay(day).length - 3 }} więcej
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Upcoming Events List -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
      <h2 class="text-xl font-bold text-gray-900 dark:text-white mb-4">
        Wydarzenia w tym miesiącu
      </h2>

      <div v-if="isLoading" class="text-center py-8">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto" />
        <p class="mt-2 text-gray-600 dark:text-gray-400">Ładowanie wydarzeń...</p>
      </div>

      <div v-else-if="error" class="text-center py-8 text-red-600 dark:text-red-400">
        {{ error }}
      </div>

      <div v-else-if="monthEvents.length > 0" class="space-y-3">
        <div
          v-for="news in monthEvents"
          :key="news.id"
          class="flex items-start gap-4 p-4 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition cursor-pointer"
          @click="openNewsEventModal(news)"
        >
          <!-- Date -->
          <div class="flex-shrink-0 w-14 h-14 bg-blue-600 dark:bg-blue-700 rounded-lg flex flex-col items-center justify-center text-white">
            <span class="text-xs font-medium">
              {{ news.eventDateTime ? new Intl.DateTimeFormat('pl-PL', { month: 'short' }).format(new Date(news.eventDateTime)) : '' }}
            </span>
            <span class="text-xl font-bold">
              {{ news.eventDateTime ? new Date(news.eventDateTime).getDate() : '' }}
            </span>
          </div>

          <!-- Event Info -->
          <div class="flex-1">
            <h3 class="font-semibold text-gray-900 dark:text-white mb-1">
              {{ news.title }}
            </h3>
            <p class="text-sm text-gray-600 dark:text-gray-400 mb-2 line-clamp-2">
              {{ news.excerpt }}
            </p>
            <div class="flex flex-wrap gap-2">
              <span
                v-if="news.eventHashtag"
                class="bg-blue-500 dark:bg-blue-600 text-white text-xs px-2 py-1 rounded-full"
              >
                {{ news.eventHashtag }}
              </span>
              <span v-if="news.eventLocation" class="text-xs text-gray-500 dark:text-gray-400 flex items-center gap-1">
                <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                {{ news.eventLocation }}
              </span>
            </div>
          </div>

          <!-- Time -->
          <div class="flex-shrink-0 text-sm text-gray-600 dark:text-gray-400">
            {{ news.eventDateTime ? formatEventTime(new Date(news.eventDateTime)) : '' }}
          </div>
        </div>
      </div>

      <div v-else class="text-center py-8 text-gray-500 dark:text-gray-400">
        <svg class="mx-auto h-12 w-12 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
        </svg>
        <p>Brak wydarzeń w tym miesiącu</p>
      </div>
    </div>

    <!-- Event Modal -->
    <Teleport to="body">
      <div
        v-if="showEventModal && selectedEvent"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50"
        @click.self="closeEventModal"
      >
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
          <!-- Modal Header -->
          <div class="sticky top-0 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 p-6">
            <div class="flex items-start justify-between">
              <div class="flex-1">
                <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                  {{ selectedEvent.title }}
                </h2>
                <div class="flex flex-wrap gap-2">
                  <span
                    v-for="tag in selectedEvent.tags"
                    :key="tag"
                    :class="[
                      getEventTagColor(tag),
                      'text-white text-xs px-2 py-1 rounded-full'
                    ]"
                  >
                    #{{ tag }}
                  </span>
                </div>
              </div>
              <button
                type="button"
                class="ml-4 text-gray-500 hover:text-gray-700 dark:hover:text-gray-300"
                @click="closeEventModal"
              >
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
          </div>

          <!-- Modal Content -->
          <div class="p-6 space-y-4">
            <!-- Date & Time -->
            <div class="flex items-start gap-3">
              <svg class="w-5 h-5 text-gray-500 dark:text-gray-400 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              <div>
                <p class="text-sm font-medium text-gray-900 dark:text-white">
                  {{ new Intl.DateTimeFormat('pl-PL', { dateStyle: 'full' }).format(selectedEvent.startDate) }}
                </p>
                <p class="text-sm text-gray-600 dark:text-gray-400">
                  {{ formatEventTime(selectedEvent.startDate) }}
                  <span v-if="selectedEvent.endDate">
                    - {{ formatEventTime(selectedEvent.endDate) }}
                  </span>
                </p>
              </div>
            </div>

            <!-- Location -->
            <div v-if="selectedEvent.location" class="flex items-start gap-3">
              <svg class="w-5 h-5 text-gray-500 dark:text-gray-400 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
              <div>
                <p class="text-sm font-medium text-gray-900 dark:text-white">Lokalizacja</p>
                <p class="text-sm text-gray-600 dark:text-gray-400">{{ selectedEvent.location }}</p>
              </div>
            </div>

            <!-- Attendees -->
            <div v-if="selectedEvent.attendees" class="flex items-start gap-3">
              <svg class="w-5 h-5 text-gray-500 dark:text-gray-400 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
              </svg>
              <div>
                <p class="text-sm font-medium text-gray-900 dark:text-white">Uczestnicy</p>
                <p class="text-sm text-gray-600 dark:text-gray-400">{{ selectedEvent.attendees }} osób</p>
              </div>
            </div>

            <!-- Description -->
            <div class="pt-4 border-t border-gray-200 dark:border-gray-700">
              <h3 class="text-sm font-medium text-gray-900 dark:text-white mb-2">Opis</h3>
              <p class="text-sm text-gray-600 dark:text-gray-400 whitespace-pre-line leading-relaxed">
                {{ selectedEvent.description }}
              </p>
            </div>

            <!-- Target Departments -->
            <div v-if="selectedEvent.targetDepartments.length > 0" class="pt-4 border-t border-gray-200 dark:border-gray-700">
              <h3 class="text-sm font-medium text-gray-900 dark:text-white mb-2">Dla działów</h3>
              <div class="flex flex-wrap gap-2">
                <span
                  v-for="deptId in selectedEvent.targetDepartments"
                  :key="deptId"
                  class="px-3 py-1 text-xs font-medium rounded-full bg-gray-100 dark:bg-gray-700 text-gray-800 dark:text-gray-200"
                >
                  Dział {{ deptId }}
                </span>
              </div>
            </div>
            <div v-else class="pt-4 border-t border-gray-200 dark:border-gray-700">
              <p class="text-sm text-gray-600 dark:text-gray-400">
                📢 Wydarzenie dla wszystkich pracowników
              </p>
            </div>

            <!-- Related News Button -->
            <div v-if="selectedEvent.newsId" class="pt-4 border-t border-gray-200 dark:border-gray-700">
              <button
                class="w-full flex items-center justify-center gap-2 px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg transition-colors"
                @click="router.push(`/dashboard/news/${selectedEvent.newsId}`)"
              >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 20H5a2 2 0 01-2-2V6a2 2 0 012-2h10a2 2 0 012 2v1m2 13a2 2 0 01-2-2V7m2 13a2 2 0 002-2V9a2 2 0 00-2-2h-2m-4-3H9M7 16h6M7 8h6v4H7V8z" />
                </svg>
                Zobacz powiązany news
              </button>
            </div>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- News Event Modal -->
    <Teleport to="body">
      <div
        v-if="showNewsEventModal && selectedNews"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50"
        @click.self="closeNewsEventModal"
      >
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
          <!-- Modal Header -->
          <div class="sticky top-0 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 p-6">
            <div class="flex items-start justify-between">
              <div class="flex-1">
                <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                  {{ selectedNews.title }}
                </h2>
                <div class="flex flex-wrap gap-2">
                  <span class="bg-blue-500 dark:bg-blue-600 text-white text-xs px-2 py-1 rounded-full">
                    📅 Wydarzenie
                  </span>
                  <span
                    v-if="selectedNews.eventHashtag"
                    class="bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300 text-xs px-2 py-1 rounded-full"
                  >
                    {{ selectedNews.eventHashtag }}
                  </span>
                </div>
              </div>
              <button
                type="button"
                class="ml-4 text-gray-500 hover:text-gray-700 dark:hover:text-gray-300"
                @click="closeNewsEventModal"
              >
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
          </div>

          <!-- Modal Content -->
          <div class="p-6 space-y-4">
            <!-- Date & Time -->
            <div v-if="selectedNews.eventDateTime" class="flex items-start gap-3">
              <svg class="w-5 h-5 text-blue-600 dark:text-blue-400 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <div>
                <p class="text-sm font-medium text-gray-900 dark:text-white">
                  {{ new Intl.DateTimeFormat('pl-PL', { dateStyle: 'full', timeStyle: 'short' }).format(new Date(selectedNews.eventDateTime)) }}
                </p>
              </div>
            </div>

            <!-- Location -->
            <div v-if="selectedNews.eventLocation" class="flex items-start gap-3">
              <svg class="w-5 h-5 text-blue-600 dark:text-blue-400 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
              <div>
                <p class="text-sm font-medium text-gray-900 dark:text-white">Lokalizacja</p>
                <p class="text-sm text-gray-600 dark:text-gray-400">{{ selectedNews.eventLocation }}</p>
              </div>
            </div>

            <!-- Description -->
            <div class="pt-4 border-t border-gray-200 dark:border-gray-700">
              <h3 class="text-sm font-medium text-gray-900 dark:text-white mb-2">Opis</h3>
              <p class="text-sm text-gray-600 dark:text-gray-400">
                {{ selectedNews.excerpt }}
              </p>
            </div>

            <!-- View Full News Button -->
            <div class="pt-4 border-t border-gray-200 dark:border-gray-700">
              <button
                class="w-full flex items-center justify-center gap-2 px-6 py-3 bg-blue-600 hover:bg-blue-700 dark:bg-blue-700 dark:hover:bg-blue-800 text-white font-semibold rounded-lg transition-colors"
                @click="router.push(`/dashboard/news/${selectedNews.id}`)"
              >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 20H5a2 2 0 01-2-2V6a2 2 0 012-2h10a2 2 0 012 2v1m2 13a2 2 0 01-2-2V7m2 13a2 2 0 002-2V9a2 2 0 00-2-2h-2m-4-3H9M7 16h6M7 8h6v4H7V8z" />
                </svg>
                Zobacz pełny news
              </button>
            </div>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

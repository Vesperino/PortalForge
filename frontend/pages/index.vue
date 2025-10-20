<script setup lang="ts">
definePageMeta({
  middleware: 'verified',
})

useHead({
  title: 'Kalendarz - PortalForge',
})

const currentDate = ref(new Date())
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
</script>

<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900 py-8 px-4 sm:px-6 lg:px-8">
    <div class="max-w-4xl mx-auto">
      <div class="text-center mb-8">
        <h1 class="text-4xl font-bold text-gray-900 dark:text-white mb-2">
          Kalendarz firmowy
        </h1>
        <p class="text-lg text-gray-600 dark:text-gray-400">
          Sprawdź nadchodzące wydarzenia
        </p>
      </div>

      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden">
        <div class="p-6">
          <div class="flex items-center justify-between mb-6">
            <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
              {{ monthNames[currentMonth] }} {{ currentYear }}
            </h2>
            <div class="flex gap-2">
              <button
                class="px-3 py-2 text-sm font-medium text-gray-700 dark:text-gray-200 bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 rounded-md transition"
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
                class="px-3 py-2 text-sm font-medium text-gray-700 dark:text-gray-200 bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 rounded-md transition"
                @click="nextMonth"
              >
                Następny →
              </button>
            </div>
          </div>

          <div class="grid grid-cols-7 gap-2">
            <div
              v-for="day in ['Pn', 'Wt', 'Śr', 'Cz', 'Pt', 'So', 'Nd']"
              :key="day"
              class="text-center font-semibold text-gray-700 dark:text-gray-300 py-2"
            >
              {{ day }}
            </div>

            <div
              v-for="(day, index) in calendarDays"
              :key="index"
              class="aspect-square"
            >
              <div
                v-if="day"
                :class="[
                  'h-full flex items-center justify-center rounded-lg transition cursor-pointer',
                  isToday(day)
                    ? 'bg-blue-600 text-white font-bold'
                    : 'bg-gray-50 dark:bg-gray-700 text-gray-900 dark:text-gray-100 hover:bg-gray-100 dark:hover:bg-gray-600'
                ]"
              >
                {{ day }}
              </div>
            </div>
          </div>
        </div>

        <div class="bg-gray-50 dark:bg-gray-700 px-6 py-4 border-t border-gray-200 dark:border-gray-600">
          <p class="text-sm text-gray-600 dark:text-gray-300">
            Wkrótce: Funkcjonalność dodawania wydarzeń
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

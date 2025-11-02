<script setup lang="ts">
import type { News } from '~/types'

interface Props {
  event: News | null
  show: boolean
}

interface Emits {
  (e: 'close'): void
  (e: 'viewDetails', eventId: number): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

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

const formatDateShort = (dateString: string | Date) => {
  const date = typeof dateString === 'string' ? new Date(dateString) : dateString
  return new Intl.DateTimeFormat('pl-PL', {
    day: 'numeric',
    month: 'short',
    year: 'numeric'
  }).format(date)
}

function handleClose() {
  emit('close')
}

function handleViewDetails() {
  if (props.event) {
    emit('viewDetails', props.event.id)
  }
}
</script>

<template>
  <Teleport to="body">
    <Transition
      enter-active-class="transition-opacity duration-200"
      leave-active-class="transition-opacity duration-200"
      enter-from-class="opacity-0"
      leave-to-class="opacity-0"
    >
      <div
        v-if="show && event"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm"
        @click.self="handleClose"
      >
        <Transition
          enter-active-class="transition-all duration-200"
          leave-active-class="transition-all duration-200"
          enter-from-class="opacity-0 scale-95"
          leave-to-class="opacity-0 scale-95"
        >
          <div
            v-if="show"
            class="bg-white dark:bg-gray-800 rounded-xl shadow-2xl max-w-2xl w-full max-h-[90vh] overflow-y-auto"
          >
            <!-- Header with Image -->
            <div class="relative">
              <div
                v-if="event.imageUrl"
                class="h-48 overflow-hidden bg-gradient-to-br from-blue-500 to-blue-700"
              >
                <img
                  :src="event.imageUrl"
                  :alt="event.title"
                  class="w-full h-full object-cover"
                >
              </div>
              <div
                v-else
                class="h-48 bg-gradient-to-br from-blue-500 to-blue-700 flex items-center justify-center"
              >
                <svg class="w-20 h-20 text-white/50" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </div>

              <!-- Close button -->
              <button
                class="absolute top-4 right-4 p-2 bg-white/10 hover:bg-white/20 backdrop-blur-sm rounded-full transition-colors"
                @click="handleClose"
              >
                <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>

              <!-- Event badge -->
              <div class="absolute bottom-4 left-4">
                <span class="px-3 py-1.5 bg-blue-600 text-white text-sm font-medium rounded-full shadow-lg">
                  📅 Wydarzenie
                </span>
              </div>
            </div>

            <!-- Content -->
            <div class="p-6 space-y-6">
              <!-- Title -->
              <div>
                <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                  {{ event.title }}
                </h2>
                <p v-if="event.excerpt" class="text-gray-600 dark:text-gray-400 leading-relaxed">
                  {{ event.excerpt }}
                </p>
              </div>

              <!-- Event Details -->
              <div class="space-y-4">
                <!-- Date and Time -->
                <div v-if="event.eventDateTime" class="flex items-start gap-3">
                  <div class="p-2 bg-blue-100 dark:bg-blue-900/50 rounded-lg">
                    <svg class="w-5 h-5 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </div>
                  <div>
                    <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Data i godzina</p>
                    <p class="text-base font-semibold text-gray-900 dark:text-gray-100">
                      {{ formatDate(event.eventDateTime) }}
                    </p>
                  </div>
                </div>

                <!-- Location -->
                <div v-if="event.eventLocation" class="flex items-start gap-3">
                  <div class="p-2 bg-green-100 dark:bg-green-900/50 rounded-lg">
                    <svg class="w-5 h-5 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                    </svg>
                  </div>
                  <div>
                    <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Lokalizacja</p>
                    <p class="text-base font-semibold text-gray-900 dark:text-gray-100">
                      {{ event.eventLocation }}
                    </p>
                  </div>
                </div>

                <!-- Hashtag -->
                <div v-if="event.eventHashtag" class="flex items-start gap-3">
                  <div class="p-2 bg-purple-100 dark:bg-purple-900/50 rounded-lg">
                    <svg class="w-5 h-5 text-purple-600 dark:text-purple-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 20l4-16m2 16l4-16M6 9h14M4 15h14" />
                    </svg>
                  </div>
                  <div>
                    <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Hashtag</p>
                    <p class="text-base font-semibold text-purple-600 dark:text-purple-400">
                      {{ event.eventHashtag }}
                    </p>
                  </div>
                </div>

                <!-- Published Date -->
                <div class="flex items-start gap-3">
                  <div class="p-2 bg-gray-100 dark:bg-gray-700 rounded-lg">
                    <svg class="w-5 h-5 text-gray-600 dark:text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                    </svg>
                  </div>
                  <div>
                    <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Opublikowano</p>
                    <p class="text-base font-semibold text-gray-900 dark:text-gray-100">
                      {{ formatDateShort(event.createdAt) }}
                    </p>
                  </div>
                </div>

                <!-- Author -->
                <div v-if="event.author || event.authorName" class="flex items-start gap-3">
                  <div class="p-2 bg-indigo-100 dark:bg-indigo-900/50 rounded-lg">
                    <svg class="w-5 h-5 text-indigo-600 dark:text-indigo-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                    </svg>
                  </div>
                  <div>
                    <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Organizator</p>
                    <p class="text-base font-semibold text-gray-900 dark:text-gray-100">
                      {{ event.authorName || `${event.author?.firstName} ${event.author?.lastName}` }}
                    </p>
                  </div>
                </div>
              </div>

              <!-- Actions -->
              <div class="flex flex-col sm:flex-row gap-3 pt-4 border-t border-gray-200 dark:border-gray-700">
                <button
                  class="flex-1 flex items-center justify-center gap-2 px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg transition-colors shadow-md"
                  @click="handleViewDetails"
                >
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                  </svg>
                  Zobacz szczegóły
                </button>
                <button
                  class="px-6 py-3 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 font-semibold rounded-lg hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors"
                  @click="handleClose"
                >
                  Zamknij
                </button>
              </div>
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import type { NewsCategory } from '~/types'

definePageMeta({
  middleware: ['auth', 'verified', 'news-admin'],
  layout: 'default'
})

const { createNews } = useNewsApi()
const router = useRouter()

const title = ref('')
const excerpt = ref('')
const content = ref('')
const imageUrl = ref('')
const category = ref<NewsCategory>('announcement')
const eventId = ref<number | undefined>(undefined)
const isEvent = ref(false)
const eventHashtag = ref('')
const eventDateTime = ref<Date | null>(null)
const eventLocation = ref('')
const eventLatitude = ref<number | undefined>(undefined)
const eventLongitude = ref<number | undefined>(undefined)
const eventPlaceId = ref('')
const departmentId = ref<number | undefined>(undefined)
const hashtags = ref<string[]>([])
const autoDetectedHashtags = ref<string[]>([])
const isSubmitting = ref(false)
const error = ref<string | null>(null)
const successMessage = ref<string | null>(null)

const categories: { value: NewsCategory; label: string }[] = [
  { value: 'announcement', label: 'Ogłoszenie' },
  { value: 'product', label: 'Produkt' },
  { value: 'hr', label: 'HR' },
  { value: 'tech', label: 'Technologia' },
  { value: 'event', label: 'Wydarzenie' }
]

function handleHashtagsDetected(detected: string[]) {
  autoDetectedHashtags.value = detected
  
  // Merge with manually added hashtags
  const combined = [...new Set([...hashtags.value, ...detected])]
  hashtags.value = combined
}

async function handleSubmit() {
  if (!title.value || !excerpt.value || !content.value) {
    error.value = 'Proszę wypełnić wszystkie wymagane pola'
    return
  }

  isSubmitting.value = true
  error.value = null
  successMessage.value = null

  try {
    const newsId = await createNews({
      title: title.value,
      excerpt: excerpt.value,
      content: content.value,
      imageUrl: imageUrl.value || undefined,
      category: category.value,
      eventId: eventId.value,
      isEvent: isEvent.value,
      eventHashtag: eventHashtag.value || undefined,
      eventDateTime: eventDateTime.value?.toISOString() || undefined,
      eventLocation: eventLocation.value || undefined,
      eventPlaceId: eventPlaceId.value || undefined,
      eventLatitude: eventLatitude.value,
      eventLongitude: eventLongitude.value,
      departmentId: departmentId.value,
      hashtags: hashtags.value.length > 0 ? hashtags.value : undefined
    })

    successMessage.value = 'News utworzony pomyślnie!'

    // Redirect after short delay
    setTimeout(() => {
      router.push(`/dashboard/news/${newsId}`)
    }, 1000)
  } catch (err: unknown) {
    error.value = err instanceof Error ? err.message : 'Nie udało się utworzyć newsa. Spróbuj ponownie.'
    console.error(err)
  } finally {
    isSubmitting.value = false
  }
}

function handleCancel() {
  router.back()
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
        Utwórz aktualność
      </h1>
      <button
        type="button"
        class="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white transition"
        @click="handleCancel"
      >
        ← Powrót
      </button>
    </div>

    <!-- Success/Error Messages -->
    <div v-if="successMessage" class="p-4 bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200 rounded-lg">
      {{ successMessage }}
    </div>
    <div v-if="error" class="p-4 bg-red-100 dark:bg-red-900 text-red-800 dark:text-red-200 rounded-lg">
      {{ error }}
    </div>

    <!-- Form -->
    <form @submit.prevent="handleSubmit">
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Main Content (Left - 2/3) -->
        <div class="lg:col-span-2 space-y-6">
          <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 space-y-6">
            <h2 class="text-xl font-semibold text-gray-900 dark:text-white border-b border-gray-200 dark:border-gray-700 pb-3">
              Treść aktualności
            </h2>

            <!-- Title -->
            <div>
              <label for="title" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Tytuł *
              </label>
              <input
                id="title"
                v-model="title"
                type="text"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                placeholder="Wprowadź tytuł aktualności"
                required
              >
            </div>

            <!-- Excerpt -->
            <div>
              <label for="excerpt" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Skrót *
              </label>
              <textarea
                id="excerpt"
                v-model="excerpt"
                rows="3"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                placeholder="Krótki opis aktualności (wyświetlany na liście)"
                required
              />
            </div>

            <!-- Content (TipTap Rich Text Editor) -->
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Treść *
              </label>
              <RichTextEditor
                v-model="content"
                @hashtags-detected="handleHashtagsDetected"
              />
            </div>
          </div>
        </div>

        <!-- Metadata Sidebar (Right - 1/3) -->
        <div class="space-y-6">
          <!-- Basic Settings -->
          <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 space-y-6">
            <h2 class="text-xl font-semibold text-gray-900 dark:text-white border-b border-gray-200 dark:border-gray-700 pb-3">
              Ustawienia
            </h2>

            <!-- Category -->
            <div>
              <label for="category" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Kategoria *
              </label>
              <select
                id="category"
                v-model="category"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                required
              >
                <option v-for="cat in categories" :key="cat.value" :value="cat.value">
                  {{ cat.label }}
                </option>
              </select>
            </div>

            <!-- Department -->
            <div>
              <label for="departmentId" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Dział (opcjonalnie)
              </label>
              <select
                id="departmentId"
                v-model="departmentId"
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
              <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
                Jeśli wybierzesz dział, news będzie widoczny tylko dla tego działu
              </p>
            </div>

            <!-- Image Upload -->
            <ImageUpload
              v-model="imageUrl"
              label="Obrazek aktualności"
              :max-size-m-b="5"
            />

            <!-- Hashtags -->
            <HashtagInput
              v-model="hashtags"
              label="Hashtagi"
              :allow-custom="true"
            />
            <p v-if="autoDetectedHashtags.length > 0" class="text-xs text-blue-600 dark:text-blue-400">
              Wykryte automatycznie: {{ autoDetectedHashtags.join(', ') }}
            </p>
          </div>

          <!-- Event Settings -->
          <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 space-y-6">
            <div class="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 pb-3">
              <h2 class="text-xl font-semibold text-gray-900 dark:text-white">
                Wydarzenie
              </h2>
              <div class="flex items-center space-x-3">
                <input
                  id="isEvent"
                  v-model="isEvent"
                  type="checkbox"
                  class="w-5 h-5 text-blue-600 bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600 rounded focus:ring-blue-500 focus:ring-2"
                >
                <label for="isEvent" class="text-sm font-medium text-gray-700 dark:text-gray-300">
                  To jest wydarzenie
                </label>
              </div>
            </div>

            <!-- Event Fields -->
            <div v-if="isEvent" class="space-y-4">
              <!-- Event Hashtag -->
              <div>
                <label for="eventHashtag" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Hashtag wydarzenia
                </label>
                <input
                  id="eventHashtag"
                  v-model="eventHashtag"
                  type="text"
                  class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                  placeholder="#wydarzenie2025"
                >
              </div>

              <!-- Event Date Time -->
              <DateTimePicker
                v-model="eventDateTime"
                label="Data i godzina wydarzenia"
              />

              <!-- Event Location -->
              <LocationPickerOSM
                v-model="eventLocation"
                :latitude="eventLatitude"
                :longitude="eventLongitude"
                label="Lokalizacja wydarzenia"
                @update:latitude="eventLatitude = $event"
                @update:longitude="eventLongitude = $event"
              />

              <!-- Event ID (optional) -->
              <div>
                <label for="eventId" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  ID Wydarzenia (opcjonalne)
                </label>
                <input
                  id="eventId"
                  v-model.number="eventId"
                  type="number"
                  class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                  placeholder="Zostaw puste jeśli nie jest powiązane z wydarzeniem"
                >
              </div>
            </div>
          </div>

          <!-- Actions (Sticky) -->
          <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 lg:sticky lg:top-6">
            <div class="flex flex-col gap-3">
              <button
                type="submit"
                :disabled="isSubmitting"
                class="w-full px-6 py-3 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:bg-gray-400 disabled:cursor-not-allowed transition font-medium text-center"
              >
                {{ isSubmitting ? 'Tworzenie...' : 'Utwórz aktualność' }}
              </button>
              <button
                type="button"
                class="w-full px-6 py-3 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-300 dark:hover:bg-gray-600 transition font-medium text-center"
                :disabled="isSubmitting"
                @click="handleCancel"
              >
                Anuluj
              </button>
            </div>
          </div>
        </div>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { NewsCategory } from '~/types'

definePageMeta({
  middleware: ['auth', 'news-admin'],
  layout: 'default'
})

const route = useRoute()
const router = useRouter()
const { fetchNewsById, updateNews } = useNewsApi()

const newsId = computed(() => Number.parseInt(route.params.id as string, 10))

const title = ref('')
const excerpt = ref('')
const content = ref('')
const imageUrl = ref('')
const category = ref<NewsCategory>('announcement')
const eventId = ref<number | undefined>(undefined)
const isLoading = ref(false)
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

async function loadNews() {
  isLoading.value = true
  error.value = null

  try {
    const news = await fetchNewsById(newsId.value)

    title.value = news.title
    excerpt.value = news.excerpt || ''
    content.value = news.content
    imageUrl.value = news.imageUrl || ''
    category.value = news.category.toLowerCase() as NewsCategory
    eventId.value = news.eventId || undefined
  } catch (err: any) {
    error.value = err?.message || 'Nie udało się załadować newsa'
    console.error(err)
  } finally {
    isLoading.value = false
  }
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
    await updateNews(newsId.value, {
      title: title.value,
      excerpt: excerpt.value,
      content: content.value,
      imageUrl: imageUrl.value || undefined,
      category: category.value,
      eventId: eventId.value
    })

    successMessage.value = 'News zaktualizowany pomyślnie!'

    // Redirect after short delay
    setTimeout(() => {
      router.push(`/dashboard/news/${newsId.value}`)
    }, 1000)
  } catch (err: any) {
    error.value = err?.message || 'Nie udało się zaktualizować newsa. Spróbuj ponownie.'
    console.error(err)
  } finally {
    isSubmitting.value = false
  }
}

function handleCancel() {
  router.push(`/dashboard/news/${newsId.value}`)
}

onMounted(() => {
  loadNews()
})
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
        Edytuj aktualność
      </h1>
      <button
        type="button"
        class="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white transition"
        @click="handleCancel"
      >
        ← Powrót
      </button>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto"/>
      <p class="mt-4 text-gray-600 dark:text-gray-400">Ładowanie aktualności...</p>
    </div>

    <!-- Form -->
    <form v-else class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 space-y-6" @submit.prevent="handleSubmit">
      <!-- Success Message -->
      <div v-if="successMessage" class="p-4 bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200 rounded-lg">
        {{ successMessage }}
      </div>

      <!-- Error Message -->
      <div v-if="error" class="p-4 bg-red-100 dark:bg-red-900 text-red-800 dark:text-red-200 rounded-lg">
        {{ error }}
      </div>

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

      <!-- Content (Rich Text Editor) -->
      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          Treść *
        </label>
        <RichTextEditor v-model="content" />
      </div>

      <!-- Image URL -->
      <div>
        <label for="imageUrl" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          URL obrazka
        </label>
        <input
          id="imageUrl"
          v-model="imageUrl"
          type="url"
          class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
          placeholder="https://example.com/image.jpg"
        >
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
          Opcjonalnie: Link do obrazka który będzie wyświetlany jako miniatura
        </p>
      </div>

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
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
          Jeśli news jest związany z wydarzeniem, podaj jego ID
        </p>
      </div>

      <!-- Actions -->
      <div class="flex gap-4 pt-4 border-t border-gray-200 dark:border-gray-700">
        <button
          type="submit"
          :disabled="isSubmitting || isLoading"
          class="px-6 py-3 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:bg-gray-400 disabled:cursor-not-allowed transition font-medium"
        >
          {{ isSubmitting ? 'Zapisywanie...' : 'Zapisz zmiany' }}
        </button>
        <button
          type="button"
          class="px-6 py-3 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-300 dark:hover:bg-gray-600 transition font-medium"
          :disabled="isSubmitting || isLoading"
          @click="handleCancel"
        >
          Anuluj
        </button>
      </div>
    </form>
  </div>
</template>


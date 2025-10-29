<script setup lang="ts">
import { ref } from 'vue'
import type { NewsCategory, News } from '~/types'

definePageMeta({
  // middleware: 'auth', // Disabled for testing
  layout: 'default'
})

const route = useRoute()
const router = useRouter()
const { fetchNewsById, updateNews } = useNewsApi()

const newsId = Number.parseInt(route.params.id as string)
const news = ref<News | null>(null)
const isLoading = ref(false)
const title = ref('')
const excerpt = ref('')
const content = ref('')
const imageUrl = ref('')
const category = ref<NewsCategory>('announcement')
const isSubmitting = ref(false)
const error = ref<string | null>(null)

const categories: { value: NewsCategory; label: string }[] = [
  { value: 'announcement', label: 'Ogłoszenie' },
  { value: 'product', label: 'Produkt' },
  { value: 'hr', label: 'HR' },
  { value: 'tech', label: 'Tech' },
  { value: 'event', label: 'Event' }
]

async function loadNews() {
  isLoading.value = true
  error.value = null

  try {
    news.value = await fetchNewsById(newsId)

    // Populate form with existing data
    title.value = news.value.title
    excerpt.value = news.value.excerpt
    content.value = news.value.content
    imageUrl.value = news.value.imageUrl || ''
    category.value = news.value.category.toLowerCase() as NewsCategory
  } catch (err) {
    error.value = 'Nie udało się załadować aktualności'
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

  try {
    await updateNews(newsId, {
      title: title.value,
      excerpt: excerpt.value,
      content: content.value,
      imageUrl: imageUrl.value || undefined,
      category: category.value
    })

    await router.push(`/dashboard/news/${newsId}`)
  } catch (err) {
    error.value = 'Nie udało się zaktualizować aktualności. Spróbuj ponownie.'
    console.error(err)
  } finally {
    isSubmitting.value = false
  }
}

onMounted(() => {
  loadNews()
})
</script>

<template>
  <div class="max-w-4xl mx-auto p-6">
    <!-- Back Button -->
    <div class="mb-6">
      <button
        type="button"
        class="flex items-center gap-2 text-blue-600 dark:text-blue-400 hover:underline focus:outline-none"
        @click="router.back()"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
        </svg>
        Wróć
      </button>
    </div>

    <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-6">Edytuj aktualność</h1>

    <!-- Loading State -->
    <div v-if="isLoading" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto"/>
      <p class="mt-4 text-gray-600 dark:text-gray-400">Ładowanie...</p>
    </div>

    <!-- Edit Form -->
    <form v-else class="space-y-6 bg-white dark:bg-gray-800 rounded-lg shadow-md p-8" @submit.prevent="handleSubmit">
      <div v-if="error" class="p-4 bg-red-100 dark:bg-red-900 text-red-700 dark:text-red-200 rounded-lg">
        {{ error }}
      </div>

      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          Tytuł *
        </label>
        <input
          v-model="title"
          type="text"
          class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
          required
        >
      </div>

      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          Kategoria *
        </label>
        <select
          v-model="category"
          class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
          required
        >
          <option v-for="cat in categories" :key="cat.value" :value="cat.value">
            {{ cat.label }}
          </option>
        </select>
      </div>

      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          Skrót *
        </label>
        <textarea
          v-model="excerpt"
          rows="3"
          class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
          required
        />
      </div>

      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          Treść *
        </label>
        <RichTextEditor v-model="content" />
      </div>

      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          URL obrazu
        </label>
        <input
          v-model="imageUrl"
          type="url"
          class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
          placeholder="https://example.com/image.jpg"
        >
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
          Opcjonalne - dodaj link do obrazu nagłówkowego
        </p>
      </div>

      <div class="flex gap-4 pt-4">
        <button
          type="submit"
          :disabled="isSubmitting"
          class="flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
        >
          <svg v-if="!isSubmitting" class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
          </svg>
          <svg v-else class="animate-spin w-5 h-5" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"/>
          </svg>
          {{ isSubmitting ? 'Zapisywanie...' : 'Zapisz zmiany' }}
        </button>
        <button
          type="button"
          class="px-6 py-3 bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-gray-200 rounded-lg hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors"
          @click="router.back()"
        >
          Anuluj
        </button>
      </div>
    </form>
  </div>
</template>

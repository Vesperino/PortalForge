<script setup lang="ts">
import { ref } from 'vue'
import type { NewsCategory } from '~/types'

definePageMeta({
  middleware: 'auth',
  layout: 'dashboard'
})

const { createNews } = useNewsApi()
const router = useRouter()

const title = ref('')
const excerpt = ref('')
const content = ref('')
const imageUrl = ref('')
const category = ref<NewsCategory>('announcement')
const isSubmitting = ref(false)
const error = ref<string | null>(null)

const categories: { value: NewsCategory; label: string }[] = [
  { value: 'announcement', label: 'Announcement' },
  { value: 'product', label: 'Product' },
  { value: 'hr', label: 'HR' },
  { value: 'tech', label: 'Tech' },
  { value: 'event', label: 'Event' }
]

async function handleSubmit() {
  if (!title.value || !excerpt.value || !content.value) {
    error.value = 'Please fill in all required fields'
    return
  }

  isSubmitting.value = true
  error.value = null

  try {
    await createNews({
      title: title.value,
      excerpt: excerpt.value,
      content: content.value,
      imageUrl: imageUrl.value || undefined,
      category: category.value
    })

    await router.push('/dashboard/news')
  } catch (err) {
    error.value = 'Failed to create news. Please try again.'
    console.error(err)
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="max-w-4xl mx-auto p-6">
    <h1 class="text-3xl font-bold mb-6">Create News</h1>

    <form @submit.prevent="handleSubmit" class="space-y-6">
      <div v-if="error" class="p-4 bg-red-100 text-red-700 rounded">
        {{ error }}
      </div>

      <div>
        <label class="block text-sm font-medium mb-2">Title *</label>
        <input
          v-model="title"
          type="text"
          class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500"
          required
        />
      </div>

      <div>
        <label class="block text-sm font-medium mb-2">Category *</label>
        <select
          v-model="category"
          class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500"
          required
        >
          <option v-for="cat in categories" :key="cat.value" :value="cat.value">
            {{ cat.label }}
          </option>
        </select>
      </div>

      <div>
        <label class="block text-sm font-medium mb-2">Excerpt *</label>
        <textarea
          v-model="excerpt"
          rows="3"
          class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500"
          required
        />
      </div>

      <div>
        <label class="block text-sm font-medium mb-2">Content *</label>
        <RichTextEditor v-model="content" />
      </div>

      <div>
        <label class="block text-sm font-medium mb-2">Image URL</label>
        <input
          v-model="imageUrl"
          type="url"
          class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div class="flex gap-4">
        <button
          type="submit"
          :disabled="isSubmitting"
          class="px-6 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:bg-gray-400"
        >
          {{ isSubmitting ? 'Creating...' : 'Create News' }}
        </button>
        <button
          type="button"
          @click="router.back()"
          class="px-6 py-2 bg-gray-200 rounded-lg hover:bg-gray-300"
        >
          Cancel
        </button>
      </div>
    </form>
  </div>
</template>

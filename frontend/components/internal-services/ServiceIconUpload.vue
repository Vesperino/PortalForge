<script setup lang="ts">
import { ref } from 'vue'

const config = useRuntimeConfig()
const authStore = useAuthStore()

interface Props {
  modelValue: string
  maxSizeMB?: number
}

interface Emits {
  (e: 'update:modelValue', value: string): void
}

const props = withDefaults(defineProps<Props>(), {
  maxSizeMB: 5
})

const emit = defineEmits<Emits>()

const fileInput = ref<HTMLInputElement | null>(null)
const isUploading = ref(false)
const uploadError = ref<string | null>(null)
const previewUrl = ref<string | null>(props.modelValue || null)

watch(() => props.modelValue, (newValue) => {
  if (newValue && newValue !== previewUrl.value) {
    previewUrl.value = newValue
  }
})

function triggerFileInput() {
  fileInput.value?.click()
}

async function handleFileChange(event: Event) {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]

  if (!file) return

  // Validate file type
  const acceptedFormats = ['image/jpeg', 'image/png', 'image/webp', 'image/gif', 'image/svg+xml', 'image/x-icon']
  if (!acceptedFormats.includes(file.type)) {
    uploadError.value = `Nieprawidłowy format pliku. Dozwolone: JPG, PNG, WebP, GIF, SVG, ICO`
    return
  }

  // Validate file size
  const fileSizeMB = file.size / (1024 * 1024)
  if (fileSizeMB > props.maxSizeMB) {
    uploadError.value = `Plik jest za duży. Maksymalny rozmiar: ${props.maxSizeMB}MB`
    return
  }

  uploadError.value = null
  isUploading.value = true

  try {
    const formData = new FormData()
    formData.append('file', file)

    const response = await $fetch(`${config.public.apiUrl}/api/storage/upload/service-icon`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${authStore.accessToken}`
      },
      body: formData
    }) as { url: string }

    if (response.url) {
      previewUrl.value = response.url
      emit('update:modelValue', response.url)
    }
  } catch (error: any) {
    console.error('Upload error:', error)
    uploadError.value = error?.data?.message || 'Nie udało się przesłać pliku'
  } finally {
    isUploading.value = false
    if (target) {
      target.value = ''
    }
  }
}

function removeImage() {
  previewUrl.value = null
  emit('update:modelValue', '')
  uploadError.value = null
}
</script>

<template>
  <div class="space-y-2">
    <!-- Upload Area -->
    <div
      v-if="!previewUrl"
      class="border-2 border-dashed border-gray-300 dark:border-gray-600 rounded-lg p-4 text-center hover:border-blue-500 dark:hover:border-blue-400 transition-colors cursor-pointer"
      @click="triggerFileInput"
    >
      <input
        ref="fileInput"
        type="file"
        class="hidden"
        accept="image/jpeg,image/png,image/webp,image/gif,image/svg+xml,image/x-icon"
        @change="handleFileChange"
      >

      <div v-if="!isUploading" class="space-y-2">
        <div class="text-3xl">🖼️</div>
        <p class="text-sm text-gray-600 dark:text-gray-400">
          Kliknij aby wybrać ikonę
        </p>
        <p class="text-xs text-gray-500 dark:text-gray-500">
          Max {{ maxSizeMB }}MB • JPG, PNG, WebP, GIF, SVG, ICO
        </p>
      </div>

      <div v-else class="space-y-2">
        <div class="text-3xl animate-pulse">⏳</div>
        <p class="text-sm text-gray-600 dark:text-gray-400">
          Przesyłanie...
        </p>
      </div>
    </div>

    <!-- Preview -->
    <div v-else class="relative">
      <img
        :src="previewUrl"
        alt="Preview"
        class="w-32 h-32 object-cover rounded-lg border border-gray-300 dark:border-gray-600 mx-auto"
      >
      <button
        type="button"
        class="absolute top-0 right-0 p-1 bg-red-500 text-white rounded-full hover:bg-red-600 transition-colors shadow-lg text-xs"
        title="Usuń obrazek"
        @click="removeImage"
      >
        ❌
      </button>
    </div>

    <!-- Errors -->
    <p v-if="uploadError" class="text-sm text-red-600 dark:text-red-400">{{ uploadError }}</p>
  </div>
</template>

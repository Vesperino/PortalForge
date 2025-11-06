<script setup lang="ts">
import { ref } from 'vue'

const config = useRuntimeConfig()
const authStore = useAuthStore()

interface Props {
  modelValue: string
  label?: string
  required?: boolean
  error?: string
  maxSizeMB?: number
  acceptedFormats?: string[]
}

interface Emits {
  (e: 'update:modelValue', value: string): void
}

const props = withDefaults(defineProps<Props>(), {
  maxSizeMB: 5,
  acceptedFormats: () => ['image/jpeg', 'image/png', 'image/webp', 'image/gif']
})

const emit = defineEmits<Emits>()

const fileInput = ref<HTMLInputElement | null>(null)
const isUploading = ref(false)
const uploadError = ref<string | null>(null)
const previewUrl = ref<string | null>(props.modelValue || null)
const uploadProgress = ref(0)

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
  if (!props.acceptedFormats.includes(file.type)) {
    uploadError.value = `Nieprawidłowy format pliku. Dozwolone: ${props.acceptedFormats.join(', ')}`
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
  uploadProgress.value = 0

  try {
    // Create FormData
    const formData = new FormData()
    formData.append('file', file)

    // Upload to backend
    const response = await $fetch(`${config.public.apiUrl}/api/storage/upload/news-image`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${authStore.accessToken}`
      },
      body: formData
    }) as { url: string }

    if (response.url) {
      previewUrl.value = response.url
      emit('update:modelValue', response.url)
      uploadProgress.value = 100
    }
  } catch (error: any) {
    console.error('Upload error:', error)
    uploadError.value = error?.data?.message || 'Nie udało się przesłać pliku'
  } finally {
    isUploading.value = false
    // Reset file input
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

function handlePaste(event: ClipboardEvent) {
  const items = event.clipboardData?.items
  if (!items) return

  for (let i = 0; i < items.length; i++) {
    const item = items[i]
    if (item && item.type.includes('image')) {
      const file = item.getAsFile()
      if (file) {
        // Create a fake event to reuse handleFileChange
        const fakeEvent = {
          target: {
            files: [file]
          }
        } as any
        handleFileChange(fakeEvent)
      }
      break
    }
  }
}
</script>

<template>
  <div class="space-y-2">
    <label v-if="label" class="block text-sm font-medium text-gray-700 dark:text-gray-300">
      {{ label }}
      <span v-if="required" class="text-red-500">*</span>
    </label>
    
    <!-- Upload Area -->
    <div
      v-if="!previewUrl"
      class="border-2 border-dashed border-gray-300 dark:border-gray-600 rounded-lg p-6 text-center hover:border-blue-500 dark:hover:border-blue-400 transition-colors cursor-pointer"
      :class="{ 'border-red-500 dark:border-red-500': error || uploadError }"
      @click="triggerFileInput"
      @paste="handlePaste"
    >
      <input
        ref="fileInput"
        type="file"
        class="hidden"
        :accept="acceptedFormats.join(',')"
        @change="handleFileChange"
      >
      
      <div v-if="!isUploading" class="space-y-2">
        <div class="text-4xl">📸</div>
        <p class="text-sm text-gray-600 dark:text-gray-400">
          Kliknij aby wybrać plik lub wklej ze schowka (Ctrl+V)
        </p>
        <p class="text-xs text-gray-500 dark:text-gray-500">
          Maksymalny rozmiar: {{ maxSizeMB }}MB
        </p>
        <p class="text-xs text-gray-500 dark:text-gray-500">
          Dozwolone formaty: JPG, PNG, WebP, GIF
        </p>
      </div>
      
      <div v-else class="space-y-2">
        <div class="text-4xl animate-pulse">⏳</div>
        <p class="text-sm text-gray-600 dark:text-gray-400">
          Przesyłanie... {{ uploadProgress }}%
        </p>
        <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
          <div
            class="bg-blue-500 h-2 rounded-full transition-all duration-300"
            :style="{ width: `${uploadProgress}%` }"
          />
        </div>
      </div>
    </div>
    
    <!-- Preview -->
    <div v-else class="relative">
      <img
        :src="previewUrl"
        alt="Preview"
        class="w-full h-64 object-cover rounded-lg border border-gray-300 dark:border-gray-600"
      >
      <button
        type="button"
        class="absolute top-2 right-2 p-2 bg-red-500 text-white rounded-full hover:bg-red-600 transition-colors shadow-lg"
        title="Usuń obrazek"
        @click="removeImage"
      >
        ❌
      </button>
    </div>
    
    <!-- Errors -->
    <p v-if="error" class="text-sm text-red-600 dark:text-red-400">{{ error }}</p>
    <p v-if="uploadError" class="text-sm text-red-600 dark:text-red-400">{{ uploadError }}</p>
    
    <!-- URL Input (fallback) -->
    <details class="text-sm">
      <summary class="cursor-pointer text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white">
        Lub wklej URL obrazka
      </summary>
      <div class="mt-2 flex gap-2">
        <input
          :value="modelValue"
          type="url"
          class="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white text-sm"
          placeholder="https://example.com/image.jpg"
          @input="emit('update:modelValue', ($event.target as HTMLInputElement).value)"
        >
      </div>
    </details>
  </div>
</template>


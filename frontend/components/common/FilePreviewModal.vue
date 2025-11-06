<script setup lang="ts">
import { ref, computed, watch, onUnmounted } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import VuePdfEmbed from 'vue-pdf-embed'
import { renderAsync } from 'docx-preview'

interface Props {
  visible: boolean
  fileUrl: string
  fileName: string
}

const props = defineProps<Props>()

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
}>()

const isLoading = ref(true)
const loadError = ref<string | null>(null)
const docxContainerRef = ref<HTMLElement | null>(null)

// Determine file type from extension
const fileExtension = computed(() => {
  const ext = props.fileName.split('.').pop()?.toLowerCase()
  return ext || ''
})

const fileType = computed(() => {
  const ext = fileExtension.value
  if (['pdf'].includes(ext)) return 'pdf'
  if (['png', 'jpg', 'jpeg', 'gif', 'webp'].includes(ext)) return 'image'
  if (['docx'].includes(ext)) return 'docx'
  if (['doc'].includes(ext)) return 'doc'
  return 'unsupported'
})

const canPreview = computed(() => {
  return ['pdf', 'image', 'docx'].includes(fileType.value)
})

// Handle modal visibility
const localVisible = computed({
  get: () => props.visible,
  set: (value) => emit('update:visible', value)
})

// Handle image load
function onImageLoad() {
  isLoading.value = false
  loadError.value = null
}

function onImageError() {
  isLoading.value = false
  loadError.value = 'Nie udało się załadować obrazu'
}

// Handle PDF load
function onPdfLoad() {
  isLoading.value = false
  loadError.value = null
}

function onPdfError() {
  isLoading.value = false
  loadError.value = 'Nie udało się załadować pliku PDF'
}

// Handle DOCX preview
async function loadDocx() {
  if (!docxContainerRef.value) return

  isLoading.value = true
  loadError.value = null

  try {
    // Fetch the DOCX file as blob
    const response = await fetch(props.fileUrl)
    if (!response.ok) throw new Error('Failed to fetch file')

    const blob = await response.blob()

    // Clear previous content
    docxContainerRef.value.innerHTML = ''

    // Render DOCX to container
    await renderAsync(blob, docxContainerRef.value, undefined, {
      className: 'docx-wrapper',
      inWrapper: true,
      ignoreWidth: false,
      ignoreHeight: false,
      ignoreFonts: false,
      breakPages: true,
      ignoreLastRenderedPageBreak: true,
      experimental: false,
      trimXmlDeclaration: true,
      useBase64URL: false,
      useMathMLPolyfill: false,
      renderHeaders: true,
      renderFooters: true,
      renderFootnotes: true,
      renderEndnotes: true,
      renderComments: false,
      debug: false
    })

    isLoading.value = false
  } catch (error) {
    console.error('Error loading DOCX:', error)
    loadError.value = 'Nie udało się załadować pliku DOCX'
    isLoading.value = false
  }
}

// Watch for file changes and load DOCX if needed
watch([() => props.visible, () => props.fileUrl], ([visible, url]) => {
  if (visible && url && fileType.value === 'docx') {
    // Small delay to ensure DOM is ready
    setTimeout(() => loadDocx(), 100)
  }
})

// Download file
function downloadFile() {
  const link = document.createElement('a')
  link.href = props.fileUrl
  link.download = props.fileName
  link.target = '_blank'
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}

// Reset state when modal is closed
watch(() => props.visible, (visible) => {
  if (!visible) {
    isLoading.value = true
    loadError.value = null
  } else {
    isLoading.value = true
  }
})

// Cleanup
onUnmounted(() => {
  if (docxContainerRef.value) {
    docxContainerRef.value.innerHTML = ''
  }
})
</script>

<template>
  <Dialog
    v-model:visible="localVisible"
    modal
    :header="fileName"
    :style="{ width: '90vw', maxWidth: '1200px' }"
    :maximizable="true"
    :dismissable-mask="true"
    class="file-preview-modal"
  >
    <template #header>
      <div class="flex items-center justify-between w-full pr-2">
        <span class="font-semibold text-lg truncate">{{ fileName }}</span>
        <div class="flex gap-2">
          <Button
            icon="pi pi-download"
            severity="secondary"
            outlined
            size="small"
            title="Pobierz plik"
            @click="downloadFile"
          />
        </div>
      </div>
    </template>

    <div class="file-preview-content">
      <!-- Loading state -->
      <div v-if="isLoading && canPreview" class="flex items-center justify-center py-12">
        <div class="flex flex-col items-center gap-4">
          <i class="pi pi-spin pi-spinner text-4xl text-blue-500" />
          <span class="text-gray-600 dark:text-gray-400">Ładowanie pliku...</span>
        </div>
      </div>

      <!-- Error state -->
      <div v-if="loadError" class="flex items-center justify-center py-12">
        <div class="flex flex-col items-center gap-4 text-center">
          <i class="pi pi-exclamation-triangle text-4xl text-red-500" />
          <span class="text-gray-700 dark:text-gray-300">{{ loadError }}</span>
          <Button
            label="Pobierz plik"
            icon="pi pi-download"
            severity="secondary"
            @click="downloadFile"
          />
        </div>
      </div>

      <!-- Image preview -->
      <div v-if="fileType === 'image' && !loadError" class="image-preview-container">
        <img
          :src="fileUrl"
          :alt="fileName"
          class="max-w-full h-auto rounded-lg shadow-lg"
          :class="{ 'opacity-0': isLoading, 'opacity-100': !isLoading }"
          @load="onImageLoad"
          @error="onImageError"
        >
      </div>

      <!-- PDF preview -->
      <div v-if="fileType === 'pdf' && !loadError" class="pdf-preview-container">
        <VuePdfEmbed
          :source="fileUrl"
          class="pdf-embed"
          @loaded="onPdfLoad"
          @loading-failed="onPdfError"
        />
      </div>

      <!-- DOCX preview -->
      <div v-if="fileType === 'docx' && !loadError" class="docx-preview-container">
        <div
          ref="docxContainerRef"
          class="docx-content"
        />
      </div>

      <!-- Unsupported file type (DOC or other) -->
      <div v-if="!canPreview && !loadError" class="flex items-center justify-center py-12">
        <div class="flex flex-col items-center gap-4 text-center max-w-md">
          <i class="pi pi-file text-6xl text-gray-400" />
          <h3 class="text-xl font-semibold text-gray-700 dark:text-gray-300">
            Podgląd niedostępny
          </h3>
          <p class="text-gray-600 dark:text-gray-400">
            Podgląd plików typu {{ fileExtension.toUpperCase() }} nie jest obsługiwany w przeglądarce.
            Pobierz plik aby go otworzyć.
          </p>
          <Button
            label="Pobierz plik"
            icon="pi pi-download"
            severity="info"
            @click="downloadFile"
          />
        </div>
      </div>
    </div>
  </Dialog>
</template>

<style scoped>
.file-preview-content {
  min-height: 400px;
  max-height: 70vh;
  overflow: auto;
}

.image-preview-container {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 1rem;
  background-color: #f9fafb;
}

.dark .image-preview-container {
  background-color: #1f2937;
}

.image-preview-container img {
  transition: opacity 0.3s ease;
}

.pdf-preview-container {
  width: 100%;
  overflow: auto;
}

.pdf-embed {
  width: 100%;
}

.docx-preview-container {
  padding: 1rem;
  background-color: white;
  overflow: auto;
}

.dark .docx-preview-container {
  background-color: #1f2937;
}

.docx-content {
  width: 100%;
}

/* DOCX styling */
:deep(.docx-wrapper) {
  background: white;
  padding: 2rem;
  box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
  font-family: 'Calibri', 'Arial', sans-serif;
  line-height: 1.5;
}

.dark :deep(.docx-wrapper) {
  background: #374151;
  color: #f3f4f6;
}

:deep(.docx-wrapper section) {
  margin-bottom: 1rem;
}

:deep(.docx-wrapper table) {
  border-collapse: collapse;
  width: 100%;
  margin: 1rem 0;
}

:deep(.docx-wrapper table td),
:deep(.docx-wrapper table th) {
  border: 1px solid #ddd;
  padding: 0.5rem;
}

.dark :deep(.docx-wrapper table td),
.dark :deep(.docx-wrapper table th) {
  border-color: #4b5563;
}

/* Scrollbar styling */
.file-preview-content::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}

.file-preview-content::-webkit-scrollbar-track {
  background: #f1f1f1;
}

.dark .file-preview-content::-webkit-scrollbar-track {
  background: #1f2937;
}

.file-preview-content::-webkit-scrollbar-thumb {
  background: #888;
  border-radius: 4px;
}

.file-preview-content::-webkit-scrollbar-thumb:hover {
  background: #555;
}
</style>

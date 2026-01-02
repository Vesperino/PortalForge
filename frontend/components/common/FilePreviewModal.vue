<script setup lang="ts">
import { ref, computed, watch, onUnmounted } from 'vue'
import { X, Download, ZoomIn, ZoomOut, RotateCw, Maximize2, Minimize2 } from 'lucide-vue-next'

interface Props {
  visible: boolean
  fileUrl: string
  fileName: string
}

const props = defineProps<Props>()

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
}>()

// State
const isLoading = ref(true)
const loadError = ref<string | null>(null)
const docxContainerRef = ref<HTMLElement | null>(null)
const pdfContainerRef = ref<HTMLElement | null>(null)
const isFullscreen = ref(false)
const imageZoom = ref(100)
const imageRotation = ref(0)

// PDF component loaded dynamically
const pdfComponent = ref<unknown>(null)
const pdfSource = ref<string>('')

// Determine file type from extension
const fileExtension = computed(() => {
  const ext = props.fileName.split('.').pop()?.toLowerCase()
  return ext || ''
})

const fileType = computed(() => {
  const ext = fileExtension.value
  if (['pdf'].includes(ext)) return 'pdf'
  if (['png', 'jpg', 'jpeg', 'gif', 'webp', 'bmp', 'svg'].includes(ext)) return 'image'
  if (['docx'].includes(ext)) return 'docx'
  if (['doc'].includes(ext)) return 'doc'
  if (['txt', 'log', 'md'].includes(ext)) return 'text'
  return 'unsupported'
})

const canPreview = computed(() => {
  return ['pdf', 'image', 'docx', 'text'].includes(fileType.value)
})

// Handle modal visibility
const closeModal = () => {
  emit('update:visible', false)
}

// Image controls
const zoomIn = () => {
  if (imageZoom.value < 200) imageZoom.value += 10
}

const zoomOut = () => {
  if (imageZoom.value > 50) imageZoom.value -= 10
}

const rotate = () => {
  imageRotation.value = (imageRotation.value + 90) % 360
}

const toggleFullscreen = () => {
  isFullscreen.value = !isFullscreen.value
}

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
async function loadPdf() {
  if (typeof window === 'undefined') return

  isLoading.value = true
  loadError.value = null

  try {
    // Dynamic import only on client side - use variable to avoid build-time resolution
    const moduleName = 'vue-pdf-embed'
    const VuePdfEmbed = (await import(/* @vite-ignore */ moduleName)).default
    pdfComponent.value = VuePdfEmbed
    pdfSource.value = props.fileUrl
    isLoading.value = false
  } catch (error) {
    console.error('Error loading PDF:', error)
    loadError.value = 'Nie udało się załadować pliku PDF'
    isLoading.value = false
  }
}

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
  if (!docxContainerRef.value || typeof window === 'undefined') return

  isLoading.value = true
  loadError.value = null

  try {
    const response = await fetch(props.fileUrl)
    if (!response.ok) throw new Error('Failed to fetch file')

    const blob = await response.blob()
    docxContainerRef.value.innerHTML = ''

    // Dynamic import only on client side - use variable to avoid build-time resolution
    const moduleName = 'docx-preview'
    const { renderAsync } = await import(/* @vite-ignore */ moduleName)

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

// Load text file
const textContent = ref<string>('')
async function loadTextFile() {
  isLoading.value = true
  loadError.value = null

  try {
    const response = await fetch(props.fileUrl)
    if (!response.ok) throw new Error('Failed to fetch file')

    textContent.value = await response.text()
    isLoading.value = false
  } catch (error) {
    console.error('Error loading text file:', error)
    loadError.value = 'Nie udało się załadować pliku tekstowego'
    isLoading.value = false
  }
}

// Watch for file changes
watch([() => props.visible, () => props.fileUrl], ([visible, url]) => {
  if (visible && url && typeof window !== 'undefined') {
    // Reset state
    imageZoom.value = 100
    imageRotation.value = 0
    isFullscreen.value = false

    if (fileType.value === 'docx') {
      setTimeout(() => loadDocx(), 100)
    } else if (fileType.value === 'text') {
      loadTextFile()
    } else if (fileType.value === 'pdf') {
      loadPdf()
    } else {
      isLoading.value = true
    }
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
    textContent.value = ''
    pdfComponent.value = null
    pdfSource.value = ''
  }
})

// Handle ESC key
const handleKeyDown = (e: KeyboardEvent) => {
  if (e.key === 'Escape' && props.visible) {
    closeModal()
  }
}

// Add/remove event listener
watch(() => props.visible, (visible) => {
  if (visible) {
    window.addEventListener('keydown', handleKeyDown)
  } else {
    window.removeEventListener('keydown', handleKeyDown)
  }
})

// Cleanup
onUnmounted(() => {
  window.removeEventListener('keydown', handleKeyDown)
  if (docxContainerRef.value) {
    docxContainerRef.value.innerHTML = ''
  }
})
</script>

<template>
  <!-- Modal Backdrop -->
  <Transition name="modal-fade">
    <div
      v-if="visible"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/70 backdrop-blur-sm p-4"
      @click.self="closeModal"
    >
      <!-- Modal Container -->
      <div
        :class="[
          'relative bg-white dark:bg-gray-800 rounded-2xl shadow-2xl overflow-hidden transition-all duration-300',
          isFullscreen ? 'w-full h-full' : 'w-[95vw] h-[90vh] max-w-7xl'
        ]"
        @click.stop
      >
        <!-- Header -->
        <div class="flex items-center justify-between px-6 py-4 border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900">
          <div class="flex-1 min-w-0 pr-4">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white truncate">
              {{ fileName }}
            </h3>
            <p class="text-sm text-gray-500 dark:text-gray-400 uppercase">
              {{ fileExtension }} • {{ fileType }}
            </p>
          </div>

          <!-- Toolbar -->
          <div class="flex items-center gap-2">
            <!-- Image Controls (only for images) -->
            <template v-if="fileType === 'image' && !loadError">
              <button
                class="p-2 text-gray-600 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
                title="Pomniejsz"
                @click="zoomOut"
              >
                <ZoomOut class="w-5 h-5" />
              </button>
              <span class="text-sm text-gray-600 dark:text-gray-400 min-w-[4rem] text-center">
                {{ imageZoom }}%
              </span>
              <button
                class="p-2 text-gray-600 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
                title="Powiększ"
                @click="zoomIn"
              >
                <ZoomIn class="w-5 h-5" />
              </button>
              <button
                class="p-2 text-gray-600 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
                title="Obróć"
                @click="rotate"
              >
                <RotateCw class="w-5 h-5" />
              </button>
              <div class="w-px h-6 bg-gray-300 dark:bg-gray-600" />
            </template>

            <!-- Download Button -->
            <button
              class="p-2 text-gray-600 dark:text-gray-400 hover:text-green-600 dark:hover:text-green-400 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
              title="Pobierz plik"
              @click="downloadFile"
            >
              <Download class="w-5 h-5" />
            </button>

            <!-- Fullscreen Toggle -->
            <button
              class="p-2 text-gray-600 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
              :title="isFullscreen ? 'Normalny widok' : 'Pełny ekran'"
              @click="toggleFullscreen"
            >
              <Minimize2 v-if="isFullscreen" class="w-5 h-5" />
              <Maximize2 v-else class="w-5 h-5" />
            </button>

            <!-- Close Button -->
            <button
              class="p-2 text-gray-600 dark:text-gray-400 hover:text-red-600 dark:hover:text-red-400 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
              title="Zamknij (ESC)"
              @click="closeModal"
            >
              <X class="w-5 h-5" />
            </button>
          </div>
        </div>

        <!-- Content -->
        <div class="relative overflow-auto" :style="{ height: 'calc(100% - 73px)' }">
          <!-- Loading State -->
          <div v-if="isLoading && canPreview" class="absolute inset-0 flex items-center justify-center bg-gray-50 dark:bg-gray-900">
            <div class="text-center">
              <div class="w-16 h-16 border-4 border-blue-500 border-t-transparent rounded-full animate-spin mx-auto" />
              <p class="mt-4 text-gray-600 dark:text-gray-400 font-medium">Ładowanie pliku...</p>
            </div>
          </div>

          <!-- Error State -->
          <div v-if="loadError" class="absolute inset-0 flex items-center justify-center bg-gray-50 dark:bg-gray-900">
            <div class="text-center max-w-md px-4">
              <div class="w-20 h-20 mx-auto mb-4 text-red-500">
                <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                </svg>
              </div>
              <h3 class="text-xl font-semibold text-gray-900 dark:text-white mb-2">
                Błąd ładowania
              </h3>
              <p class="text-gray-600 dark:text-gray-400 mb-6">
                {{ loadError }}
              </p>
              <button
                class="px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors inline-flex items-center gap-2"
                @click="downloadFile"
              >
                <Download class="w-5 h-5" />
                Pobierz plik
              </button>
            </div>
          </div>

          <!-- Image Preview -->
          <div
            v-if="fileType === 'image' && !loadError"
            class="flex items-center justify-center min-h-full p-8 bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800"
          >
            <div class="relative">
              <img
                :src="fileUrl"
                :alt="fileName"
                class="max-w-full max-h-[calc(90vh-200px)] object-contain shadow-2xl rounded-lg transition-all duration-300"
                :class="{ 'opacity-0': isLoading, 'opacity-100': !isLoading }"
                :style="{
                  transform: `scale(${imageZoom / 100}) rotate(${imageRotation}deg)`,
                  transformOrigin: 'center'
                }"
                @load="onImageLoad"
                @error="onImageError"
              >
            </div>
          </div>

          <!-- PDF Preview -->
          <ClientOnly>
            <div v-if="fileType === 'pdf' && !loadError && pdfComponent" class="min-h-full bg-gray-100 dark:bg-gray-900 p-4">
              <div ref="pdfContainerRef" class="max-w-5xl mx-auto">
                <component
                  :is="pdfComponent"
                  :source="pdfSource"
                  class="pdf-document shadow-lg"
                  @loaded="onPdfLoad"
                  @loading-failed="onPdfError"
                />
              </div>
            </div>
          </ClientOnly>

          <!-- DOCX Preview -->
          <ClientOnly>
            <div v-if="fileType === 'docx' && !loadError" class="min-h-full bg-gray-50 dark:bg-gray-900 p-8">
              <div class="max-w-4xl mx-auto">
                <div
                  ref="docxContainerRef"
                  class="docx-content bg-white dark:bg-gray-800 shadow-lg rounded-lg"
                />
              </div>
            </div>
          </ClientOnly>

          <!-- Text Preview -->
          <div v-if="fileType === 'text' && !loadError && !isLoading" class="min-h-full bg-gray-50 dark:bg-gray-900 p-8">
            <div class="max-w-4xl mx-auto">
              <pre class="bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 p-6 rounded-lg shadow-lg overflow-x-auto text-sm font-mono leading-relaxed">{{ textContent }}</pre>
            </div>
          </div>

          <!-- Unsupported File Type -->
          <div v-if="!canPreview && !loadError" class="absolute inset-0 flex items-center justify-center bg-gray-50 dark:bg-gray-900">
            <div class="text-center max-w-md px-4">
              <div class="w-24 h-24 mx-auto mb-6 text-gray-400 dark:text-gray-600">
                <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z" />
                </svg>
              </div>
              <h3 class="text-2xl font-bold text-gray-900 dark:text-white mb-3">
                Podgląd niedostępny
              </h3>
              <p class="text-gray-600 dark:text-gray-400 mb-6 leading-relaxed">
                Podgląd plików typu <span class="font-semibold text-gray-900 dark:text-white">{{ fileExtension.toUpperCase() }}</span> nie jest obsługiwany w przeglądarce.
                Pobierz plik, aby go otworzyć na swoim komputerze.
              </p>
              <button
                class="px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors inline-flex items-center gap-2"
                @click="downloadFile"
              >
                <Download class="w-5 h-5" />
                Pobierz plik
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </Transition>
</template>

<style scoped>
/* Modal transition */
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.3s ease;
}

.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}

/* PDF styling */
.pdf-document :deep(.vue-pdf-embed) {
  margin: 0 auto;
}

.pdf-document :deep(.vue-pdf-embed > div) {
  margin-bottom: 1rem;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
}

/* DOCX styling */
.docx-content :deep(.docx-wrapper) {
  padding: 3rem;
  font-family: 'Calibri', 'Arial', sans-serif;
  line-height: 1.6;
  color: #1f2937;
}

.dark .docx-content :deep(.docx-wrapper) {
  color: #f3f4f6;
}

.docx-content :deep(.docx-wrapper section) {
  margin-bottom: 1.5rem;
}

.docx-content :deep(.docx-wrapper table) {
  border-collapse: collapse;
  width: 100%;
  margin: 1.5rem 0;
}

.docx-content :deep(.docx-wrapper table td),
.docx-content :deep(.docx-wrapper table th) {
  border: 1px solid #e5e7eb;
  padding: 0.75rem;
}

.dark .docx-content :deep(.docx-wrapper table td),
.dark .docx-content :deep(.docx-wrapper table th) {
  border-color: #4b5563;
}

.docx-content :deep(.docx-wrapper h1),
.docx-content :deep(.docx-wrapper h2),
.docx-content :deep(.docx-wrapper h3) {
  margin-top: 1.5rem;
  margin-bottom: 1rem;
  font-weight: 600;
}

/* Scrollbar styling */
div::-webkit-scrollbar {
  width: 10px;
  height: 10px;
}

div::-webkit-scrollbar-track {
  background: #f1f5f9;
  border-radius: 5px;
}

.dark div::-webkit-scrollbar-track {
  background: #1e293b;
}

div::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 5px;
}

div::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}

.dark div::-webkit-scrollbar-thumb {
  background: #475569;
}

.dark div::-webkit-scrollbar-thumb:hover {
  background: #64748b;
}
</style>

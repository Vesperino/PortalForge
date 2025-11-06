<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import {
  X,
  Download,
  ChevronLeft,
  ChevronRight,
  ZoomIn,
  ZoomOut,
  Maximize2,
  Minimize2,
  RotateCw,
  Minimize
} from 'lucide-vue-next'

interface Props {
  attachments: string[]
  initialIndex?: number
}

const props = withDefaults(defineProps<Props>(), {
  initialIndex: 0
})

const emit = defineEmits<{
  close: []
}>()

const currentIndex = ref(props.initialIndex)
const zoom = ref(1)
const rotation = ref(0)
const isFullscreen = ref(false)
const isPanning = ref(false)
const panStart = ref({ x: 0, y: 0 })
const panOffset = ref({ x: 0, y: 0 })

const currentAttachment = computed(() => props.attachments[currentIndex.value])

const getFileExtension = (url: string): string => {
  const parts = url.split('.')
  return parts.length > 1 ? parts[parts.length - 1].toLowerCase() : ''
}

const getFileName = (url: string): string => {
  const parts = url.split('/')
  return parts[parts.length - 1]
}

const isImage = computed(() => {
  const ext = getFileExtension(currentAttachment.value)
  return ['jpg', 'jpeg', 'png', 'gif', 'svg', 'webp'].includes(ext)
})

const isPdf = computed(() => {
  const ext = getFileExtension(currentAttachment.value)
  return ext === 'pdf'
})

const canNavigatePrev = computed(() => currentIndex.value > 0)
const canNavigateNext = computed(() => currentIndex.value < props.attachments.length - 1)

const navigatePrev = () => {
  if (canNavigatePrev.value) {
    currentIndex.value--
    resetView()
  }
}

const navigateNext = () => {
  if (canNavigateNext.value) {
    currentIndex.value++
    resetView()
  }
}

const zoomIn = () => {
  zoom.value = Math.min(zoom.value + 0.25, 5)
}

const zoomOut = () => {
  zoom.value = Math.max(zoom.value - 0.25, 0.25)
}

const fitToScreen = () => {
  zoom.value = 1
  rotation.value = 0
  panOffset.value = { x: 0, y: 0 }
}

const rotateImage = () => {
  rotation.value = (rotation.value + 90) % 360
}

const resetView = () => {
  zoom.value = 1
  rotation.value = 0
  panOffset.value = { x: 0, y: 0 }
}

const toggleFullscreen = () => {
  isFullscreen.value = !isFullscreen.value
}

// Pan functionality
const startPan = (e: MouseEvent) => {
  if (!isImage.value || zoom.value <= 1) return
  isPanning.value = true
  panStart.value = {
    x: e.clientX - panOffset.value.x,
    y: e.clientY - panOffset.value.y
  }
}

const doPan = (e: MouseEvent) => {
  if (!isPanning.value) return
  panOffset.value = {
    x: e.clientX - panStart.value.x,
    y: e.clientY - panStart.value.y
  }
}

const endPan = () => {
  isPanning.value = false
}

const handleKeydown = (e: KeyboardEvent) => {
  if (e.key === 'Escape') {
    emit('close')
  } else if (e.key === 'ArrowLeft') {
    navigatePrev()
  } else if (e.key === 'ArrowRight') {
    navigateNext()
  } else if (e.key === '+' || e.key === '=') {
    zoomIn()
  } else if (e.key === '-') {
    zoomOut()
  } else if (e.key === 'r' || e.key === 'R') {
    rotateImage()
  } else if (e.key === 'f' || e.key === 'F') {
    fitToScreen()
  }
}

const downloadFile = () => {
  const link = document.createElement('a')
  link.href = currentAttachment.value
  link.download = getFileName(currentAttachment.value)
  link.target = '_blank'
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}

const imageTransformStyle = computed(() => ({
  transform: `scale(${zoom.value}) rotate(${rotation.value}deg) translate(${panOffset.value.x / zoom.value}px, ${panOffset.value.y / zoom.value}px)`,
  cursor: zoom.value > 1 ? (isPanning.value ? 'grabbing' : 'grab') : 'default'
}))

onMounted(() => {
  window.addEventListener('keydown', handleKeydown)
  window.addEventListener('mousemove', doPan)
  window.addEventListener('mouseup', endPan)
})

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeydown)
  window.removeEventListener('mousemove', doPan)
  window.removeEventListener('mouseup', endPan)
})

watch(() => props.initialIndex, (newIndex) => {
  currentIndex.value = newIndex
  resetView()
})
</script>

<template>
  <div
    class="fixed inset-0 z-50 flex items-center justify-center bg-gradient-to-br from-black/95 via-gray-900/95 to-black/95 backdrop-blur-md"
    @click.self="emit('close')"
  >
    <!-- Modal Container -->
    <div
      :class="[
        'relative flex flex-col bg-gradient-to-b from-gray-800 to-gray-900 shadow-2xl overflow-hidden transition-all duration-300',
        isFullscreen ? 'w-full h-full rounded-none' : 'w-[96vw] h-[96vh] max-w-7xl rounded-2xl border border-gray-700/50'
      ]"
    >
      <!-- Header -->
      <div class="flex items-center justify-between px-6 py-4 bg-gray-800/80 backdrop-blur-sm border-b border-gray-700/50">
        <div class="flex items-center gap-4 flex-1 min-w-0">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 bg-gradient-to-br from-blue-500 to-blue-600 rounded-lg flex items-center justify-center shadow-lg">
              <component
                :is="isImage ? 'svg' : isPdf ? 'svg' : 'svg'"
                class="w-6 h-6 text-white"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  v-if="isImage"
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
                />
                <path
                  v-else-if="isPdf"
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z"
                />
                <path
                  v-else
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                />
              </component>
            </div>
            <div>
              <h3 class="text-base font-semibold text-white truncate max-w-xs lg:max-w-md">
                {{ getFileName(currentAttachment) }}
              </h3>
              <p class="text-xs text-gray-400">
                {{ currentIndex + 1 }} z {{ attachments.length }}
              </p>
            </div>
          </div>
        </div>

        <!-- Toolbar -->
        <div class="flex items-center gap-1.5">
          <!-- Image Controls -->
          <template v-if="isImage">
            <div class="flex items-center gap-1 px-2 py-1 bg-gray-700/50 rounded-lg mr-2">
              <button
                class="p-2 hover:bg-gray-600/50 rounded-lg transition-all text-gray-300 hover:text-white hover:scale-110"
                title="Pomniejsz (−)"
                @click="zoomOut"
              >
                <ZoomOut class="w-4 h-4" />
              </button>
              <span class="text-xs text-gray-300 font-medium min-w-[3.5rem] text-center px-2">
                {{ Math.round(zoom * 100) }}%
              </span>
              <button
                class="p-2 hover:bg-gray-600/50 rounded-lg transition-all text-gray-300 hover:text-white hover:scale-110"
                title="Powiększ (+)"
                @click="zoomIn"
              >
                <ZoomIn class="w-4 h-4" />
              </button>
            </div>

            <button
              class="p-2 hover:bg-gray-700/50 rounded-lg transition-all text-gray-300 hover:text-white hover:scale-110"
              title="Obróć (R)"
              @click="rotateImage"
            >
              <RotateCw class="w-5 h-5" />
            </button>

            <button
              class="p-2 hover:bg-gray-700/50 rounded-lg transition-all text-gray-300 hover:text-white hover:scale-110"
              title="Dopasuj do ekranu (F)"
              @click="fitToScreen"
            >
              <Minimize class="w-5 h-5" />
            </button>

            <div class="w-px h-6 bg-gray-700 mx-1" />
          </template>

          <!-- Fullscreen Toggle -->
          <button
            class="p-2 hover:bg-gray-700/50 rounded-lg transition-all text-gray-300 hover:text-white hover:scale-110"
            :title="isFullscreen ? 'Wyjdź z pełnego ekranu' : 'Pełny ekran'"
            @click="toggleFullscreen"
          >
            <Minimize2 v-if="isFullscreen" class="w-5 h-5" />
            <Maximize2 v-else class="w-5 h-5" />
          </button>

          <!-- Download -->
          <button
            class="p-2 hover:bg-gray-700/50 rounded-lg transition-all text-gray-300 hover:text-white hover:scale-110"
            title="Pobierz"
            @click="downloadFile"
          >
            <Download class="w-5 h-5" />
          </button>

          <div class="w-px h-6 bg-gray-700 mx-1" />

          <!-- Close -->
          <button
            class="p-2 hover:bg-red-600/20 rounded-lg transition-all text-gray-300 hover:text-red-400 hover:scale-110"
            title="Zamknij (ESC)"
            @click="emit('close')"
          >
            <X class="w-5 h-5" />
          </button>
        </div>
      </div>

      <!-- Content -->
      <div class="flex-1 relative overflow-hidden bg-gradient-to-br from-gray-900 via-gray-950 to-black">
        <!-- Navigation Buttons -->
        <button
          v-if="canNavigatePrev"
          class="absolute left-4 top-1/2 -translate-y-1/2 z-20 p-4 bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-500 hover:to-blue-600 rounded-full transition-all shadow-xl hover:shadow-2xl hover:scale-110 text-white group"
          title="Poprzedni (←)"
          @click="navigatePrev"
        >
          <ChevronLeft class="w-6 h-6 group-hover:scale-110 transition-transform" />
        </button>

        <button
          v-if="canNavigateNext"
          class="absolute right-4 top-1/2 -translate-y-1/2 z-20 p-4 bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-500 hover:to-blue-600 rounded-full transition-all shadow-xl hover:shadow-2xl hover:scale-110 text-white group"
          title="Następny (→)"
          @click="navigateNext"
        >
          <ChevronRight class="w-6 h-6 group-hover:scale-110 transition-transform" />
        </button>

        <!-- Image Viewer -->
        <div
          v-if="isImage"
          class="w-full h-full flex items-center justify-center p-8 overflow-hidden"
          @mousedown="startPan"
        >
          <img
            :src="currentAttachment"
            :alt="getFileName(currentAttachment)"
            :style="imageTransformStyle"
            class="max-w-full max-h-full object-contain transition-transform duration-200 select-none"
            draggable="false"
          >
        </div>

        <!-- PDF Viewer -->
        <div v-else-if="isPdf" class="w-full h-full p-4">
          <div class="w-full h-full bg-white rounded-lg overflow-hidden shadow-2xl">
            <iframe
              :src="currentAttachment"
              class="w-full h-full border-0"
              title="PDF Viewer"
            />
          </div>
        </div>

        <!-- Unsupported File Type -->
        <div v-else class="w-full h-full flex items-center justify-center text-center p-8">
          <div class="max-w-md">
            <div class="w-24 h-24 bg-gradient-to-br from-gray-700 to-gray-800 rounded-2xl flex items-center justify-center mx-auto mb-6 shadow-xl">
              <Download class="w-12 h-12 text-gray-400" />
            </div>
            <h3 class="text-2xl font-bold text-white mb-3">
              Podgląd niedostępny
            </h3>
            <p class="text-gray-400 mb-8 leading-relaxed">
              Ten typ pliku ({{ getFileExtension(currentAttachment).toUpperCase() }}) nie może być wyświetlony w przeglądarce. Pobierz plik, aby go otworzyć.
            </p>
            <button
              class="px-8 py-4 bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-500 hover:to-blue-600 text-white rounded-xl font-semibold transition-all shadow-xl hover:shadow-2xl hover:scale-105 inline-flex items-center gap-3"
              @click="downloadFile"
            >
              <Download class="w-5 h-5" />
              Pobierz plik
            </button>
          </div>
        </div>
      </div>

      <!-- Footer (thumbnails for multiple files) -->
      <div
        v-if="attachments.length > 1"
        class="px-6 py-4 bg-gray-800/80 backdrop-blur-sm border-t border-gray-700/50 overflow-x-auto scrollbar-thin scrollbar-thumb-gray-700 scrollbar-track-gray-800"
      >
        <div class="flex gap-3 min-w-max">
          <button
            v-for="(attachment, index) in attachments"
            :key="index"
            :class="[
              'flex-shrink-0 w-24 h-24 rounded-xl overflow-hidden border-2 transition-all hover:scale-110 hover:shadow-xl relative group',
              index === currentIndex
                ? 'border-blue-500 ring-4 ring-blue-500/30 shadow-xl scale-105'
                : 'border-gray-600/50 hover:border-blue-400/50'
            ]"
            @click="currentIndex = index; resetView()"
          >
            <img
              v-if="['jpg', 'jpeg', 'png', 'gif', 'svg', 'webp'].includes(getFileExtension(attachment))"
              :src="attachment"
              :alt="`Miniatura ${index + 1}`"
              class="w-full h-full object-cover"
            >
            <div
              v-else
              class="w-full h-full bg-gradient-to-br from-gray-700 to-gray-800 flex items-center justify-center"
            >
              <div class="text-center">
                <component
                  :is="getFileExtension(attachment) === 'pdf' ? 'svg' : 'svg'"
                  class="w-8 h-8 text-gray-400 mx-auto mb-1"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    v-if="getFileExtension(attachment) === 'pdf'"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z"
                  />
                  <path
                    v-else
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                  />
                </component>
                <span class="text-xs text-gray-300 font-medium uppercase">
                  {{ getFileExtension(attachment) }}
                </span>
              </div>
            </div>
            <div
              v-if="index === currentIndex"
              class="absolute inset-0 bg-blue-500/10 pointer-events-none"
            />
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Custom scrollbar */
.scrollbar-thin::-webkit-scrollbar {
  height: 8px;
}

.scrollbar-track-gray-800::-webkit-scrollbar-track {
  background: rgb(31 41 55);
  border-radius: 4px;
}

.scrollbar-thumb-gray-700::-webkit-scrollbar-thumb {
  background: rgb(55 65 81);
  border-radius: 4px;
}

.scrollbar-thumb-gray-700::-webkit-scrollbar-thumb:hover {
  background: rgb(75 85 99);
}
</style>

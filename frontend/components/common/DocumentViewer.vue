<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { X, Download, ChevronLeft, ChevronRight, ZoomIn, ZoomOut, Maximize2, Minimize2 } from 'lucide-vue-next'

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
const isFullscreen = ref(false)

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
    resetZoom()
  }
}

const navigateNext = () => {
  if (canNavigateNext.value) {
    currentIndex.value++
    resetZoom()
  }
}

const zoomIn = () => {
  zoom.value = Math.min(zoom.value + 0.25, 3)
}

const zoomOut = () => {
  zoom.value = Math.max(zoom.value - 0.25, 0.5)
}

const resetZoom = () => {
  zoom.value = 1
}

const toggleFullscreen = () => {
  isFullscreen.value = !isFullscreen.value
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

onMounted(() => {
  window.addEventListener('keydown', handleKeydown)
})

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeydown)
})

watch(() => props.initialIndex, (newIndex) => {
  currentIndex.value = newIndex
  resetZoom()
})
</script>

<template>
  <div
    class="fixed inset-0 z-50 flex items-center justify-center bg-black/90 backdrop-blur-sm"
    @click.self="emit('close')"
  >
    <!-- Modal Container -->
    <div
      :class="[
        'relative flex flex-col bg-gray-900 rounded-lg shadow-2xl overflow-hidden transition-all',
        isFullscreen ? 'w-full h-full rounded-none' : 'w-[95vw] h-[95vh] max-w-7xl'
      ]"
    >
      <!-- Header -->
      <div class="flex items-center justify-between px-6 py-4 bg-gray-800 border-b border-gray-700">
        <div class="flex items-center gap-4 flex-1 min-w-0">
          <h3 class="text-lg font-semibold text-white truncate">
            {{ getFileName(currentAttachment) }}
          </h3>
          <span class="text-sm text-gray-400">
            {{ currentIndex + 1 }} / {{ attachments.length }}
          </span>
        </div>

        <!-- Toolbar -->
        <div class="flex items-center gap-2">
          <!-- Zoom Controls (only for images) -->
          <template v-if="isImage">
            <button
              class="p-2 hover:bg-gray-700 rounded-lg transition-colors text-gray-300 hover:text-white"
              title="Pomniejsz (−)"
              @click="zoomOut"
            >
              <ZoomOut class="w-5 h-5" />
            </button>
            <span class="text-sm text-gray-400 min-w-[4rem] text-center">
              {{ Math.round(zoom * 100) }}%
            </span>
            <button
              class="p-2 hover:bg-gray-700 rounded-lg transition-colors text-gray-300 hover:text-white"
              title="Powiększ (+)"
              @click="zoomIn"
            >
              <ZoomIn class="w-5 h-5" />
            </button>
          </template>

          <!-- Fullscreen Toggle -->
          <button
            class="p-2 hover:bg-gray-700 rounded-lg transition-colors text-gray-300 hover:text-white"
            :title="isFullscreen ? 'Wyjdź z pełnego ekranu' : 'Pełny ekran'"
            @click="toggleFullscreen"
          >
            <Minimize2 v-if="isFullscreen" class="w-5 h-5" />
            <Maximize2 v-else class="w-5 h-5" />
          </button>

          <!-- Download -->
          <button
            class="p-2 hover:bg-gray-700 rounded-lg transition-colors text-gray-300 hover:text-white"
            title="Pobierz"
            @click="downloadFile"
          >
            <Download class="w-5 h-5" />
          </button>

          <!-- Close -->
          <button
            class="p-2 hover:bg-gray-700 rounded-lg transition-colors text-gray-300 hover:text-white"
            title="Zamknij (ESC)"
            @click="emit('close')"
          >
            <X class="w-5 h-5" />
          </button>
        </div>
      </div>

      <!-- Content -->
      <div class="flex-1 relative overflow-hidden bg-gray-950">
        <!-- Navigation Buttons -->
        <button
          v-if="canNavigatePrev"
          class="absolute left-4 top-1/2 -translate-y-1/2 z-10 p-3 bg-gray-800/90 hover:bg-gray-700 rounded-full transition-colors text-white disabled:opacity-50 disabled:cursor-not-allowed"
          title="Poprzedni (←)"
          @click="navigatePrev"
        >
          <ChevronLeft class="w-6 h-6" />
        </button>

        <button
          v-if="canNavigateNext"
          class="absolute right-4 top-1/2 -translate-y-1/2 z-10 p-3 bg-gray-800/90 hover:bg-gray-700 rounded-full transition-colors text-white disabled:opacity-50 disabled:cursor-not-allowed"
          title="Następny (→)"
          @click="navigateNext"
        >
          <ChevronRight class="w-6 h-6" />
        </button>

        <!-- Image Viewer -->
        <div
          v-if="isImage"
          class="w-full h-full flex items-center justify-center p-8 overflow-auto"
        >
          <img
            :src="currentAttachment"
            :alt="getFileName(currentAttachment)"
            :style="{ transform: `scale(${zoom})` }"
            class="max-w-full max-h-full object-contain transition-transform duration-200"
          />
        </div>

        <!-- PDF Viewer -->
        <div v-else-if="isPdf" class="w-full h-full">
          <iframe
            :src="currentAttachment"
            class="w-full h-full border-0"
            title="PDF Viewer"
          />
        </div>

        <!-- Unsupported File Type -->
        <div v-else class="w-full h-full flex items-center justify-center text-center p-8">
          <div>
            <div class="w-20 h-20 bg-gray-800 rounded-full flex items-center justify-center mx-auto mb-4">
              <Download class="w-10 h-10 text-gray-400" />
            </div>
            <h3 class="text-xl font-semibold text-white mb-2">
              Podgląd niedostępny
            </h3>
            <p class="text-gray-400 mb-6">
              Nie można wyświetlić tego typu pliku w przeglądarce
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

      <!-- Footer (thumbnails for multiple files) -->
      <div
        v-if="attachments.length > 1"
        class="px-6 py-4 bg-gray-800 border-t border-gray-700 overflow-x-auto"
      >
        <div class="flex gap-3">
          <button
            v-for="(attachment, index) in attachments"
            :key="index"
            :class="[
              'flex-shrink-0 w-20 h-20 rounded-lg overflow-hidden border-2 transition-all hover:scale-105',
              index === currentIndex
                ? 'border-blue-500 ring-2 ring-blue-500/50'
                : 'border-gray-600 hover:border-gray-500'
            ]"
            @click="currentIndex = index; resetZoom()"
          >
            <img
              v-if="['jpg', 'jpeg', 'png', 'gif', 'svg', 'webp'].includes(getFileExtension(attachment))"
              :src="attachment"
              :alt="`Thumbnail ${index + 1}`"
              class="w-full h-full object-cover"
            />
            <div
              v-else
              class="w-full h-full bg-gray-700 flex items-center justify-center text-xs text-gray-300 uppercase font-medium"
            >
              {{ getFileExtension(attachment) }}
            </div>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

const { getSettings, updateSettings, testStorage, storageSettings, isLoading } = useSystemSettings()

const localSettings = ref<Record<string, string>>({})
const isTesting = ref(false)
const testResult = ref<any>(null)
const saveMessage = ref<string | null>(null)
const saveError = ref<string | null>(null)

onMounted(async () => {
  await loadSettings()
})

async function loadSettings() {
  try {
    await getSettings()
    
    // Initialize local settings
    storageSettings.value.forEach(setting => {
      localSettings.value[setting.key] = setting.value
    })
  } catch (error) {
    console.error('Failed to load settings:', error)
  }
}

async function handleSave() {
  saveMessage.value = null
  saveError.value = null

  try {
    const updates = Object.entries(localSettings.value).map(([key, value]) => ({
      key,
      value
    }))

    await updateSettings(updates)
    saveMessage.value = 'Ustawienia zapisane pomyślnie!'
    
    setTimeout(() => {
      saveMessage.value = null
    }, 3000)
  } catch (error: any) {
    saveError.value = error?.message || 'Nie udało się zapisać ustawień'
  }
}

async function handleTestStorage() {
  isTesting.value = true
  testResult.value = null

  try {
    testResult.value = await testStorage()
  } catch (error) {
    console.error('Storage test failed:', error)
    testResult.value = {
      success: false,
      message: 'Test nie powiódł się'
    }
  } finally {
    isTesting.value = false
  }
}

const fullBasePath = computed(() => localSettings.value['Storage:BasePath'] || '/app/storage')
const newsImagesFullPath = computed(() => {
  const base = localSettings.value['Storage:BasePath'] || '/app/storage'
  const sub = localSettings.value['Storage:NewsImagesPath'] || 'images'
  return `${base}/${sub}`
})
const documentsFullPath = computed(() => {
  const base = localSettings.value['Storage:BasePath'] || '/app/storage'
  const sub = localSettings.value['Storage:DocumentsPath'] || 'documents'
  return `${base}/${sub}`
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
          Ustawienia systemowe
        </h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          Konfiguracja globalnych ustawień systemu PortalForge
        </p>
      </div>
      <NuxtLink
        to="/admin"
        class="px-4 py-2 text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white transition"
      >
        ← Powrót do panelu
      </NuxtLink>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading && !localSettings['Storage:BasePath']" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto" />
      <p class="mt-4 text-gray-600 dark:text-gray-400">
        Ładowanie ustawień...
      </p>
    </div>

    <!-- Settings Form -->
    <div v-else class="bg-white dark:bg-gray-800 rounded-lg shadow-md">
      <div class="p-6 border-b border-gray-200 dark:border-gray-700">
        <h2 class="text-xl font-semibold text-gray-900 dark:text-white flex items-center gap-2">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 19a2 2 0 01-2-2V7a2 2 0 012-2h4l2 2h4a2 2 0 012 2v1M5 19h14a2 2 0 002-2v-5a2 2 0 00-2-2H9a2 2 0 00-2 2v5a2 2 0 01-2 2z" />
          </svg>
          Ustawienia Storage
        </h2>
        <p class="mt-1 text-sm text-gray-600 dark:text-gray-400">
          Konfiguracja ścieżek do przechowywania plików uploadowanych przez użytkowników
        </p>
      </div>

      <form class="p-6 space-y-6" @submit.prevent="handleSave">
        <!-- Success/Error Messages -->
        <div v-if="saveMessage" class="p-4 bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200 rounded-lg">
          {{ saveMessage }}
        </div>
        <div v-if="saveError" class="p-4 bg-red-100 dark:bg-red-900 text-red-800 dark:text-red-200 rounded-lg">
          {{ saveError }}
        </div>

        <!-- Base Path -->
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Ścieżka bazowa (Base Path)
          </label>
          <input
            v-model="localSettings['Storage:BasePath']"
            type="text"
            class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white font-mono text-sm"
            placeholder="/app/storage"
          >
          <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
            Absolutna ścieżka do głównego katalogu storage <strong>(wewnątrz kontenera Docker)</strong>
          </p>
          <div class="mt-2 p-3 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg">
            <p class="text-xs text-blue-800 dark:text-blue-200">
              <strong>Domyślnie:</strong> <code class="bg-blue-100 dark:bg-blue-900 px-1 py-0.5 rounded">/app/storage</code> (ścieżka w kontenerze)<br>
              <strong>Na hoście VPS:</strong> <code class="bg-blue-100 dark:bg-blue-900 px-1 py-0.5 rounded">~/portalforge/storage</code> (zmapowane przez Docker volume)
            </p>
            <p class="mt-2 text-xs text-blue-700 dark:text-blue-300">
              💡 Aby zmienić lokalizację na większy dysk (np. <code class="bg-blue-100 dark:bg-blue-900 px-1 py-0.5 rounded">/mnt/company-data</code>),
              zrestartuj kontener z nowym volume: <code class="bg-blue-100 dark:bg-blue-900 px-1 py-0.5 rounded">-v /mnt/company-data/storage:/app/storage</code>
            </p>
          </div>
        </div>

        <!-- News Images Path -->
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Ścieżka obrazków newsów
          </label>
          <input
            v-model="localSettings['Storage:NewsImagesPath']"
            type="text"
            class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white font-mono text-sm"
            placeholder="images"
          >
          <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
            Relatywna ścieżka do Base Path
          </p>
          <p class="mt-1 text-xs text-blue-600 dark:text-blue-400 font-mono">
            Pełna ścieżka: {{ newsImagesFullPath }}
          </p>
        </div>

        <!-- Documents Path -->
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Ścieżka dokumentów
          </label>
          <input
            v-model="localSettings['Storage:DocumentsPath']"
            type="text"
            class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white font-mono text-sm"
            placeholder="documents"
          >
          <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
            Relatywna ścieżka do Base Path
          </p>
          <p class="mt-1 text-xs text-blue-600 dark:text-blue-400 font-mono">
            Pełna ścieżka: {{ documentsFullPath }}
          </p>
        </div>

        <!-- Max File Size -->
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Maksymalny rozmiar pliku (MB)
          </label>
          <input
            v-model.number="localSettings['Storage:MaxFileSizeMB']"
            type="number"
            min="1"
            max="100"
            class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            placeholder="10"
          >
          <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
            Maksymalny rozmiar pojedynczego pliku w megabajtach
          </p>
        </div>

        <!-- Actions -->
        <div class="flex gap-4 pt-4 border-t border-gray-200 dark:border-gray-700">
          <button
            type="submit"
            class="px-6 py-3 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:bg-gray-400 disabled:cursor-not-allowed transition font-medium"
            :disabled="isLoading"
          >
            {{ isLoading ? 'Zapisywanie...' : 'Zapisz ustawienia' }}
          </button>
          
          <button
            type="button"
            class="px-6 py-3 bg-green-500 text-white rounded-lg hover:bg-green-600 disabled:bg-gray-400 disabled:cursor-not-allowed transition font-medium"
            :disabled="isTesting"
            @click="handleTestStorage"
          >
            {{ isTesting ? 'Testowanie...' : 'Testuj Storage' }}
          </button>
        </div>
      </form>

      <!-- Test Results -->
      <div v-if="testResult" class="p-6 border-t border-gray-200 dark:border-gray-700">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Wyniki testu storage
        </h3>

        <div
          :class="[
            'p-4 rounded-lg mb-4',
            testResult.success 
              ? 'bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200' 
              : 'bg-red-100 dark:bg-red-900 text-red-800 dark:text-red-200'
          ]"
        >
          <div class="flex items-center gap-2 font-semibold">
            <svg v-if="testResult.success" class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
            </svg>
            <svg v-else class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
            {{ testResult.success ? 'Test zakończony sukcesem' : 'Test nie powiódł się' }}
          </div>
          <p v-if="testResult.message" class="mt-2 text-sm">
            {{ testResult.message }}
          </p>
        </div>

        <!-- Base Path Status -->
        <div class="space-y-3">
          <div class="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-900 rounded-lg">
            <div class="flex items-center gap-2">
              <svg
                :class="testResult.basePathExists ? 'text-green-500' : 'text-red-500'"
                class="w-5 h-5"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" v-if="testResult.basePathExists" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" v-else />
              </svg>
              <span class="text-gray-700 dark:text-gray-300 font-medium">Base Path</span>
            </div>
            <span class="text-sm text-gray-600 dark:text-gray-400 font-mono">{{ testResult.basePath }}</span>
          </div>

          <div class="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-900 rounded-lg">
            <div class="flex items-center gap-2">
              <svg
                :class="testResult.basePathWritable ? 'text-green-500' : 'text-red-500'"
                class="w-5 h-5"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" v-if="testResult.basePathWritable" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" v-else />
              </svg>
              <span class="text-gray-700 dark:text-gray-300 font-medium">Możliwość zapisu</span>
            </div>
            <span class="text-sm text-gray-600 dark:text-gray-400">
              {{ testResult.basePathWritable ? 'Tak' : 'Nie' }}
            </span>
          </div>

          <!-- Subdirectories -->
          <div v-if="testResult.subdirectories" class="space-y-2">
            <h4 class="text-sm font-medium text-gray-700 dark:text-gray-300 mt-4 mb-2">Podkatalogi:</h4>
            <div
              v-for="subdir in testResult.subdirectories"
              :key="subdir.name"
              class="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-900 rounded-lg"
            >
              <div class="flex items-center gap-2">
                <svg
                  :class="subdir.exists ? 'text-green-500' : 'text-red-500'"
                  class="w-5 h-5"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" v-if="subdir.exists" />
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" v-else />
                </svg>
                <span class="text-gray-700 dark:text-gray-300">{{ subdir.name }}</span>
              </div>
              <div class="text-right">
                <p class="text-sm text-gray-600 dark:text-gray-400 font-mono">{{ subdir.fullPath }}</p>
                <p v-if="subdir.error" class="text-xs text-red-600 dark:text-red-400">{{ subdir.error }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>





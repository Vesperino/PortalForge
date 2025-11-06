<script setup lang="ts">
definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

const { getSettings, updateSettings, testStorage, storageSettings, aiSettings, isLoading } = useSystemSettings()
const { testConnection } = useChatAI()

const activeTab = ref<'storage' | 'ai'>('storage')
const localSettings = ref<Record<string, string>>({})
const isTesting = ref(false)
const testResult = ref<any>(null)
const saveMessage = ref<string | null>(null)
const saveError = ref<string | null>(null)
const isTestingConnection = ref(false)
const connectionTestResult = ref<boolean | null>(null)

onMounted(async () => {
  await loadSettings()
})

async function loadSettings() {
  try {
    await getSettings()

    // Initialize local settings for Storage
    storageSettings.value.forEach(setting => {
      localSettings.value[setting.key] = setting.value
    })

    // Initialize local settings for AI
    aiSettings.value.forEach(setting => {
      localSettings.value[setting.key] = setting.value
    })
  }
  catch (error) {
    console.error('Failed to load settings:', error)
  }
}

async function handleSave() {
  saveMessage.value = null
  saveError.value = null

  try {
    const updates = Object.entries(localSettings.value)
      // Filter out empty API key (don't update if user didn't enter new one)
      .filter(([key, value]) => !(key === 'AI:OpenAIApiKey' && !value))
      .map(([key, value]) => ({
        key,
        value
      }))

    await updateSettings(updates)
    saveMessage.value = 'Ustawienia zapisane pomyślnie!'

    // Clear API key field after successful save
    localSettings.value['AI:OpenAIApiKey'] = ''

    setTimeout(() => {
      saveMessage.value = null
    }, 3000)
  }
  catch (error: any) {
    saveError.value = error?.message || 'Nie udało się zapisać ustawień'
  }
}

async function handleTestStorage() {
  isTesting.value = true
  testResult.value = null

  try {
    testResult.value = await testStorage()
  }
  catch (error) {
    console.error('Storage test failed:', error)
    testResult.value = {
      success: false,
      message: 'Test nie powiódł się'
    }
  }
  finally {
    isTesting.value = false
  }
}

async function handleTestConnection() {
  isTestingConnection.value = true
  connectionTestResult.value = null

  try {
    const result = await testConnection()
    connectionTestResult.value = result
  }
  catch (error) {
    console.error('Connection test failed:', error)
    connectionTestResult.value = false
  }
  finally {
    isTestingConnection.value = false
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

    <!-- Category Tabs -->
    <div v-else class="bg-white dark:bg-gray-800 rounded-lg shadow-md">
      <div class="border-b border-gray-200 dark:border-gray-700">
        <nav class="flex -mb-px">
          <button
            :class="[
              'px-6 py-4 text-sm font-medium border-b-2 transition',
              activeTab === 'storage'
                ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300'
            ]"
            @click="activeTab = 'storage'"
          >
            <div class="flex items-center gap-2">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 19a2 2 0 01-2-2V7a2 2 0 012-2h4l2 2h4a2 2 0 012 2v1M5 19h14a2 2 0 002-2v-5a2 2 0 00-2-2H9a2 2 0 00-2 2v5a2 2 0 01-2 2z" />
              </svg>
              Storage
            </div>
          </button>
          <button
            :class="[
              'px-6 py-4 text-sm font-medium border-b-2 transition',
              activeTab === 'ai'
                ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300'
            ]"
            @click="activeTab = 'ai'"
          >
            <div class="flex items-center gap-2">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
              </svg>
              Asystent AI
            </div>
          </button>
        </nav>
      </div>

      <!-- Storage Tab -->
      <div v-if="activeTab === 'storage'">
        <div class="p-6 border-b border-gray-200 dark:border-gray-700">
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white">
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
                <strong>Pełna ścieżka:</strong> <code class="bg-blue-100 dark:bg-blue-900 px-1 py-0.5 rounded">{{ fullBasePath }}</code>
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
        </div>
      </div>

      <!-- AI Tab -->
      <div v-else-if="activeTab === 'ai'">
        <div class="p-6 border-b border-gray-200 dark:border-gray-700">
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white">
            Konfiguracja Asystenta AI
          </h2>
          <p class="mt-1 text-sm text-gray-600 dark:text-gray-400">
            Integracja z OpenAI dla funkcji tłumaczenia i czatu AI
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

          <!-- OpenAI API Key -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              OpenAI API Key
            </label>
            <input
              v-model="localSettings['AI:OpenAIApiKey']"
              type="password"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white font-mono text-sm"
              placeholder="Wpisz nowy klucz API (zostanie zaszyfrowany)"
            >
            <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
              Klucz API z platformy OpenAI. Pozostaw puste aby zachować obecny klucz.
            </p>
            <div class="mt-2 p-3 bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-lg">
              <p class="text-xs text-yellow-800 dark:text-yellow-200">
                <strong>🔒 Bezpieczeństwo:</strong> Klucz jest szyfrowany AES-256 przed zapisem. Z powodów bezpieczeństwa obecny klucz nie jest wyświetlany.
              </p>
            </div>
          </div>

          <!-- Translation Model -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Model do tłumaczenia
            </label>
            <select
              v-model="localSettings['AI:TranslationModel']"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            >
              <option value="gpt-5-mini-2025-08-07">GPT-5 Mini</option>
            </select>
            <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
              Model OpenAI używany do tłumaczenia tekstów
            </p>
          </div>

          <!-- Chat Model -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Model do czatu
            </label>
            <select
              v-model="localSettings['AI:ChatModel']"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            >
              <option value="gpt-5-2025-08-07">GPT-5</option>
              <option value="gpt-5-mini-2025-08-07">GPT-5 Mini</option>
              <option value="gpt-4.1-nano-2025-04-14">GPT-4.1 Nano</option>
            </select>
            <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
              Model OpenAI używany do standardowego czatu
            </p>
          </div>

          <!-- Max Tokens -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Maksymalna liczba tokenów na zapytanie
            </label>
            <input
              v-model.number="localSettings['AI:MaxTokensPerRequest']"
              type="number"
              min="100"
              max="8000"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="4000"
            >
            <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
              Maksymalna liczba tokenów w pojedynczym zapytaniu do AI
            </p>
          </div>

          <!-- Translation Max Characters -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Maksymalna długość tekstu do tłumaczenia (znaki)
            </label>
            <input
              v-model.number="localSettings['AI:TranslationMaxCharacters']"
              type="number"
              min="1000"
              max="20000"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="8000"
            >
            <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
              Limit znaków dla pojedynczego tłumaczenia (zalecane: 8000)
            </p>
          </div>

          <!-- Temperature -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Temperatura (0.0 - 1.0)
            </label>
            <input
              v-model.number="localSettings['AI:Temperature']"
              type="number"
              min="0"
              max="1"
              step="0.1"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="0.7"
            >
            <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
              Kreatywność odpowiedzi AI. Niższe wartości = bardziej deterministyczne odpowiedzi.
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
              :disabled="isTestingConnection"
              @click="handleTestConnection"
            >
              {{ isTestingConnection ? 'Testowanie...' : 'Testuj połączenie' }}
            </button>
          </div>

          <!-- Connection Test Result -->
          <div v-if="connectionTestResult !== null" class="mt-4">
            <div
              :class="[
                'p-4 rounded-lg',
                connectionTestResult
                  ? 'bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200'
                  : 'bg-red-100 dark:bg-red-900 text-red-800 dark:text-red-200'
              ]"
            >
              <div class="flex items-center gap-2 font-semibold">
                <svg v-if="connectionTestResult" class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                </svg>
                <svg v-else class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
                {{ connectionTestResult ? 'Połączenie z OpenAI działa poprawnie!' : 'Nie udało się połączyć z OpenAI' }}
              </div>
              <p class="mt-2 text-sm">
                {{ connectionTestResult
                  ? 'Klucz API jest poprawny i można korzystać z funkcji AI.'
                  : 'Sprawdź klucz API lub połączenie internetowe.'
                }}
              </p>
            </div>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

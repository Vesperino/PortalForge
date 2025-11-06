<script setup lang="ts">
definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

const { translateText, sendChatMessage, isTranslating, isChatting, translationResult, chatResult, error } = useChatAI()
const { getSettingValue, getSettings } = useSystemSettings()

const activeTab = ref<'translate' | 'chat'>('translate')

// Translation
const textToTranslate = ref('')
const targetLanguage = ref('Polish')
const maxChars = ref(8000)

// Chat
const chatMessage = ref('')
const chatHistory = ref<Array<{ role: string; content: string }>>([])
const chatMessagesContainer = ref<HTMLElement | null>(null)

onMounted(async () => {
  await getSettings()
  const maxCharsStr = getSettingValue('AI:TranslationMaxCharacters', '8000')
  maxChars.value = Number.parseInt(maxCharsStr) || 8000
})

async function handleTranslate() {
  if (!textToTranslate.value || !targetLanguage.value) {
    return
  }

  await translateText(textToTranslate.value, targetLanguage.value)
}

async function handleSendMessage() {
  if (!chatMessage.value.trim()) {
    return
  }

  const userMessage = chatMessage.value.trim()

  // Add user message to history
  chatHistory.value.push({
    role: 'user',
    content: userMessage
  })

  chatMessage.value = ''

  // Scroll to bottom
  nextTick(() => {
    if (chatMessagesContainer.value) {
      chatMessagesContainer.value.scrollTop = chatMessagesContainer.value.scrollHeight
    }
  })

  // Send message to AI
  await sendChatMessage(userMessage, chatHistory.value)

  // Add AI response to history
  if (chatResult.value) {
    chatHistory.value.push({
      role: 'assistant',
      content: chatResult.value
    })

    // Scroll to bottom again after response
    nextTick(() => {
      if (chatMessagesContainer.value) {
        chatMessagesContainer.value.scrollTop = chatMessagesContainer.value.scrollHeight
      }
    })
  }
}

function clearChat() {
  chatHistory.value = []
}

const charactersRemaining = computed(() => maxChars.value - textToTranslate.value.length)
const isOverLimit = computed(() => charactersRemaining.value < 0)
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
          Asystent AI
        </h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          Tłumaczenie tekstów i rozmowa z AI wspomaganym przez OpenAI
        </p>
      </div>
    </div>

    <!-- Tabs Container -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md">
      <!-- Tab Navigation -->
      <div class="border-b border-gray-200 dark:border-gray-700">
        <nav class="flex -mb-px">
          <button
            :class="[
              'px-6 py-4 text-sm font-medium border-b-2 transition',
              activeTab === 'translate'
                ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300'
            ]"
            @click="activeTab = 'translate'"
          >
            <div class="flex items-center gap-2">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5h12M9 3v2m1.048 9.5A18.022 18.022 0 016.412 9m6.088 9h7M11 21l5-10 5 10M12.751 5C11.783 10.77 8.07 15.61 3 18.129" />
              </svg>
              Tłumaczenie
            </div>
          </button>
          <button
            :class="[
              'px-6 py-4 text-sm font-medium border-b-2 transition',
              activeTab === 'chat'
                ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300'
            ]"
            @click="activeTab = 'chat'"
          >
            <div class="flex items-center gap-2">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
              </svg>
              Chat AI
            </div>
          </button>
        </nav>
      </div>

      <!-- Translation Tab -->
      <div v-if="activeTab === 'translate'" class="p-6 space-y-6">
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Tekst do przetłumaczenia
            <span
              class="float-right text-xs"
              :class="isOverLimit ? 'text-red-500 font-semibold' : 'text-gray-500 dark:text-gray-400'"
            >
              {{ charactersRemaining }} znaków pozostało (limit: {{ maxChars }})
            </span>
          </label>
          <textarea
            v-model="textToTranslate"
            rows="10"
            class="w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
            :class="isOverLimit
              ? 'border-red-500 dark:border-red-500'
              : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white'"
            placeholder="Wpisz lub wklej tekst, który chcesz przetłumaczyć..."
            :disabled="isTranslating"
          />
          <p v-if="isOverLimit" class="mt-1 text-sm text-red-600 dark:text-red-400">
            Tekst przekracza limit znaków. Zmniejsz długość tekstu.
          </p>
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Język docelowy
          </label>
          <select
            v-model="targetLanguage"
            class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            :disabled="isTranslating"
          >
            <option value="Polish">Polski</option>
            <option value="English">Angielski</option>
            <option value="German">Niemiecki</option>
            <option value="French">Francuski</option>
            <option value="Spanish">Hiszpański</option>
            <option value="Italian">Włoski</option>
            <option value="Portuguese">Portugalski</option>
            <option value="Russian">Rosyjski</option>
            <option value="Chinese">Chiński</option>
            <option value="Japanese">Japoński</option>
          </select>
        </div>

        <button
          :disabled="!textToTranslate.trim() || isOverLimit || isTranslating"
          class="w-full px-6 py-3 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:bg-gray-400 disabled:cursor-not-allowed transition font-medium flex items-center justify-center gap-2"
          @click="handleTranslate"
        >
          <svg v-if="isTranslating" class="animate-spin h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
          </svg>
          {{ isTranslating ? 'Tłumaczenie...' : 'Przetłumacz' }}
        </button>

        <!-- Error Message -->
        <div v-if="error" class="p-4 bg-red-100 dark:bg-red-900 text-red-800 dark:text-red-200 rounded-lg">
          <div class="flex items-center gap-2">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            {{ error }}
          </div>
        </div>

        <!-- Translation Result -->
        <div v-if="translationResult || isTranslating" class="mt-6">
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Wynik tłumaczenia:
          </label>
          <div class="p-4 bg-gray-50 dark:bg-gray-700 rounded-lg border border-gray-200 dark:border-gray-600 min-h-[200px]">
            <p class="whitespace-pre-wrap text-gray-900 dark:text-white">{{ translationResult }}</p>
            <div v-if="isTranslating && !translationResult" class="flex items-center gap-2 text-blue-600 dark:text-blue-400">
              <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
              </svg>
              <span class="text-sm">Odbieranie odpowiedzi z AI...</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Chat Tab -->
      <div v-else-if="activeTab === 'chat'" class="flex flex-col h-[700px]">
        <!-- Chat Header -->
        <div class="p-4 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between">
          <div>
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white">Rozmowa z AI</h3>
            <p class="text-sm text-gray-600 dark:text-gray-400">
              {{ chatHistory.length }} {{ chatHistory.length === 1 ? 'wiadomość' : 'wiadomości' }}
            </p>
          </div>
          <button
            v-if="chatHistory.length > 0"
            class="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition"
            @click="clearChat"
          >
            Wyczyść historię
          </button>
        </div>

        <!-- Chat Messages -->
        <div
          ref="chatMessagesContainer"
          class="flex-1 overflow-y-auto p-6 space-y-4 bg-gray-50 dark:bg-gray-900"
        >
          <!-- Empty state -->
          <div v-if="chatHistory.length === 0" class="flex flex-col items-center justify-center h-full text-gray-500 dark:text-gray-400">
            <svg class="w-16 h-16 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
            </svg>
            <p class="text-lg font-medium">Rozpocznij rozmowę z AI</p>
            <p class="text-sm mt-2">Zadaj pytanie lub poproś o pomoc</p>
          </div>

          <!-- Messages -->
          <div
            v-for="(msg, index) in chatHistory"
            :key="index"
            :class="[
              'flex',
              msg.role === 'user' ? 'justify-end' : 'justify-start'
            ]"
          >
            <div
              :class="[
                'max-w-[75%] rounded-lg p-4',
                msg.role === 'user'
                  ? 'bg-blue-500 text-white'
                  : 'bg-white dark:bg-gray-800 text-gray-900 dark:text-white border border-gray-200 dark:border-gray-700'
              ]"
            >
              <p class="text-xs font-semibold mb-1 opacity-75">
                {{ msg.role === 'user' ? 'Ty' : 'AI Asystent' }}
              </p>
              <p class="whitespace-pre-wrap leading-relaxed">{{ msg.content }}</p>
            </div>
          </div>

          <!-- Streaming AI Response -->
          <div v-if="isChatting" class="flex justify-start">
            <div class="max-w-[75%] rounded-lg p-4 bg-white dark:bg-gray-800 text-gray-900 dark:text-white border border-gray-200 dark:border-gray-700">
              <p class="text-xs font-semibold mb-1 opacity-75">AI Asystent</p>
              <p class="whitespace-pre-wrap leading-relaxed">{{ chatResult }}</p>
              <div class="flex items-center gap-2 mt-2 text-blue-600 dark:text-blue-400">
                <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                </svg>
                <span class="text-xs">Odpowiadam...</span>
              </div>
            </div>
          </div>

          <!-- Error Message in Chat -->
          <div v-if="error" class="flex justify-center">
            <div class="max-w-[75%] p-4 bg-red-100 dark:bg-red-900 text-red-800 dark:text-red-200 rounded-lg">
              <div class="flex items-center gap-2">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                {{ error }}
              </div>
            </div>
          </div>
        </div>

        <!-- Chat Input -->
        <div class="p-4 border-t border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
          <div class="flex gap-3">
            <input
              v-model="chatMessage"
              type="text"
              class="flex-1 px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="Wpisz wiadomość..."
              :disabled="isChatting"
              @keyup.enter="handleSendMessage"
            >
            <button
              :disabled="!chatMessage.trim() || isChatting"
              class="px-6 py-3 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:bg-gray-400 disabled:cursor-not-allowed transition font-medium flex items-center gap-2"
              @click="handleSendMessage"
            >
              <svg v-if="isChatting" class="animate-spin h-5 w-5" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
              </svg>
              <svg v-else class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" />
              </svg>
              {{ isChatting ? 'Wysyłanie...' : 'Wyślij' }}
            </button>
          </div>
          <p class="mt-2 text-xs text-gray-500 dark:text-gray-400">
            Naciśnij Enter aby wysłać wiadomość
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

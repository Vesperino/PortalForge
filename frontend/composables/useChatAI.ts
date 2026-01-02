export function useChatAI() {
  const config = useRuntimeConfig()
  const authStore = useAuthStore()
  const isTranslating = ref(false)
  const isChatting = ref(false)
  const translationResult = ref('')
  const chatResult = ref('')
  const error = ref<string | null>(null)

  /**
   * Translates text using OpenAI API
   */
  async function translateText(text: string, targetLanguage: string) {
    isTranslating.value = true
    translationResult.value = ''
    error.value = null

    try {
      const response = await fetch(`${config.public.apiUrl}/api/chatai/translate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${authStore.accessToken}`
        },
        body: JSON.stringify({
          textToTranslate: text,
          targetLanguage
        })
      })

      if (!response.ok) {
        const errorData = await response.json().catch(() => null)
        throw new Error(errorData?.error || `HTTP error! status: ${response.status}`)
      }

      const data = await response.json()
      translationResult.value = data.translatedText
    }
    catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'Translation failed'
      console.error('Translation error:', err)
    }
    finally {
      isTranslating.value = false
    }
  }

  /**
   * Sends chat message to OpenAI API
   */
  async function sendChatMessage(message: string, conversationHistory?: Array<{ role: string; content: string }>) {
    isChatting.value = true
    chatResult.value = ''
    error.value = null

    try {
      const response = await fetch(`${config.public.apiUrl}/api/chatai/chat`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${authStore.accessToken}`
        },
        body: JSON.stringify({
          message,
          conversationHistory: conversationHistory || []
        })
      })

      if (!response.ok) {
        const errorData = await response.json().catch(() => null)
        throw new Error(errorData?.error || `HTTP error! status: ${response.status}`)
      }

      const data = await response.json()
      chatResult.value = data.message
    }
    catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'Chat failed'
      console.error('Chat error:', err)
    }
    finally {
      isChatting.value = false
    }
  }

  /**
   * Tests OpenAI connection
   */
  async function testConnection(apiKey?: string) {
    try {
      const url = apiKey
        ? `${config.public.apiUrl}/api/chatai/test-connection?testApiKey=${encodeURIComponent(apiKey)}`
        : `${config.public.apiUrl}/api/chatai/test-connection`

      const response = await $fetch<boolean>(url, {
        headers: {
          Authorization: `Bearer ${authStore.accessToken}`
        }
      })

      return response
    }
    catch (err: unknown) {
      console.error('Connection test failed:', err)
      return false
    }
  }

  return {
    isTranslating: readonly(isTranslating),
    isChatting: readonly(isChatting),
    translationResult: readonly(translationResult),
    chatResult: readonly(chatResult),
    error: readonly(error),
    translateText,
    sendChatMessage,
    testConnection
  }
}

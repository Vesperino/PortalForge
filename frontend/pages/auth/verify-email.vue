<script setup lang="ts">
definePageMeta({
  layout: 'auth',
  middleware: 'guest'
})

const route = useRoute()
const config = useRuntimeConfig()

const email = ref((route.query.email as string) || '')
const isLoading = ref(false)
const message = ref('')
const messageType = ref<'success' | 'error' | ''>('')

// Rate limiting timer
const cooldownSeconds = ref(0)
const canResend = computed(() => cooldownSeconds.value === 0 && !isLoading.value && email.value !== '')

// Start cooldown timer
const startCooldown = () => {
  cooldownSeconds.value = 120 // 2 minutes
  const interval = setInterval(() => {
    cooldownSeconds.value--
    if (cooldownSeconds.value <= 0) {
      clearInterval(interval)
    }
  }, 1000)
}

// Format cooldown time
const cooldownTime = computed(() => {
  const minutes = Math.floor(cooldownSeconds.value / 60)
  const seconds = cooldownSeconds.value % 60
  return `${minutes}:${seconds.toString().padStart(2, '0')}`
})

// Resend verification email
const resendEmail = async () => {
  if (!email.value) {
    message.value = 'Podaj adres email'
    messageType.value = 'error'
    return
  }

  isLoading.value = true
  message.value = ''
  messageType.value = ''

  try {
    const { data, error } = await useFetch('/api/auth/resend-verification', {
      method: 'POST',
      baseURL: config.public.apiUrl,
      body: {
        email: email.value
      }
    })

    if (error.value) {
      message.value = error.value.data?.message || 'Nie udało się wysłać emaila'
      messageType.value = 'error'
    } else {
      message.value = 'Email weryfikacyjny został wysłany. Sprawdź swoją skrzynkę pocztową.'
      messageType.value = 'success'
      startCooldown()
    }
  } catch (err) {
    message.value = 'Wystąpił błąd podczas wysyłania emaila'
    messageType.value = 'error'
  } finally {
    isLoading.value = false
  }
}

// Check if there's an email from registration
onMounted(() => {
  if (email.value) {
    // If redirected from registration, show message
    message.value = 'Sprawdź swoją skrzynkę pocztową. Email weryfikacyjny został wysłany.'
    messageType.value = 'success'
    startCooldown()
  }
})
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <div class="text-center">
        <h2 class="text-3xl font-bold text-gray-900 dark:text-white">
          Weryfikacja emaila
        </h2>
        <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">
          Aby kontynuować, zweryfikuj swój adres email
        </p>
      </div>

      <div class="bg-white dark:bg-gray-800 shadow-md rounded-lg p-6">
        <!-- Icon -->
        <div class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-blue-100 dark:bg-blue-900">
          <svg class="h-6 w-6 text-blue-600 dark:text-blue-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"></path>
          </svg>
        </div>

        <!-- Instructions -->
        <div class="mt-4 text-center">
          <p class="text-sm text-gray-600 dark:text-gray-300">
            Wysłaliśmy link weryfikacyjny na Twój adres email. Kliknij w link, aby aktywować konto.
          </p>
        </div>

        <!-- Success/Error Message -->
        <div v-if="message" class="mt-4">
          <div
            v-if="messageType === 'success'"
            class="bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-md p-4"
          >
            <p class="text-sm text-green-800 dark:text-green-300">{{ message }}</p>
          </div>
          <div
            v-else-if="messageType === 'error'"
            class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md p-4"
          >
            <p class="text-sm text-red-800 dark:text-red-300">{{ message }}</p>
          </div>
        </div>

        <!-- Email Input -->
        <div class="mt-6">
          <label for="email" class="block text-sm font-medium text-gray-700 dark:text-gray-300">
            Adres email
          </label>
          <input
            id="email"
            v-model="email"
            type="email"
            required
            class="mt-1 block w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
            placeholder="twoj@email.pl"
          />
        </div>

        <!-- Resend Button -->
        <div class="mt-6">
          <button
            type="button"
            :disabled="!canResend"
            @click="resendEmail"
            class="w-full flex justify-center items-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <span v-if="isLoading">
              <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              Wysyłanie...
            </span>
            <span v-else-if="cooldownSeconds > 0">
              Wyślij ponownie ({{ cooldownTime }})
            </span>
            <span v-else>
              Wyślij ponownie email weryfikacyjny
            </span>
          </button>
        </div>

        <!-- Back to Login -->
        <div class="mt-6 text-center">
          <NuxtLink
            to="/auth/login"
            class="text-sm text-blue-600 hover:text-blue-500 dark:text-blue-400"
          >
            Wróć do logowania
          </NuxtLink>
        </div>
      </div>
    </div>
  </div>
</template>

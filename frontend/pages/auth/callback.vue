<script setup lang="ts">
definePageMeta({
  layout: 'auth',
  middleware: 'guest'
})

const router = useRouter()
const config = useRuntimeConfig()

const status = ref<'loading' | 'success' | 'error'>('loading')
const errorMessage = ref('')

onMounted(async () => {
  try {
    // Supabase redirects with tokens in the URL hash
    // Format: #access_token=XXX&refresh_token=YYY&expires_in=3600&token_type=bearer&type=signup
    const hashParams = new URLSearchParams(window.location.hash.substring(1))

    const accessToken = hashParams.get('access_token')
    const refreshToken = hashParams.get('refresh_token')
    const type = hashParams.get('type')
    const error = hashParams.get('error')
    const errorDescription = hashParams.get('error_description')

    // Check for errors first
    if (error) {
      status.value = 'error'
      errorMessage.value = errorDescription || 'Weryfikacja nie powiodła się'
      return
    }

    // Verify we have tokens for signup
    if (!accessToken || !refreshToken || type !== 'signup') {
      status.value = 'error'
      errorMessage.value = 'Nieprawidłowy link weryfikacyjny'
      return
    }

    // Call backend to verify email and update database
    const { error: verifyError } = await useFetch('/api/Auth/verify-email', {
      method: 'POST',
      baseURL: config.public.apiUrl,
      body: {
        token: accessToken,
        email: '' // Email not needed, we get it from token
      }
    })

    if (verifyError.value) {
      status.value = 'error'
      errorMessage.value = verifyError.value.data?.message || 'Weryfikacja nie powiodła się'
      return
    }

    status.value = 'success'

    // Redirect to home page after 3 seconds
    setTimeout(() => {
      router.push('/')
    }, 3000)
  } catch (err) {
    console.error('Email verification error:', err)
    status.value = 'error'
    errorMessage.value = 'Wystąpił błąd podczas weryfikacji emaila'
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
      </div>

      <!-- Loading State -->
      <div v-if="status === 'loading'" class="text-center py-8">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600" />
        <p class="mt-4 text-gray-600 dark:text-gray-300">
          Weryfikowanie emaila...
        </p>
      </div>

      <!-- Success State -->
      <div v-else-if="status === 'success'" class="text-center py-8">
        <div class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-green-100 dark:bg-green-900">
          <svg class="h-6 w-6 text-green-600 dark:text-green-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
          </svg>
        </div>
        <h3 class="mt-4 text-lg font-medium text-gray-900 dark:text-white">
          Konto zostało aktywowane!
        </h3>
        <p class="mt-2 text-gray-600 dark:text-gray-300">
          Twój email został pomyślnie zweryfikowany.
        </p>
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
          Za chwilę zostaniesz przekierowany do strony głównej...
        </p>
      </div>

      <!-- Error State -->
      <div v-else-if="status === 'error'" class="text-center py-8">
        <div class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-red-100 dark:bg-red-900">
          <svg class="h-6 w-6 text-red-600 dark:text-red-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </div>
        <h3 class="mt-4 text-lg font-medium text-gray-900 dark:text-white">
          Weryfikacja nie powiodła się
        </h3>
        <p class="mt-2 text-gray-600 dark:text-gray-300">
          {{ errorMessage }}
        </p>
        <div class="mt-6">
          <NuxtLink
            to="/auth/verify-email"
            class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
          >
            Wyślij ponownie email weryfikacyjny
          </NuxtLink>
        </div>
      </div>
    </div>
  </div>
</template>

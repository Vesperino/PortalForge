<script setup lang="ts">
definePageMeta({
  layout: 'auth',
  middleware: 'guest'
})

const route = useRoute()
const router = useRouter()
const config = useRuntimeConfig()

const status = ref<'loading' | 'success' | 'error'>('loading')
const errorMessage = ref('')

onMounted(async () => {
  try {
    // Get token from URL
    const token = route.query.token as string
    const type = route.query.type as string

    if (!token || type !== 'signup') {
      status.value = 'error'
      errorMessage.value = 'Nieprawidłowy link weryfikacyjny'
      return
    }

    // Call backend to verify email
    const { error } = await useFetch('/api/auth/verify-email', {
      method: 'POST',
      baseURL: config.public.apiUrl,
      body: {
        token,
        email: route.query.email || ''
      }
    })

    if (error.value) {
      status.value = 'error'
      errorMessage.value = error.value.data?.message || 'Weryfikacja nie powiodła się'
      return
    }

    status.value = 'success'

    // Redirect to login after 3 seconds
    setTimeout(() => {
      router.push('/auth/login')
    }, 3000)
  } catch {
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
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"/>
        <p class="mt-4 text-gray-600 dark:text-gray-300">Weryfikowanie emaila...</p>
      </div>

      <!-- Success State -->
      <div v-else-if="status === 'success'" class="text-center py-8">
        <div class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-green-100">
          <svg class="h-6 w-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
          </svg>
        </div>
        <h3 class="mt-4 text-lg font-medium text-gray-900 dark:text-white">
          Email zweryfikowany pomyślnie!
        </h3>
        <p class="mt-2 text-gray-600 dark:text-gray-300">
          Za chwilę zostaniesz przekierowany do logowania...
        </p>
      </div>

      <!-- Error State -->
      <div v-else-if="status === 'error'" class="text-center py-8">
        <div class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-red-100">
          <svg class="h-6 w-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
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

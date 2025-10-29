<script setup lang="ts">
definePageMeta({
  layout: 'auth',
  middleware: 'guest'
})

useHead({
  title: 'Logowanie - PortalForge',
  meta: [
    { name: 'description', content: 'Zaloguj się do portalu wewnętrznego organizacji' }
  ]
})

const { login } = useAuth()
const router = useRouter()

// Zawsze loguj jako admin - dane testowe
const email = ref('admin@portalforge.com')
const password = ref('Admin123!')
const isLoading = ref(false)
const error = ref<string | null>(null)

async function handleLogin() {
  isLoading.value = true
  error.value = null

  try {
    await login(email.value, password.value)

    // Przekieruj do dashboard
    await router.push('/dashboard')
  } catch (err: any) {
    error.value = err.message || 'Wystąpił błąd podczas logowania'
    console.error('Login error:', err)
  } finally {
    isLoading.value = false
  }
}

// Auto-login dla testów (opcjonalne - możesz usunąć)
onMounted(() => {
  // Możesz odkomentować poniższą linię aby automatycznie zalogować przy wejściu
  // handleLogin()
})
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-100 dark:bg-gray-900 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <!-- Header -->
      <div>
        <h2 class="mt-6 text-center text-3xl font-extrabold text-gray-900 dark:text-white">
          Zaloguj się do PortalForge
        </h2>
        <p class="mt-2 text-center text-sm text-gray-600 dark:text-gray-400">
          Logowanie testowe jako Administrator
        </p>
      </div>

      <!-- Login Form -->
      <form class="mt-8 space-y-6" @submit.prevent="handleLogin">
        <!-- Error Message -->
        <div v-if="error" class="rounded-md bg-red-50 dark:bg-red-900/20 p-4">
          <div class="flex">
            <div class="flex-shrink-0">
              <svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
              </svg>
            </div>
            <div class="ml-3">
              <p class="text-sm text-red-800 dark:text-red-200">
                {{ error }}
              </p>
            </div>
          </div>
        </div>

        <div class="rounded-md shadow-sm space-y-4">
          <!-- Email -->
          <div>
            <label for="email" class="sr-only">Email</label>
            <input
              id="email"
              v-model="email"
              type="email"
              autocomplete="email"
              required
              class="appearance-none rounded-lg relative block w-full px-3 py-2 border border-gray-300 dark:border-gray-600 placeholder-gray-500 dark:placeholder-gray-400 text-gray-900 dark:text-white bg-white dark:bg-gray-800 focus:outline-none focus:ring-blue-500 focus:border-blue-500 focus:z-10 sm:text-sm"
              placeholder="Email"
              readonly
            >
          </div>

          <!-- Password -->
          <div>
            <label for="password" class="sr-only">Hasło</label>
            <input
              id="password"
              v-model="password"
              type="password"
              autocomplete="current-password"
              required
              class="appearance-none rounded-lg relative block w-full px-3 py-2 border border-gray-300 dark:border-gray-600 placeholder-gray-500 dark:placeholder-gray-400 text-gray-900 dark:text-white bg-white dark:bg-gray-800 focus:outline-none focus:ring-blue-500 focus:border-blue-500 focus:z-10 sm:text-sm"
              placeholder="Hasło"
              readonly
            >
          </div>
        </div>

        <!-- Info Box -->
        <div class="rounded-md bg-blue-50 dark:bg-blue-900/20 p-4">
          <div class="flex">
            <div class="flex-shrink-0">
              <svg class="h-5 w-5 text-blue-400" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd" />
              </svg>
            </div>
            <div class="ml-3">
              <p class="text-sm text-blue-800 dark:text-blue-200">
                <strong>Konto testowe:</strong> admin@portalforge.com<br>
                <strong>Rola:</strong> Administrator (Admin)<br>
                <strong>Uprawnienia:</strong> Pełny dostęp do wszystkich funkcji
              </p>
            </div>
          </div>
        </div>

        <!-- Submit Button -->
        <div>
          <button
            type="submit"
            :disabled="isLoading"
            class="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
          >
            <span class="absolute left-0 inset-y-0 flex items-center pl-3">
              <svg v-if="!isLoading" class="h-5 w-5 text-blue-500 group-hover:text-blue-400" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clip-rule="evenodd" />
              </svg>
              <div v-else class="animate-spin h-5 w-5 border-2 border-white border-t-transparent rounded-full" />
            </span>
            {{ isLoading ? 'Logowanie...' : 'Zaloguj się jako Admin' }}
          </button>
        </div>

        <!-- Additional Links -->
        <div class="flex items-center justify-center text-sm">
          <NuxtLink
            to="/auth/register"
            class="font-medium text-blue-600 hover:text-blue-500 dark:text-blue-400 dark:hover:text-blue-300"
          >
            Nie masz konta? Zarejestruj się
          </NuxtLink>
        </div>
      </form>
    </div>
  </div>
</template>

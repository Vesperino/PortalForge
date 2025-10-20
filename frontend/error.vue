<script setup lang="ts">
import type { NuxtError } from '#app'

const props = defineProps<{
  error: NuxtError
}>()

const authStore = useAuthStore()
const router = useRouter()

const is404 = computed(() => props.error.statusCode === 404)

onMounted(() => {
  if (is404.value) {
    if (authStore.isAuthenticated) {
      router.push('/')
    } else {
      router.push('/auth/login')
    }
  }
})
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full text-center">
      <div v-if="is404">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mb-4" />
        <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">
          Przekierowywanie...
        </h2>
        <p class="text-gray-600 dark:text-gray-400">
          Strona nie istnieje. Za chwilę zostaniesz przekierowany.
        </p>
      </div>

      <div v-else class="space-y-4">
        <div class="mx-auto flex items-center justify-center h-16 w-16 rounded-full bg-red-100 dark:bg-red-900">
          <svg class="h-8 w-8 text-red-600 dark:text-red-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
        </div>

        <h1 class="text-4xl font-bold text-gray-900 dark:text-white">
          {{ error.statusCode }}
        </h1>

        <h2 class="text-2xl font-semibold text-gray-700 dark:text-gray-300">
          Wystąpił błąd
        </h2>

        <p class="text-gray-600 dark:text-gray-400">
          {{ error.message || 'Coś poszło nie tak' }}
        </p>

        <div class="flex gap-4 justify-center mt-6">
          <NuxtLink
            to="/"
            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-md transition"
          >
            Strona główna
          </NuxtLink>
          <button
            class="px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-900 dark:text-white rounded-md transition"
            @click="router.back()"
          >
            Wróć
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

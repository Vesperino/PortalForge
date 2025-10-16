<script setup lang="ts">
const router = useRouter()

const email = ref('')
const isLoading = ref(false)
const error = ref<string | null>(null)
const success = ref(false)
const formErrors = ref<{ email?: string }>({})

const validateForm = (): boolean => {
  formErrors.value = {}

  if (!email.value) {
    formErrors.value.email = 'Email jest wymagany'
    return false
  }

  if (!email.value.includes('@') || !email.value.includes('.')) {
    formErrors.value.email = 'Nieprawidłowy format email'
    return false
  }

  return true
}

const handleSubmit = async () => {
  if (!validateForm()) {
    return
  }

  isLoading.value = true
  error.value = null

  try {
    const { error: fetchError } = await useFetch('/api/auth/reset-password', {
      method: 'POST',
      body: { email: email.value }
    })

    if (fetchError.value) {
      error.value = fetchError.value.data?.message || 'Wystąpił błąd podczas resetowania hasła'
    } else {
      success.value = true
    }
  } catch {
    error.value = 'Wystąpił błąd podczas resetowania hasła'
  } finally {
    isLoading.value = false
  }
}

const goBack = () => {
  router.push('/auth/login')
}
</script>

<template>
  <div class="space-y-6">
    <div>
      <h2 class="text-2xl font-bold text-gray-900 text-center mb-2">
        Resetowanie hasła
      </h2>
      <p class="text-sm text-gray-600 text-center">
        Wprowadź swój adres email, a wyślemy Ci link do zresetowania hasła.
      </p>
    </div>

    <BaseAlert v-if="error" variant="error" dismissible @dismiss="error = null">
      {{ error }}
    </BaseAlert>

    <BaseAlert v-if="success" variant="success">
      <p class="font-medium">Link został wysłany!</p>
      <p class="text-sm mt-1">
        Sprawdź swoją skrzynkę email i kliknij w link, aby zresetować hasło.
      </p>
    </BaseAlert>

    <form v-if="!success" class="space-y-6" @submit.prevent="handleSubmit">
      <BaseInput
        v-model="email"
        type="email"
        label="Email"
        placeholder="twoj.email@firma.pl"
        autocomplete="email"
        :error="formErrors.email"
        :disabled="isLoading"
        required
      />

      <div class="flex gap-3">
        <BaseButton
          type="button"
          variant="secondary"
          size="lg"
          full-width
          :disabled="isLoading"
          @click="goBack"
        >
          Anuluj
        </BaseButton>

        <BaseButton
          type="submit"
          variant="primary"
          size="lg"
          full-width
          :loading="isLoading"
          :disabled="isLoading"
        >
          {{ isLoading ? 'Wysyłanie...' : 'Wyślij link' }}
        </BaseButton>
      </div>
    </form>

    <div v-else class="text-center">
      <BaseButton
        variant="primary"
        size="lg"
        full-width
        @click="goBack"
      >
        Powrót do logowania
      </BaseButton>
    </div>

    <div class="text-center text-sm text-gray-600">
      Pamiętasz hasło?
      <NuxtLink
        to="/auth/login"
        class="font-medium text-blue-600 hover:text-blue-500 focus:outline-none focus:underline"
      >
        Zaloguj się
      </NuxtLink>
    </div>
  </div>
</template>

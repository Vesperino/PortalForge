<script setup lang="ts">
const { register, isLoading, error } = useAuth()

const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const formErrors = ref<{ email?: string; password?: string; confirmPassword?: string }>({})
const showPassword = ref(false)
const showConfirmPassword = ref(false)

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

  if (!password.value) {
    formErrors.value.password = 'Hasło jest wymagane'
    return false
  }

  if (password.value.length < 8) {
    formErrors.value.password = 'Hasło musi mieć minimum 8 znaków'
    return false
  }

  if (!confirmPassword.value) {
    formErrors.value.confirmPassword = 'Potwierdzenie hasła jest wymagane'
    return false
  }

  if (password.value !== confirmPassword.value) {
    formErrors.value.confirmPassword = 'Hasła nie są identyczne'
    return false
  }

  return true
}

const handleSubmit = async () => {
  if (!validateForm()) {
    return
  }

  await register({
    email: email.value,
    password: password.value,
    confirmPassword: confirmPassword.value
  })
}
</script>

<template>
  <form class="space-y-6" @submit.prevent="handleSubmit">
    <div>
      <h2 class="text-2xl font-bold text-gray-900 text-center mb-2">
        Utwórz konto
      </h2>
      <p class="text-sm text-gray-600 text-center">
        Zarejestruj się, aby uzyskać dostęp do portalu.
      </p>
    </div>

    <BaseAlert v-if="error" variant="error" dismissible>
      {{ error }}
    </BaseAlert>

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

    <div>
      <div class="relative">
        <BaseInput
          v-model="password"
          :type="showPassword ? 'text' : 'password'"
          label="Hasło"
          placeholder="••••••••"
          autocomplete="new-password"
          :error="formErrors.password"
          :disabled="isLoading"
          required
        />
        <button
          type="button"
          class="absolute right-3 top-9 text-gray-400 hover:text-gray-600 focus:outline-none"
          :aria-label="showPassword ? 'Ukryj hasło' : 'Pokaż hasło'"
          @click="showPassword = !showPassword"
        >
          <svg
            v-if="!showPassword"
            class="w-5 h-5"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
            />
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
            />
          </svg>
          <svg
            v-else
            class="w-5 h-5"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21"
            />
          </svg>
        </button>
      </div>
    </div>

    <div>
      <div class="relative">
        <BaseInput
          v-model="confirmPassword"
          :type="showConfirmPassword ? 'text' : 'password'"
          label="Potwierdź hasło"
          placeholder="••••••••"
          autocomplete="new-password"
          :error="formErrors.confirmPassword"
          :disabled="isLoading"
          required
        />
        <button
          type="button"
          class="absolute right-3 top-9 text-gray-400 hover:text-gray-600 focus:outline-none"
          :aria-label="showConfirmPassword ? 'Ukryj hasło' : 'Pokaż hasło'"
          @click="showConfirmPassword = !showConfirmPassword"
        >
          <svg
            v-if="!showConfirmPassword"
            class="w-5 h-5"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
            />
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
            />
          </svg>
          <svg
            v-else
            class="w-5 h-5"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21"
            />
          </svg>
        </button>
      </div>
    </div>

    <div class="text-xs text-gray-500">
      Hasło musi zawierać minimum 8 znaków.
    </div>

    <BaseButton
      type="submit"
      variant="primary"
      size="lg"
      full-width
      :loading="isLoading"
      :disabled="isLoading"
    >
      {{ isLoading ? 'Rejestracja...' : 'Zarejestruj się' }}
    </BaseButton>

    <div class="text-center text-sm text-gray-600">
      Masz już konto?
      <NuxtLink
        to="/auth/login"
        class="font-medium text-blue-600 hover:text-blue-500 focus:outline-none focus:underline"
      >
        Zaloguj się
      </NuxtLink>
    </div>
  </form>
</template>

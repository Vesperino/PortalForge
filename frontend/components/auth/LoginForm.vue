<script setup lang="ts">
const { login, isLoading, error } = useAuth()

const email = ref('')
const password = ref('')
const formErrors = ref<{ email?: string; password?: string }>({})
const showPassword = ref(false)

const validateForm = (): boolean => {
  formErrors.value = {}

  if (!email.value) {
    formErrors.value.email = 'Email jest wymagany'
    return false
  }

  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  if (!emailRegex.test(email.value)) {
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

  return true
}

const handleSubmit = async () => {
  if (!validateForm()) {
    return
  }

  await login({
    email: email.value,
    password: password.value
  })
}

const togglePasswordVisibility = () => {
  showPassword.value = !showPassword.value
}
</script>

<template>
  <form @submit.prevent="handleSubmit" class="space-y-6">
    <div>
      <h2 class="text-2xl font-bold text-gray-900 text-center mb-2">
        Zaloguj się
      </h2>
      <p class="text-sm text-gray-600 text-center">
        Witaj ponownie! Wprowadź swoje dane logowania.
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
          autocomplete="current-password"
          :error="formErrors.password"
          :disabled="isLoading"
          required
        />
        <button
          type="button"
          class="absolute right-3 top-9 text-gray-400 hover:text-gray-600 focus:outline-none"
          :aria-label="showPassword ? 'Ukryj hasło' : 'Pokaż hasło'"
          @click="togglePasswordVisibility"
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

    <div class="flex items-center justify-between">
      <div class="flex items-center">
        <input
          id="remember-me"
          name="remember-me"
          type="checkbox"
          class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
        >
        <label for="remember-me" class="ml-2 block text-sm text-gray-900">
          Zapamiętaj mnie
        </label>
      </div>

      <div class="text-sm">
        <NuxtLink
          to="/auth/reset-password"
          class="font-medium text-blue-600 hover:text-blue-500 focus:outline-none focus:underline"
        >
          Zapomniałeś hasła?
        </NuxtLink>
      </div>
    </div>

    <BaseButton
      type="submit"
      variant="primary"
      size="lg"
      full-width
      :loading="isLoading"
      :disabled="isLoading"
    >
      {{ isLoading ? 'Logowanie...' : 'Zaloguj się' }}
    </BaseButton>

    <div class="text-center text-sm text-gray-600">
      Nie masz konta?
      <NuxtLink
        to="/auth/register"
        class="font-medium text-blue-600 hover:text-blue-500 focus:outline-none focus:underline"
      >
        Zarejestruj się
      </NuxtLink>
    </div>
  </form>
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'default',
  middleware: ['auth']
})

const user = ref({
  firstName: 'Jan',
  lastName: 'Kowalski',
  email: 'jan.kowalski@company.com',
  department: 'IT',
  position: 'Developer',
  phone: '+48 123 456 789',
  avatar: ''
})

const isEditing = ref(false)

const toggleEdit = () => {
  isEditing.value = !isEditing.value
}

const saveChanges = () => {
  // TODO: Save changes to backend
  isEditing.value = false
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
        Moje konto
      </h1>
      <BaseButton
        v-if="!isEditing"
        variant="primary"
        @click="toggleEdit"
      >
        Edytuj profil
      </BaseButton>
    </div>

    <!-- Profile Card -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden">
      <!-- Cover -->
      <div class="h-32 bg-gradient-to-r from-blue-500 to-blue-600" />

      <!-- Profile Content -->
      <div class="px-6 pb-6">
        <!-- Avatar -->
        <div class="flex items-end -mt-16 mb-4">
          <div class="w-32 h-32 rounded-full bg-gray-300 dark:bg-gray-700 border-4 border-white dark:border-gray-800 flex items-center justify-center text-4xl font-bold text-white">
            {{ user.firstName[0] }}{{ user.lastName[0] }}
          </div>
          <div class="ml-4 mb-2">
            <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
              {{ user.firstName }} {{ user.lastName }}
            </h2>
            <p class="text-gray-600 dark:text-gray-400">
              {{ user.position }}
            </p>
          </div>
        </div>

        <!-- Profile Information -->
        <div class="space-y-6 mt-6">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <!-- Email -->
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Email
              </label>
              <BaseInput
                v-model="user.email"
                type="email"
                :disabled="!isEditing"
                :class="{ 'bg-gray-50 dark:bg-gray-900': !isEditing }"
              />
            </div>

            <!-- Phone -->
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Telefon
              </label>
              <BaseInput
                v-model="user.phone"
                type="tel"
                :disabled="!isEditing"
                :class="{ 'bg-gray-50 dark:bg-gray-900': !isEditing }"
              />
            </div>

            <!-- Department -->
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Dział
              </label>
              <BaseInput
                v-model="user.department"
                :disabled="true"
                class="bg-gray-50 dark:bg-gray-900"
              />
            </div>

            <!-- Position -->
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Stanowisko
              </label>
              <BaseInput
                v-model="user.position"
                :disabled="true"
                class="bg-gray-50 dark:bg-gray-900"
              />
            </div>
          </div>

          <!-- Action Buttons (when editing) -->
          <div v-if="isEditing" class="flex gap-3 pt-4 border-t border-gray-200 dark:border-gray-700">
            <BaseButton variant="primary" @click="saveChanges">
              Zapisz zmiany
            </BaseButton>
            <BaseButton variant="secondary" @click="toggleEdit">
              Anuluj
            </BaseButton>
          </div>
        </div>
      </div>
    </div>

    <!-- Additional Sections -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <!-- Change Password -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Zmień hasło
        </h3>
        <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">
          Aktualizuj swoje hasło, aby zachować bezpieczeństwo konta.
        </p>
        <BaseButton variant="secondary" size="sm">
          Zmień hasło
        </BaseButton>
      </div>

      <!-- Account Settings -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Ustawienia konta
        </h3>
        <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">
          Zarządzaj ustawieniami powiadomień i preferencjami.
        </p>
        <BaseButton variant="secondary" size="sm">
          Ustawienia
        </BaseButton>
      </div>
    </div>
  </div>
</template>

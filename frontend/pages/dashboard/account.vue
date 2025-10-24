<script setup lang="ts">
definePageMeta({
  layout: 'default',
  middleware: ['auth']
})

const authStore = useAuthStore()
const { getEmployees, getNews, getDocuments } = useMockData()

// Get a sample employee as the current user (in real app, this would come from auth store)
const employees = getEmployees()
const currentUser = employees.length > 0 ? employees[0] : null

if (!currentUser) {
  throw new Error('No employees found')
}

const user = ref({
  firstName: currentUser.firstName,
  lastName: currentUser.lastName,
  email: currentUser.email,
  department: currentUser.department?.name || '',
  position: currentUser.position?.name || '',
  phone: currentUser.phone || '',
  avatar: ''
})

const isEditing = ref(false)

// Get user's activity
const userNews = getNews().filter(news => news.authorId === currentUser.id)
const userDocuments = getDocuments().filter(doc => doc.uploadedBy === currentUser.id)

// Calculate stats
const stats = computed(() => ({
  newsPublished: userNews.length,
  documentsUploaded: userDocuments.length,
  teamMembers: currentUser?.subordinates?.length || 0,
  lastLogin: new Date(Date.now() - Math.random() * 7 * 24 * 60 * 60 * 1000) // Random date within last week
}))

const formatDate = (date: Date) => {
  return new Intl.DateTimeFormat('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(date)
}

const toggleEdit = () => {
  isEditing.value = !isEditing.value
}

const saveChanges = () => {
  // TODO: Save changes to backend
  isEditing.value = false
  alert('Zmiany zostały zapisane!')
}

const logout = async () => {
  await authStore.logout()
  navigateTo('/auth/login')
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

    <!-- Activity Stats -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
        Twoja aktywność
      </h3>
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div class="text-center p-4 bg-blue-50 dark:bg-blue-900/20 rounded-lg">
          <p class="text-3xl font-bold text-blue-600 dark:text-blue-400">
            {{ stats.newsPublished }}
          </p>
          <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
            Opublikowane aktualności
          </p>
        </div>
        <div class="text-center p-4 bg-green-50 dark:bg-green-900/20 rounded-lg">
          <p class="text-3xl font-bold text-green-600 dark:text-green-400">
            {{ stats.documentsUploaded }}
          </p>
          <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
            Dodane dokumenty
          </p>
        </div>
        <div class="text-center p-4 bg-purple-50 dark:bg-purple-900/20 rounded-lg">
          <p class="text-3xl font-bold text-purple-600 dark:text-purple-400">
            {{ stats.teamMembers }}
          </p>
          <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
            Członków zespołu
          </p>
        </div>
        <div class="text-center p-4 bg-orange-50 dark:bg-orange-900/20 rounded-lg">
          <p class="text-sm font-medium text-gray-900 dark:text-white">
            Ostatnie logowanie
          </p>
          <p class="text-xs text-gray-600 dark:text-gray-400 mt-1">
            {{ formatDate(stats.lastLogin) }}
          </p>
        </div>
      </div>
    </div>

    <!-- Recent Activity -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Recent News -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Ostatnie aktualności
        </h3>
        <div v-if="userNews.length > 0" class="space-y-3">
          <div
            v-for="news in userNews.slice(0, 3)"
            :key="news.id"
            class="p-3 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors cursor-pointer"
          >
            <h4 class="font-medium text-gray-900 dark:text-white text-sm">
              {{ news.title }}
            </h4>
            <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
              {{ formatDate(news.createdAt) }}
            </p>
          </div>
        </div>
        <p v-else class="text-sm text-gray-500 dark:text-gray-400">
          Nie masz jeszcze opublikowanych aktualności.
        </p>
      </div>

      <!-- Recent Documents -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Ostatnie dokumenty
        </h3>
        <div v-if="userDocuments.length > 0" class="space-y-3">
          <div
            v-for="doc in userDocuments.slice(0, 3)"
            :key="doc.id"
            class="p-3 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors cursor-pointer"
          >
            <h4 class="font-medium text-gray-900 dark:text-white text-sm">
              {{ doc.name }}
            </h4>
            <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
              {{ formatDate(doc.uploadedAt) }}
            </p>
          </div>
        </div>
        <p v-else class="text-sm text-gray-500 dark:text-gray-400">
          Nie masz jeszcze dodanych dokumentów.
        </p>
      </div>
    </div>

    <!-- Team Members -->
    <div v-if="currentUser?.subordinates && currentUser.subordinates.length > 0" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
        Twój zespół
      </h3>
      <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
        <div
          v-for="member in currentUser?.subordinates || []"
          :key="member.id"
          class="flex items-center gap-3 p-3 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors cursor-pointer"
        >
          <div class="w-12 h-12 rounded-full bg-blue-500 flex items-center justify-center text-white font-semibold">
            {{ member.firstName[0] }}{{ member.lastName[0] }}
          </div>
          <div class="flex-1 min-w-0">
            <p class="font-medium text-gray-900 dark:text-white text-sm truncate">
              {{ member.firstName }} {{ member.lastName }}
            </p>
            <p class="text-xs text-gray-600 dark:text-gray-400 truncate">
              {{ member.position?.name }}
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Additional Sections -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
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

      <!-- Logout -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Wyloguj się
        </h3>
        <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">
          Zakończ bieżącą sesję i wyloguj się z systemu.
        </p>
        <BaseButton variant="danger" size="sm" @click="logout">
          Wyloguj
        </BaseButton>
      </div>
    </div>
  </div>
</template>

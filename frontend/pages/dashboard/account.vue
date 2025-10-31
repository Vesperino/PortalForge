<script setup lang="ts">
definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

const authStore = useAuthStore()

// Use real user data from auth store
const currentUser = computed(() => authStore.user)

if (!currentUser.value) {
  throw new Error('User not authenticated')
}

const user = ref({
  firstName: currentUser.value.firstName,
  lastName: currentUser.value.lastName,
  email: currentUser.value.email,
  department: currentUser.value.department || '',
  position: currentUser.value.position || '',
  phone: currentUser.value.phoneNumber || '',
  avatar: currentUser.value.profilePhotoUrl || ''
})

const isEditing = ref(false)

// User's activity - TODO: Replace with real API calls when endpoints are available
const userNews = ref<any[]>([])
const userDocuments = ref<any[]>([])

// Calculate stats
const stats = computed(() => ({
  newsPublished: userNews.value.length,
  documentsUploaded: userDocuments.value.length,
  teamMembers: 0, // TODO: Fetch from API when subordinates endpoint is available
  lastLogin: new Date() // TODO: Track real last login
}))

// Vacation and sick leave data (mock data)
const vacationData = ref({
  total: 26,
  used: 8,
  remaining: 18,
  history: [
    {
      id: 1,
      startDate: new Date(2024, 6, 15),
      endDate: new Date(2024, 6, 19),
      days: 5,
      type: 'Urlop wypoczynkowy',
      status: 'Zatwierdzony'
    },
    {
      id: 2,
      startDate: new Date(2024, 4, 10),
      endDate: new Date(2024, 4, 11),
      days: 2,
      type: 'Urlop na żądanie',
      status: 'Zatwierdzony'
    },
    {
      id: 3,
      startDate: new Date(2024, 2, 20),
      endDate: new Date(2024, 2, 20),
      days: 1,
      type: 'Urlop okolicznościowy',
      status: 'Zatwierdzony'
    }
  ]
})

const sickLeaveData = ref({
  total: 3,
  history: [
    {
      id: 1,
      startDate: new Date(2024, 8, 5),
      endDate: new Date(2024, 8, 7),
      days: 3,
      reason: 'Przeziębienie'
    }
  ]
})

const workData = ref({
  startDate: new Date(2022, 0, 15),
  yearsOfService: 2
})

const formatDate = (date: Date) => {
  return new Intl.DateTimeFormat('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(date)
}

const formatDateShort = (date: Date) => {
  return new Intl.DateTimeFormat('pl-PL', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  }).format(date)
}

const formatDateRange = (startDate: Date, endDate: Date) => {
  const start = formatDateShort(startDate)
  const end = formatDateShort(endDate)
  return start === end ? start : `${start} - ${end}`
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

    <!-- Vacation and Sick Leave Section -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-6">
        Urlopy i absencje
      </h3>

      <!-- Summary Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
        <div class="p-4 bg-blue-50 dark:bg-blue-900/20 rounded-lg border border-blue-200 dark:border-blue-800">
          <div class="flex items-center gap-3">
            <svg class="w-8 h-8 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            <div>
              <p class="text-2xl font-bold text-blue-600 dark:text-blue-400">
                {{ vacationData.total }}
              </p>
              <p class="text-xs text-gray-600 dark:text-gray-400">
                Dni urlopu w roku
              </p>
            </div>
          </div>
        </div>

        <div class="p-4 bg-green-50 dark:bg-green-900/20 rounded-lg border border-green-200 dark:border-green-800">
          <div class="flex items-center gap-3">
            <svg class="w-8 h-8 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div>
              <p class="text-2xl font-bold text-green-600 dark:text-green-400">
                {{ vacationData.remaining }}
              </p>
              <p class="text-xs text-gray-600 dark:text-gray-400">
                Dni pozostało
              </p>
            </div>
          </div>
        </div>

        <div class="p-4 bg-orange-50 dark:bg-orange-900/20 rounded-lg border border-orange-200 dark:border-orange-800">
          <div class="flex items-center gap-3">
            <svg class="w-8 h-8 text-orange-600 dark:text-orange-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div>
              <p class="text-2xl font-bold text-orange-600 dark:text-orange-400">
                {{ vacationData.used }}
              </p>
              <p class="text-xs text-gray-600 dark:text-gray-400">
                Dni wykorzystano
              </p>
            </div>
          </div>
        </div>

        <div class="p-4 bg-red-50 dark:bg-red-900/20 rounded-lg border border-red-200 dark:border-red-800">
          <div class="flex items-center gap-3">
            <svg class="w-8 h-8 text-red-600 dark:text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            <div>
              <p class="text-2xl font-bold text-red-600 dark:text-red-400">
                {{ sickLeaveData.total }}
              </p>
              <p class="text-xs text-gray-600 dark:text-gray-400">
                Dni chorobowe w roku
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- Progress Bar -->
      <div class="mb-6">
        <div class="flex justify-between text-sm mb-2">
          <span class="text-gray-700 dark:text-gray-300">Wykorzystanie urlopu</span>
          <span class="font-semibold text-gray-900 dark:text-white">
            {{ vacationData.used }} / {{ vacationData.total }} dni ({{ Math.round((vacationData.used / vacationData.total) * 100) }}%)
          </span>
        </div>
        <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-3">
          <div
            class="bg-gradient-to-r from-blue-500 to-blue-600 h-3 rounded-full transition-all duration-300"
            :style="{ width: `${(vacationData.used / vacationData.total) * 100}%` }"
          />
        </div>
      </div>

      <!-- Vacation and Sick Leave History -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Vacation History -->
        <div>
          <h4 class="font-semibold text-gray-900 dark:text-white mb-3 flex items-center gap-2">
            <svg class="w-5 h-5 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            Historia urlopów ({{ vacationData.history.length }})
          </h4>
          <div class="space-y-2">
            <div
              v-for="vacation in vacationData.history"
              :key="vacation.id"
              class="p-3 rounded-lg bg-gray-50 dark:bg-gray-700 border border-gray-200 dark:border-gray-600"
            >
              <div class="flex items-start justify-between">
                <div class="flex-1">
                  <div class="flex items-center gap-2 mb-1">
                    <span class="text-sm font-medium text-gray-900 dark:text-white">
                      {{ vacation.type }}
                    </span>
                    <span class="px-2 py-0.5 text-xs font-medium rounded-full bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200">
                      {{ vacation.status }}
                    </span>
                  </div>
                  <p class="text-xs text-gray-600 dark:text-gray-400">
                    {{ formatDateRange(vacation.startDate, vacation.endDate) }}
                  </p>
                </div>
                <span class="text-sm font-bold text-blue-600 dark:text-blue-400">
                  {{ vacation.days }} {{ vacation.days === 1 ? 'dzień' : 'dni' }}
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- Sick Leave History -->
        <div>
          <h4 class="font-semibold text-gray-900 dark:text-white mb-3 flex items-center gap-2">
            <svg class="w-5 h-5 text-red-600 dark:text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            Historia L4 ({{ sickLeaveData.history.length }})
          </h4>
          <div class="space-y-2">
            <div
              v-for="leave in sickLeaveData.history"
              :key="leave.id"
              class="p-3 rounded-lg bg-gray-50 dark:bg-gray-700 border border-gray-200 dark:border-gray-600"
            >
              <div class="flex items-start justify-between">
                <div class="flex-1">
                  <div class="mb-1">
                    <span class="text-sm font-medium text-gray-900 dark:text-white">
                      {{ leave.reason }}
                    </span>
                  </div>
                  <p class="text-xs text-gray-600 dark:text-gray-400">
                    {{ formatDateRange(leave.startDate, leave.endDate) }}
                  </p>
                </div>
                <span class="text-sm font-bold text-red-600 dark:text-red-400">
                  {{ leave.days }} {{ leave.days === 1 ? 'dzień' : 'dni' }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Work Experience -->
      <div class="mt-6 p-4 bg-purple-50 dark:bg-purple-900/20 rounded-lg border border-purple-200 dark:border-purple-800">
        <div class="flex items-center gap-3">
          <svg class="w-8 h-8 text-purple-600 dark:text-purple-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 13.255A23.931 23.931 0 0112 15c-3.183 0-6.22-.62-9-1.745M16 6V4a2 2 0 00-2-2h-4a2 2 0 00-2 2v2m4 6h.01M5 20h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
          </svg>
          <div class="flex-1">
            <p class="text-sm font-medium text-gray-900 dark:text-white">
              Staż pracy w firmie
            </p>
            <p class="text-xs text-gray-600 dark:text-gray-400">
              Od {{ formatDateShort(workData.startDate) }}
            </p>
          </div>
          <div class="text-right">
            <p class="text-2xl font-bold text-purple-600 dark:text-purple-400">
              {{ workData.yearsOfService }}
            </p>
            <p class="text-xs text-gray-600 dark:text-gray-400">
              {{ workData.yearsOfService === 1 ? 'rok' : 'lat' }}
            </p>
          </div>
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

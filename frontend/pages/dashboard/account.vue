<script setup lang="ts">
import type { User } from '~/types/auth'
import type { VacationSummary } from '~/composables/useVacations'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

const authStore = useAuthStore()
const toast = useNotificationToast()

// Use real user data from auth store
const currentUser = computed(() => authStore.user as User)

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

// Calculate stats from real user data
const stats = computed(() => ({
  teamMembers: currentUser.value.subordinates?.length ?? 0,
  lastLogin: currentUser.value.lastLoginAt ? new Date(currentUser.value.lastLoginAt) : null
}))

// Vacation API integration
const { getUserVacationSummary, getMyVacations } = useVacations()
const vacationSummary = ref<VacationSummary | null>(null)
const myVacations = ref<any[]>([])
const selectedYear = ref<number>(new Date().getFullYear())
const availableYears = computed<number[]>(() => {
  const y = new Date().getFullYear()
  return [y - 1, y, y + 1]
})

// Vacation details modal
const showVacationModal = ref(false)
const selectedVacation = ref<any | null>(null)
const openVacationDetails = (id: string | number) => {
  const found = myVacations.value.find(v => v.id === id)
  if (found) {
    selectedVacation.value = found
    showVacationModal.value = true
  }
}
const closeVacationDetails = () => {
  showVacationModal.value = false
  selectedVacation.value = null
}
const isLoadingVacation = ref(false)
const vacationError = ref<string | null>(null)

// Fetch vacation data
const fetchVacationData = async () => {
  if (!currentUser.value?.id) return

  isLoadingVacation.value = true
  vacationError.value = null

  try {
    vacationSummary.value = await getUserVacationSummary(currentUser.value.id)
    // Load my vacations for history (selected year)
    myVacations.value = await getMyVacations(selectedYear.value)
  } catch (error: any) {
    console.error('Error fetching vacation data:', error)
    vacationError.value = error.message || 'Nie udało się pobrać danych urlopowych'
  } finally {
    isLoadingVacation.value = false
  }
}

// Load vacation data on mount
onMounted(() => {
  fetchVacationData()
})

// Computed vacation data - all from real API
const vacationData = computed(() => ({
  total: vacationSummary.value?.annualVacationDays ?? 0,
  used: vacationSummary.value?.vacationDaysUsed ?? 0,
  remaining: vacationSummary.value?.vacationDaysRemaining ?? 0,
  onDemandUsed: vacationSummary.value?.onDemandVacationDaysUsed ?? 0,
  onDemandRemaining: vacationSummary.value?.onDemandVacationDaysRemaining ?? 0,
  carriedOver: vacationSummary.value?.carriedOverVacationDays ?? 0,
  carriedOverExpiry: vacationSummary.value?.carriedOverExpiryDate,
  totalAvailable: vacationSummary.value?.totalAvailableVacationDays ?? 0
}))

// Sick leave data from API
const sickLeaveData = computed(() => ({
  total: vacationSummary.value?.circumstantialLeaveDaysUsed ?? 0
}))

// Work experience calculated from user data
const workData = computed(() => {
  const employmentDate = currentUser.value.employmentStartDate
    ? new Date(currentUser.value.employmentStartDate)
    : null

  const yearsOfService = employmentDate
    ? Math.floor((Date.now() - employmentDate.getTime()) / (1000 * 60 * 60 * 24 * 365))
    : 0

  return {
    startDate: employmentDate,
    yearsOfService
  }
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
  toast.success('Zmiany zostały zapisane!')
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
            {{ user.firstName?.[0] || '' }}{{ user.lastName?.[0] || '' }}
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
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div class="text-center p-4 bg-purple-50 dark:bg-purple-900/20 rounded-lg">
          <p class="text-3xl font-bold text-purple-600 dark:text-purple-400">
            {{ stats.teamMembers }}
          </p>
          <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
            Członków zespołu
          </p>
        </div>
        <div class="text-center p-4 bg-orange-50 dark:bg-orange-900/20 rounded-lg">
          <div v-if="stats.lastLogin">
            <p class="text-sm font-medium text-gray-900 dark:text-white">
              Ostatnie logowanie
            </p>
            <p class="text-xs text-gray-600 dark:text-gray-400 mt-1">
              {{ formatDate(stats.lastLogin) }}
            </p>
          </div>
          <div v-else class="text-sm text-gray-500 dark:text-gray-400">
            Brak danych o ostatnim logowaniu
          </div>
        </div>
      </div>
    </div>

    <!-- Vacation and Sick Leave Section -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-6">
        Urlopy i absencje
      </h3>

      <!-- Loading State -->
      <div v-if="isLoadingVacation" class="flex justify-center items-center py-12">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600" />
      </div>

      <!-- Error State -->
      <div v-else-if="vacationError" class="p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg mb-6">
        <p class="text-sm text-red-600 dark:text-red-400">
          {{ vacationError }}
        </p>
      </div>

      <!-- Vacation Data -->
      <template v-else>
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

      <!-- Work Experience (if available) -->
        <div v-if="workData.startDate" class="mt-6 p-4 bg-purple-50 dark:bg-purple-900/20 rounded-lg border border-purple-200 dark:border-purple-800">
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

        <!-- Vacation History -->
        <div class="mt-6 space-y-4">
          <div class="flex items-center gap-3">
            <label class="text-sm text-gray-700 dark:text-gray-300">Rok:</label>
            <select
              v-model.number="selectedYear"
              @change="fetchVacationData"
              class="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded bg-white dark:bg-gray-800 text-gray-900 dark:text-white"
            >
              <option v-for="y in availableYears" :key="y" :value="y">{{ y }}</option>
            </select>
          </div>

          <VacationHistory
            :entries="myVacations.map(v => ({ id: v.id, startDate: new Date(v.startDate), endDate: new Date(v.endDate), days: Math.ceil((new Date(v.endDate).getTime() - new Date(v.startDate).getTime())/(1000*60*60*24)) + 1, type: 'Urlop', status: v.status }))"
            title="Historia urlopów"
            empty-message="Brak historii urlopów w tym roku"
            @view-details="openVacationDetails"
          />

          <!-- Vacation Details Modal -->
          <Teleport to="body">
            <div
              v-if="showVacationModal && selectedVacation"
              class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
              @click.self="closeVacationDetails"
            >
              <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-lg w-full p-6">
                <div class="flex items-start justify-between mb-4">
                  <h3 class="text-xl font-bold text-gray-900 dark:text-white">Szczegóły urlopu</h3>
                  <button class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200" @click="closeVacationDetails">✕</button>
                </div>
                <div class="space-y-3 text-sm">
                  <div class="flex justify-between">
                    <span class="text-gray-500 dark:text-gray-400">Zakres</span>
                    <span class="font-medium text-gray-900 dark:text-white">{{ new Date(selectedVacation.startDate).toLocaleDateString('pl-PL') }} – {{ new Date(selectedVacation.endDate).toLocaleDateString('pl-PL') }}</span>
                  </div>
                  <div class="flex justify-between">
                    <span class="text-gray-500 dark:text-gray-400">Dni</span>
                    <span class="font-medium">{{ Math.ceil((new Date(selectedVacation.endDate).getTime() - new Date(selectedVacation.startDate).getTime())/(1000*60*60*24)) + 1 }}</span>
                  </div>
                  <div class="flex justify-between">
                    <span class="text-gray-500 dark:text-gray-400">Status</span>
                    <span class="font-medium">{{ selectedVacation.status }}</span>
                  </div>
                  <div class="flex justify-between">
                    <span class="text-gray-500 dark:text-gray-400">Zastępca</span>
                    <span class="font-medium">{{ selectedVacation.substitute ? `${selectedVacation.substitute.firstName} ${selectedVacation.substitute.lastName}` : '—' }}</span>
                  </div>
                  <div class="flex justify-between" v-if="selectedVacation.createdAt">
                    <span class="text-gray-500 dark:text-gray-400">Utworzone</span>
                    <span class="font-medium">{{ new Date(selectedVacation.createdAt).toLocaleString('pl-PL') }}</span>
                  </div>
                </div>
                <div class="mt-6 flex justify-end gap-2">
                  <button class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded" @click="$router.push('/dashboard/team/vacation-calendar')">Przejdź do kalendarza</button>
                  <button class="px-4 py-2 bg-gray-200 dark:bg-gray-700 text-gray-900 dark:text-white rounded" @click="closeVacationDetails">Zamknij</button>
                </div>
              </div>
            </div>
          </Teleport>
        </div>
      </template>
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

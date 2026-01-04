<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Header -->
      <div class="mb-8">
        <NuxtLink
          to="/admin"
          class="inline-flex items-center text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300 mb-4"
        >
          <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
          Powrót do panelu administracyjnego
        </NuxtLink>
        <div class="flex justify-between items-center">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Serwisy Wewnętrzne</h1>
            <p class="mt-2 text-gray-600 dark:text-gray-400">Zarządzaj linkami do narzędzi i systemów wewnętrznych</p>
          </div>
          <div class="flex gap-3">
            <button
              class="inline-flex items-center px-4 py-2 bg-gray-600 dark:bg-gray-500 text-white rounded-lg hover:bg-gray-700 dark:hover:bg-gray-600 transition-colors"
              @click="showCategoryModal = true"
            >
              <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
              </svg>
              Kategorie
            </button>
            <button
              class="inline-flex items-center px-4 py-2 bg-blue-600 dark:bg-blue-500 text-white rounded-lg hover:bg-blue-700 dark:hover:bg-blue-600 transition-colors"
              @click="openCreateModal"
            >
              <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
              Dodaj Serwis
            </button>
          </div>
        </div>
      </div>

      <!-- Filters -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 mb-6">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Szukaj</label>
            <input
              v-model="searchTerm"
              type="text"
              placeholder="Nazwa serwisu..."
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Kategoria</label>
            <select
              v-model="selectedCategory"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="">Wszystkie kategorie</option>
              <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
            </select>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Status</label>
            <select
              v-model="selectedStatus"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="all">Wszystkie</option>
              <option value="active">Aktywne</option>
              <option value="inactive">Nieaktywne</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 dark:border-blue-400"/>
        <p class="mt-4 text-gray-600 dark:text-gray-400">Ładowanie serwisów...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-800 rounded-lg p-4 mb-6">
        <p class="text-red-800 dark:text-red-300">{{ error }}</p>
      </div>

      <!-- Services Table -->
      <div v-else class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden">
        <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
          <thead class="bg-gray-50 dark:bg-gray-700">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Ikona
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Nazwa / Opis
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Kategoria
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Zakres
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Status
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Akcje
              </th>
            </tr>
          </thead>
          <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
            <tr v-for="service in filteredServices" :key="service.id" class="hover:bg-gray-50 dark:hover:bg-gray-700">
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-2xl">
                  <span v-if="service.iconType === 'emoji'">{{ service.icon || '🔗' }}</span>
                  <img v-else-if="service.iconType === 'image'" :src="service.icon" alt="" class="w-8 h-8 rounded" >
                  <i v-else :class="service.icon" class="text-gray-600 dark:text-gray-400"/>
                </div>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-2">
                  <div>
                    <div class="text-sm font-medium text-gray-900 dark:text-white flex items-center gap-2">
                      {{ service.name }}
                      <span v-if="service.isPinned" class="text-yellow-500" title="Przypięty">📌</span>
                    </div>
                    <div class="text-sm text-gray-500 dark:text-gray-400">{{ service.description }}</div>
                    <a :href="service.url" target="_blank" class="text-xs text-blue-600 dark:text-blue-400 hover:underline">
                      {{ service.url }}
                    </a>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span v-if="service.categoryName" class="text-sm text-gray-900 dark:text-white">
                  {{ service.categoryName }}
                </span>
                <span v-else class="text-sm text-gray-400 dark:text-gray-500">Brak kategorii</span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span
                  v-if="service.isGlobal"
                  class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400"
                >
                  Globalny
                </span>
                <span
                  v-else
                  class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400"
                >
                  {{ service.departmentIds.length }} działów
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span
                  class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full"
                  :class="service.isActive ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400' : 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400'"
                >
                  {{ service.isActive ? 'Aktywny' : 'Nieaktywny' }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                <button
                  class="text-blue-600 dark:text-blue-400 hover:text-blue-900 dark:hover:text-blue-300 mr-4"
                  @click="openEditModal(service)"
                >
                  Edytuj
                </button>
                <button
                  class="text-red-600 dark:text-red-400 hover:text-red-900 dark:hover:text-red-300"
                  @click="confirmDelete(service)"
                >
                  Usuń
                </button>
              </td>
            </tr>
          </tbody>
        </table>

        <div v-if="filteredServices.length === 0" class="text-center py-12">
          <p class="text-gray-500 dark:text-gray-400">Brak serwisów do wyświetlenia</p>
        </div>
      </div>
    </div>

    <!-- Service Modal (Create/Edit) -->
    <InternalServicesServiceModal
      v-if="showServiceModal"
      :service="editingService"
      :categories="categories"
      @close="showServiceModal = false"
      @saved="handleServiceSaved"
    />

    <!-- Category Management Modal -->
    <InternalServicesCategoryModal
      v-if="showCategoryModal"
      :categories="categories"
      @close="showCategoryModal = false"
      @saved="loadCategories"
    />
  </div>
</template>

<script setup lang="ts">
import type { InternalService, InternalServiceCategory } from '~/types/internal-services'

definePageMeta({
  middleware: ['auth', 'verified', 'admin'],
  layout: 'default'
})

const { fetchAllServices, deleteService, fetchAllCategories } = useInternalServicesApi()
const toast = useNotificationToast()
const confirmModal = useConfirmModal()

const services = ref<InternalService[]>([])
const categories = ref<InternalServiceCategory[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

const searchTerm = ref('')
const selectedCategory = ref('')
const selectedStatus = ref('all')

const showServiceModal = ref(false)
const showCategoryModal = ref(false)
const editingService = ref<InternalService | null>(null)

const filteredServices = computed(() => {
  return services.value.filter(service => {
    const matchesSearch = service.name.toLowerCase().includes(searchTerm.value.toLowerCase()) ||
                         service.description.toLowerCase().includes(searchTerm.value.toLowerCase())

    const matchesCategory = !selectedCategory.value || service.categoryId === selectedCategory.value

    const matchesStatus = selectedStatus.value === 'all' ||
                         (selectedStatus.value === 'active' && service.isActive) ||
                         (selectedStatus.value === 'inactive' && !service.isActive)

    return matchesSearch && matchesCategory && matchesStatus
  })
})

async function loadServices() {
  loading.value = true
  error.value = null
  try {
    services.value = await fetchAllServices()
  } catch (err: unknown) {
    error.value = err instanceof Error ? err.message : 'Nie udało się załadować serwisów'
    console.error('Error loading services:', err)
  } finally {
    loading.value = false
  }
}

async function loadCategories() {
  try {
    categories.value = await fetchAllCategories()
  } catch (err) {
    console.error('Error loading categories:', err)
  }
}

function openCreateModal() {
  editingService.value = null
  showServiceModal.value = true
}

function openEditModal(service: InternalService) {
  editingService.value = service
  showServiceModal.value = true
}

async function confirmDelete(service: InternalService) {
  const confirmed = await confirmModal.confirmDelete(`serwis "${service.name}"`)

  if (confirmed) {
    try {
      await deleteService(service.id)
      await loadServices()
      toast.success('Serwis został usunięty')
    } catch (err: unknown) {
      toast.error('Nie udało się usunąć serwisu', err instanceof Error ? err.message : undefined)
    }
  }
}

function handleServiceSaved() {
  showServiceModal.value = false
  loadServices()
}

onMounted(() => {
  loadServices()
  loadCategories()
})
</script>

<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-2">
          Szablony wniosków
        </h1>
        <p class="text-gray-600 dark:text-gray-400">
          Zarządzaj szablonami wniosków dostępnymi w systemie
        </p>
      </div>

      <!-- Actions Bar -->
      <div class="mb-6 flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between">
        <div class="flex-1 max-w-md">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Szukaj szablonów..."
            class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-800 dark:text-white"
          >
        </div>
        
        <NuxtLink
          to="/admin/request-templates/create"
          class="inline-flex items-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors"
        >
          <Plus class="w-5 h-5 mr-2" />
          Nowy szablon
        </NuxtLink>
      </div>

      <!-- Filters -->
      <div class="mb-6 flex flex-wrap gap-3">
        <button
          @click="filterCategory = ''"
          :class="[
            'px-4 py-2 rounded-lg font-medium transition-colors',
            filterCategory === '' 
              ? 'bg-blue-600 text-white' 
              : 'bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 border border-gray-300 dark:border-gray-600 hover:bg-gray-50 dark:hover:bg-gray-700'
          ]"
        >
          Wszystkie
        </button>
        <button
          v-for="category in uniqueCategories"
          :key="category"
          @click="filterCategory = category"
          :class="[
            'px-4 py-2 rounded-lg font-medium transition-colors',
            filterCategory === category 
              ? 'bg-blue-600 text-white' 
              : 'bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 border border-gray-300 dark:border-gray-600 hover:bg-gray-50 dark:hover:bg-gray-700'
          ]"
        >
          {{ category }}
        </button>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex items-center justify-center py-12">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
        <p class="text-red-800 dark:text-red-200">{{ error }}</p>
      </div>

      <!-- Templates Grid -->
      <div v-else-if="filteredTemplates.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div
          v-for="template in filteredTemplates"
          :key="template.id"
          class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 hover:shadow-md transition-shadow overflow-hidden"
        >
          <!-- Card Header -->
          <div class="p-6 border-b border-gray-200 dark:border-gray-700">
            <div class="flex items-start justify-between mb-3">
              <Icon
                :name="getIconifyName(template.icon)"
                class="w-10 h-10"
              />
              <span
                :class="[
                  'px-2 py-1 text-xs font-medium rounded-full',
                  template.isActive
                    ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                    : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-200'
                ]"
              >
                {{ template.isActive ? 'Aktywny' : 'Nieaktywny' }}
              </span>
            </div>
            
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
              {{ template.name }}
            </h3>
            
            <p class="text-sm text-gray-600 dark:text-gray-400 line-clamp-2">
              {{ template.description }}
            </p>
          </div>

          <!-- Card Body -->
          <div class="p-6">
            <div class="space-y-2 text-sm">
              <div class="flex items-center text-gray-600 dark:text-gray-400">
                <Tag class="w-4 h-4 mr-2" />
                <span>{{ template.category }}</span>
              </div>
              
              <div v-if="template.departmentId" class="flex items-center text-gray-600 dark:text-gray-400">
                <Building class="w-4 h-4 mr-2" />
                <span>Dział: {{ template.departmentId }}</span>
              </div>
              <div v-else class="flex items-center text-gray-600 dark:text-gray-400">
                <Globe class="w-4 h-4 mr-2" />
                <span>Dostępny dla wszystkich</span>
              </div>
              
              <div v-if="template.estimatedProcessingDays" class="flex items-center text-gray-600 dark:text-gray-400">
                <Clock class="w-4 h-4 mr-2" />
                <span>{{ template.estimatedProcessingDays }} dni</span>
              </div>
              
              <div class="flex items-center text-gray-600 dark:text-gray-400">
                <FileText class="w-4 h-4 mr-2" />
                <span>{{ template.fields.length }} pól</span>
              </div>
              
              <div class="flex items-center text-gray-600 dark:text-gray-400">
                <Users class="w-4 h-4 mr-2" />
                <span>{{ template.approvalStepTemplates.length }} etap(ów) zatwierdzania</span>
              </div>
            </div>
          </div>

          <!-- Card Footer -->
          <div class="px-6 py-4 bg-gray-50 dark:bg-gray-700/50 flex items-center justify-between">
            <div class="flex gap-3">
              <NuxtLink
                :to="`/admin/request-templates/edit/${template.id}`"
                class="text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300 font-medium text-sm"
              >
                Edytuj
              </NuxtLink>

              <button
                @click="viewTemplate(template)"
                class="text-gray-600 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 font-medium text-sm"
              >
                Szczegóły
              </button>
            </div>

            <button
              @click="confirmDelete(template)"
              class="text-red-600 dark:text-red-400 hover:text-red-700 dark:hover:text-red-300 font-medium text-sm flex items-center gap-1"
              :disabled="deleting === template.id"
            >
              <Trash2 class="w-4 h-4" />
              {{ deleting === template.id ? 'Usuwanie...' : 'Usuń' }}
            </button>
          </div>
        </div>
      </div>

      <!-- Empty State - No templates at all -->
      <div v-else-if="templates.length === 0" class="text-center py-12">
        <FileText class="w-16 h-16 mx-auto text-gray-400 mb-4" />
        <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-2">
          Nie ma jeszcze żadnych szablonów wniosków
        </h3>
        <p class="text-gray-600 dark:text-gray-400 mb-6">
          Rozpocznij od utworzenia pierwszego szablonu wniosku dla swojej organizacji lub załaduj przykładowe szablony
        </p>
        <div class="flex gap-3 justify-center">
          <NuxtLink
            to="/admin/request-templates/create"
            class="inline-flex items-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors"
          >
            <Plus class="w-5 h-5 mr-2" />
            Utwórz pierwszy szablon
          </NuxtLink>
          <button
            @click="seedTemplates"
            :disabled="seeding"
            class="inline-flex items-center px-4 py-2 bg-green-600 hover:bg-green-700 disabled:bg-gray-400 text-white font-medium rounded-lg transition-colors"
          >
            <template v-if="seeding">
              <div class="animate-spin rounded-full h-5 w-5 border-b-2 border-white mr-2"></div>
              Ładowanie...
            </template>
            <template v-else>
              Załaduj przykładowe szablony
            </template>
          </button>
        </div>
      </div>

      <!-- Empty State - No results after filtering -->
      <div v-else class="text-center py-12">
        <FileText class="w-16 h-16 mx-auto text-gray-400 mb-4" />
        <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-2">
          Brak wyników wyszukiwania
        </h3>
        <p class="text-gray-600 dark:text-gray-400 mb-6">
          Nie znaleziono żadnych szablonów spełniających kryteria wyszukiwania
        </p>
        <button
          @click="clearFilters"
          class="inline-flex items-center px-4 py-2 bg-gray-600 hover:bg-gray-700 text-white font-medium rounded-lg transition-colors"
        >
          Wyczyść filtry
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { Plus, Tag, Building, Globe, Clock, FileText, Users, Trash2 } from 'lucide-vue-next'
import type { RequestTemplate } from '~/types/requests'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'admin', 'request-templates-admin']
})

const { getAllTemplates, deleteTemplate } = useRequestsApi()
const toast = useNotificationToast()
const confirmModal = useConfirmModal()

const templates = ref<RequestTemplate[]>([])
const loading = ref(true)
const error = ref('')
const searchQuery = ref('')
const filterCategory = ref('')
const seeding = ref(false)
const deleting = ref<string | null>(null)

const uniqueCategories = computed(() => {
  const categories = templates.value.map(t => t.category)
  return [...new Set(categories)].sort()
})

const filteredTemplates = computed(() => {
  let result = templates.value

  // Filter by category
  if (filterCategory.value) {
    result = result.filter(t => t.category === filterCategory.value)
  }

  // Filter by search query
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(t => 
      t.name.toLowerCase().includes(query) ||
      t.description.toLowerCase().includes(query) ||
      t.category.toLowerCase().includes(query)
    )
  }

  return result
})

// Icon mapping - same as in requests.vue
const iconMapping: Record<string, string> = {
  'beach-umbrella': 'fluent-emoji-flat:beach-with-umbrella',
  plane: 'fluent-emoji-flat:airplane',
  calendar: 'heroicons:calendar-days',
  laptop: 'heroicons:computer-desktop',
  toolbox: 'heroicons:wrench-screwdriver',
  document: 'heroicons:document-text',
  folder: 'heroicons:folder',
  clipboard: 'heroicons:clipboard-document-list',
  shield: 'heroicons:shield-check',
  warning: 'heroicons:exclamation-triangle',
  graduation: 'heroicons:academic-cap',
  book: 'heroicons:book-open',
  users: 'heroicons:user-group',
  bell: 'heroicons:bell',
  check: 'heroicons:check-circle'
}

const getIconifyName = (iconName: string) => {
  return iconMapping[iconName] || 'heroicons:question-mark-circle'
}

const loadTemplates = async () => {
  try {
    loading.value = true
    error.value = ''
    templates.value = await getAllTemplates()
  } catch (err: any) {
    console.error('Error loading templates:', err)
    error.value = 'Nie udało się załadować szablonów. Spróbuj ponownie później.'
  } finally {
    loading.value = false
  }
}

const viewTemplate = (template: RequestTemplate) => {
  // Could open a modal or navigate to detail view
  navigateTo(`/admin/request-templates/edit/${template.id}`)
}

const clearFilters = () => {
  searchQuery.value = ''
  filterCategory.value = ''
}

const confirmDelete = async (template: RequestTemplate) => {
  const confirmed = await confirmModal.confirmDelete(
    `szablon "${template.name}"`,
    'Ta operacja jest nieodwracalna. Szablon zostanie usunięty z systemu.'
  )

  if (confirmed) {
    await handleDelete(template.id)
  }
}

const handleDelete = async (templateId: string) => {
  try {
    deleting.value = templateId
    error.value = ''

    await deleteTemplate(templateId)

    // Remove from local array
    templates.value = templates.value.filter(t => t.id !== templateId)

    toast.success('Sukces!', 'Szablon został usunięty pomyślnie')
  } catch (err: any) {
    console.error('Error deleting template:', err)

    // Check if error is due to template being used
    if (err.data?.errors && Array.isArray(err.data.errors)) {
      const errorMessage = err.data.errors.join(', ')
      toast.error('Nie można usunąć szablonu', errorMessage)
    } else {
      toast.error('Błąd', 'Nie udało się usunąć szablonu. Spróbuj ponownie później.')
    }
  } finally {
    deleting.value = null
  }
}

const seedTemplates = async () => {
  try {
    seeding.value = true
    error.value = ''

    const config = useRuntimeConfig()
    const { getAuthHeaders } = useAuth()

    await $fetch(`${config.public.apiUrl}/api/request-templates/seed`, {
      method: 'POST',
      headers: getAuthHeaders()
    })

    // Reload templates after seeding
    await loadTemplates()

    toast.success('Sukces!', 'Przykładowe szablony zostały załadowane pomyślnie')
  } catch (err: any) {
    console.error('Error seeding templates:', err)
    toast.error('Błąd', 'Nie udało się załadować przykładowych szablonów. Spróbuj ponownie później.')
  } finally {
    seeding.value = false
  }
}

onMounted(() => {
  loadTemplates()
})
</script>


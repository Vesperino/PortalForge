<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Loading State -->
      <div v-if="loading" class="flex items-center justify-center py-12">
        <div class="text-center">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p class="text-gray-600 dark:text-gray-400">Ładowanie szablonu...</p>
        </div>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-6">
        <h3 class="text-lg font-semibold text-red-900 dark:text-red-200 mb-2">Błąd</h3>
        <p class="text-red-700 dark:text-red-300">{{ error }}</p>
        <NuxtLink
          to="/admin/request-templates"
          class="inline-flex items-center mt-4 text-blue-600 dark:text-blue-400 hover:text-blue-700"
        >
          <ArrowLeft class="w-4 h-4 mr-2" />
          Powrót do listy
        </NuxtLink>
      </div>

      <!-- Form -->
      <div v-else>
        <!-- Header -->
        <div class="mb-8">
          <NuxtLink
            to="/admin/request-templates"
            class="inline-flex items-center text-blue-600 dark:text-blue-400 hover:text-blue-700 mb-4"
          >
            <ArrowLeft class="w-4 h-4 mr-2" />
            Powrót do listy
          </NuxtLink>
          
          <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-2">
            Edytuj szablon wniosku
          </h1>
          <p class="text-gray-600 dark:text-gray-400">
            Modyfikuj szablon wniosku: {{ form.name }}
          </p>
        </div>

        <!-- Simple Form (without steps for now) -->
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
          <div class="space-y-6">
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Nazwa szablonu *
              </label>
              <input
                v-model="form.name"
                type="text"
                required
                placeholder="np. Zamówienie sprzętu IT"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
              >
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Opis *
              </label>
              <textarea
                v-model="form.description"
                rows="3"
                required
                placeholder="Opisz cel i zastosowanie tego szablonu..."
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
              />
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Kategoria *
              </label>
              <input
                v-model="form.category"
                type="text"
                required
                placeholder="np. Hardware, Software, HR"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
              >
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Ikona
              </label>
              <input
                v-model="form.icon"
                type="text"
                placeholder="np. FileText, Laptop, Users"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
              >
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Szacowany czas przetwarzania (dni)
              </label>
              <input
                v-model.number="form.estimatedProcessingDays"
                type="number"
                min="1"
                placeholder="np. 7"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
              >
            </div>

            <div class="flex items-center">
              <input
                v-model="form.isActive"
                type="checkbox"
                id="isActive"
                class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
              >
              <label for="isActive" class="ml-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                Szablon aktywny
              </label>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex justify-between items-center mt-8 pt-6 border-t border-gray-200 dark:border-gray-700">
            <NuxtLink
              to="/admin/request-templates"
              class="inline-flex items-center px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700"
            >
              <ArrowLeft class="w-4 h-4 mr-2" />
              Anuluj
            </NuxtLink>

            <button
              @click="saveTemplate"
              :disabled="saving"
              class="inline-flex items-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg disabled:opacity-50"
            >
              <Save class="w-4 h-4 mr-2" />
              {{ saving ? 'Zapisywanie...' : 'Zapisz zmiany' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ArrowLeft, Save } from 'lucide-vue-next'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'admin', 'request-templates-admin']
})

const route = useRoute()
const router = useRouter()
const { getTemplateById, updateTemplate } = useRequestsApi()

const templateId = computed(() => route.params.id as string)

const loading = ref(true)
const saving = ref(false)
const error = ref<string | null>(null)

const form = ref({
  name: '',
  description: '',
  icon: 'FileText',
  category: '',
  estimatedProcessingDays: undefined as number | undefined,
  isActive: true
})

const loadTemplate = async () => {
  try {
    loading.value = true
    error.value = null
    
    const template = await getTemplateById(templateId.value)
    
    if (!template) {
      error.value = 'Szablon nie został znaleziony'
      return
    }

    // Populate form with template data
    form.value = {
      name: template.name,
      description: template.description,
      icon: template.icon || 'FileText',
      category: template.category,
      estimatedProcessingDays: template.estimatedProcessingDays,
      isActive: template.isActive
    }
  } catch (err: any) {
    console.error('Error loading template:', err)
    error.value = err.message || 'Nie udało się załadować szablonu'
  } finally {
    loading.value = false
  }
}

const saveTemplate = async () => {
  try {
    saving.value = true

    await updateTemplate(templateId.value, {
      name: form.value.name,
      description: form.value.description,
      icon: form.value.icon,
      category: form.value.category,
      estimatedProcessingDays: form.value.estimatedProcessingDays,
      isActive: form.value.isActive
    })

    alert('Szablon zaktualizowany pomyślnie!')
    router.push('/admin/request-templates')
  } catch (err: any) {
    console.error('Error updating template:', err)
    alert('Błąd podczas aktualizacji szablonu: ' + (err.message || 'Nieznany błąd'))
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  loadTemplate()
})
</script>


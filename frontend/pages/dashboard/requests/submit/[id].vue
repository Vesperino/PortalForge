<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Loading -->
      <div v-if="loading" class="text-center py-12">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
      </div>

      <!-- Error -->
      <div v-else-if="error || !template" class="text-center py-12">
        <p class="text-red-600">{{ error || 'Szablon nie został znaleziony' }}</p>
        <NuxtLink to="/dashboard/requests" class="text-blue-600 hover:underline mt-4 inline-block">
          Powrót do wniosków
        </NuxtLink>
      </div>

      <!-- Form -->
      <div v-else>
        <!-- Header -->
        <div class="mb-8">
          <NuxtLink
            to="/dashboard/requests"
            class="inline-flex items-center text-blue-600 dark:text-blue-400 hover:text-blue-700 mb-4"
          >
            <ArrowLeft class="w-4 h-4 mr-2" />
            Powrót do listy
          </NuxtLink>
          
          <div class="flex items-center gap-4 mb-4">
            <Icon
              :name="getIconifyName(template.icon)"
              class="w-12 h-12"
            />
            <div>
              <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
                {{ template.name }}
              </h1>
              <p class="text-gray-600 dark:text-gray-400">
                {{ template.description }}
              </p>
            </div>
          </div>

          <div v-if="template.estimatedProcessingDays" class="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400">
            <Clock class="w-4 h-4" />
            <span>Szacowany czas procesowania: {{ template.estimatedProcessingDays }} dni</span>
          </div>
        </div>

        <!-- Form Fields -->
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6 mb-6">
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-6">
            Wypełnij formularz
          </h2>

          <div class="space-y-6">
            <div v-for="field in sortedFields" :key="field.id">
              <!-- Text -->
              <div v-if="field.fieldType === 'Text'">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {{ field.label }}
                  <span v-if="field.isRequired" class="text-red-500">*</span>
                </label>
                <input
                  v-model="formData[field.id!]"
                  type="text"
                  :placeholder="field.placeholder"
                  :required="field.isRequired"
                  class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
                >
                <p v-if="field.helpText" class="mt-1 text-sm text-gray-500">{{ field.helpText }}</p>
              </div>

              <!-- Textarea -->
              <div v-else-if="field.fieldType === 'Textarea'">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {{ field.label }}
                  <span v-if="field.isRequired" class="text-red-500">*</span>
                </label>
                <textarea
                  v-model="formData[field.id!]"
                  rows="4"
                  :placeholder="field.placeholder"
                  :required="field.isRequired"
                  class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
                />
                <p v-if="field.helpText" class="mt-1 text-sm text-gray-500">{{ field.helpText }}</p>
              </div>

              <!-- Number -->
              <div v-else-if="field.fieldType === 'Number'">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {{ field.label }}
                  <span v-if="field.isRequired" class="text-red-500">*</span>
                </label>
                <input
                  v-model.number="formData[field.id!]"
                  type="number"
                  :placeholder="field.placeholder"
                  :required="field.isRequired"
                  :min="field.minValue"
                  :max="field.maxValue"
                  class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
                >
                <p v-if="field.helpText" class="mt-1 text-sm text-gray-500">{{ field.helpText }}</p>
              </div>

              <!-- Select -->
              <div v-else-if="field.fieldType === 'Select'">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {{ field.label }}
                  <span v-if="field.isRequired" class="text-red-500">*</span>
                </label>
                <select
                  v-model="formData[field.id!]"
                  :required="field.isRequired"
                  class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
                >
                  <option value="">Wybierz...</option>
                  <option
                    v-for="option in parseOptions(field.options)"
                    :key="option.value"
                    :value="option.value"
                  >
                    {{ option.label }}
                  </option>
                </select>
                <p v-if="field.helpText" class="mt-1 text-sm text-gray-500">{{ field.helpText }}</p>
              </div>

              <!-- Date -->
              <div v-else-if="field.fieldType === 'Date'">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {{ field.label }}
                  <span v-if="field.isRequired" class="text-red-500">*</span>
                </label>
                <input
                  v-model="formData[field.id!]"
                  type="date"
                  :required="field.isRequired"
                  class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
                >
                <p v-if="field.helpText" class="mt-1 text-sm text-gray-500">{{ field.helpText }}</p>
              </div>

              <!-- Checkbox -->
              <div v-else-if="field.fieldType === 'Checkbox'">
                <label class="flex items-center">
                  <input
                    v-model="formData[field.id!]"
                    type="checkbox"
                    :required="field.isRequired"
                    class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                  >
                  <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">
                    {{ field.label }}
                    <span v-if="field.isRequired" class="text-red-500">*</span>
                  </span>
                </label>
                <p v-if="field.helpText" class="mt-1 text-sm text-gray-500">{{ field.helpText }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Priority -->
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6 mb-6">
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-4">
            Priorytet
          </h2>
          <div class="flex gap-4">
            <label class="flex items-center">
              <input
                v-model="priority"
                type="radio"
                value="Standard"
                class="w-4 h-4 text-blue-600 border-gray-300 focus:ring-blue-500"
              >
              <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">
                Standard
              </span>
            </label>
            <label class="flex items-center">
              <input
                v-model="priority"
                type="radio"
                value="Urgent"
                class="w-4 h-4 text-red-600 border-gray-300 focus:ring-red-500"
              >
              <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">
                Pilne
              </span>
            </label>
          </div>
        </div>

        <!-- Submit -->
        <div class="flex items-center justify-between">
          <NuxtLink
            to="/dashboard/requests"
            class="px-6 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700"
          >
            Anuluj
          </NuxtLink>

          <button
            @click="submitRequest"
            :disabled="submitting"
            class="px-6 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed text-white rounded-lg font-medium transition-colors"
          >
            {{ submitting ? 'Wysyłanie...' : 'Złóż wniosek' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { ArrowLeft, Clock } from 'lucide-vue-next'
import type { RequestTemplate, RequestPriority } from '~/types/requests'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

const route = useRoute()
const { getTemplateById, submitRequest: submitRequestApi } = useRequestsApi()

const templateId = route.params.id as string
const template = ref<RequestTemplate | null>(null)
const loading = ref(true)
const error = ref('')
const formData = ref<Record<string, any>>({})
const priority = ref<RequestPriority>('Standard')
const submitting = ref(false)

const sortedFields = computed(() => {
  if (!template.value) return []
  return [...template.value.fields].sort((a, b) => a.order - b.order)
})

// Icon mapping for Fluent Emoji Flat icons
const iconMapping: Record<string, string> = {
  'beach-umbrella': 'fluent-emoji-flat:beach-with-umbrella',
  'palm-tree': 'fluent-emoji-flat:palm-tree',
  'laptop': 'fluent-emoji-flat:laptop',
  'desktop': 'fluent-emoji-flat:desktop-computer',
  'phone': 'fluent-emoji-flat:mobile-phone',
  'briefcase': 'fluent-emoji-flat:briefcase',
  'calendar': 'fluent-emoji-flat:calendar',
  'clock': 'fluent-emoji-flat:alarm-clock',
  'document': 'fluent-emoji-flat:page-facing-up',
  'folder': 'fluent-emoji-flat:file-folder',
  'email': 'fluent-emoji-flat:e-mail',
  'user': 'fluent-emoji-flat:bust-in-silhouette',
  'users': 'fluent-emoji-flat:busts-in-silhouette',
  'settings': 'fluent-emoji-flat:gear',
  'chart': 'fluent-emoji-flat:chart-increasing',
  'money': 'fluent-emoji-flat:money-bag',
  'gift': 'fluent-emoji-flat:wrapped-gift',
  'star': 'fluent-emoji-flat:star',
  'heart': 'fluent-emoji-flat:red-heart',
  'home': 'fluent-emoji-flat:house',
  'car': 'fluent-emoji-flat:automobile',
  'plane': 'fluent-emoji-flat:airplane',
  'train': 'fluent-emoji-flat:locomotive',
  'bus': 'fluent-emoji-flat:bus',
  'bicycle': 'fluent-emoji-flat:bicycle',
  'camera': 'fluent-emoji-flat:camera',
  'video': 'fluent-emoji-flat:video-camera',
  'music': 'fluent-emoji-flat:musical-note',
  'book': 'fluent-emoji-flat:books',
  'graduation': 'fluent-emoji-flat:graduation-cap',
  'trophy': 'fluent-emoji-flat:trophy',
  'medal': 'fluent-emoji-flat:sports-medal',
  'flag': 'fluent-emoji-flat:triangular-flag',
  'bell': 'fluent-emoji-flat:bell',
  'lock': 'fluent-emoji-flat:locked',
  'key': 'fluent-emoji-flat:key',
  'shield': 'fluent-emoji-flat:shield',
  'warning': 'fluent-emoji-flat:warning',
  'info': 'fluent-emoji-flat:information',
  'check': 'fluent-emoji-flat:check-mark-button',
  'cross': 'fluent-emoji-flat:cross-mark',
  'plus': 'fluent-emoji-flat:plus',
  'minus': 'fluent-emoji-flat:minus',
  'search': 'fluent-emoji-flat:magnifying-glass-tilted-left',
  'filter': 'fluent-emoji-flat:funnel',
  'download': 'fluent-emoji-flat:down-arrow',
  'upload': 'fluent-emoji-flat:up-arrow',
  'refresh': 'fluent-emoji-flat:counterclockwise-arrows-button',
  'trash': 'fluent-emoji-flat:wastebasket',
  'edit': 'fluent-emoji-flat:pencil',
  'copy': 'fluent-emoji-flat:clipboard',
  'paste': 'fluent-emoji-flat:page-with-curl',
  'cut': 'fluent-emoji-flat:scissors',
  'link': 'fluent-emoji-flat:link',
  'unlink': 'fluent-emoji-flat:broken-chain',
  'image': 'fluent-emoji-flat:framed-picture',
  'file': 'fluent-emoji-flat:page-facing-up',
  'archive': 'fluent-emoji-flat:card-file-box',
  'database': 'fluent-emoji-flat:floppy-disk',
  'server': 'fluent-emoji-flat:desktop-computer',
  'cloud': 'fluent-emoji-flat:cloud',
  'sun': 'fluent-emoji-flat:sun',
  'moon': 'fluent-emoji-flat:crescent-moon',
  'umbrella': 'fluent-emoji-flat:umbrella',
  'snowflake': 'fluent-emoji-flat:snowflake',
  'fire': 'fluent-emoji-flat:fire',
  'water': 'fluent-emoji-flat:droplet',
  'leaf': 'fluent-emoji-flat:leaf-fluttering-in-wind',
  'tree': 'fluent-emoji-flat:deciduous-tree',
  'flower': 'fluent-emoji-flat:blossom',
  'pizza': 'fluent-emoji-flat:pizza',
  'coffee': 'fluent-emoji-flat:hot-beverage',
  'cake': 'fluent-emoji-flat:birthday-cake',
  'apple': 'fluent-emoji-flat:red-apple',
  'banana': 'fluent-emoji-flat:banana',
  'hamburger': 'fluent-emoji-flat:hamburger',
  'popcorn': 'fluent-emoji-flat:popcorn',
  'icecream': 'fluent-emoji-flat:ice-cream',
  'donut': 'fluent-emoji-flat:doughnut',
  'cookie': 'fluent-emoji-flat:cookie',
  'chocolate': 'fluent-emoji-flat:chocolate-bar',
  'candy': 'fluent-emoji-flat:candy',
  'lollipop': 'fluent-emoji-flat:lollipop',
  'honey': 'fluent-emoji-flat:honey-pot',
  'milk': 'fluent-emoji-flat:glass-of-milk',
  'beer': 'fluent-emoji-flat:beer-mug',
  'wine': 'fluent-emoji-flat:wine-glass',
  'cocktail': 'fluent-emoji-flat:cocktail-glass',
  'soccer': 'fluent-emoji-flat:soccer-ball',
  'basketball': 'fluent-emoji-flat:basketball',
  'football': 'fluent-emoji-flat:american-football',
  'baseball': 'fluent-emoji-flat:baseball',
  'tennis': 'fluent-emoji-flat:tennis',
  'volleyball': 'fluent-emoji-flat:volleyball',
  'bowling': 'fluent-emoji-flat:bowling',
  'golf': 'fluent-emoji-flat:flag-in-hole',
  'fishing': 'fluent-emoji-flat:fishing-pole',
  'running': 'fluent-emoji-flat:person-running',
  'swimming': 'fluent-emoji-flat:person-swimming',
  'cycling': 'fluent-emoji-flat:person-biking',
  'hiking': 'fluent-emoji-flat:person-climbing'
}

const getIconifyName = (iconName: string) => {
  return iconMapping[iconName] || 'fluent-emoji-flat:question-mark'
}

const parseOptions = (optionsJson: string | undefined) => {
  if (!optionsJson) return []
  try {
    return JSON.parse(optionsJson)
  } catch {
    return []
  }
}

const loadTemplate = async () => {
  try {
    loading.value = true
    template.value = await getTemplateById(templateId)
  } catch (err) {
    console.error('Error loading template:', err)
    error.value = 'Nie udało się załadować szablonu'
  } finally {
    loading.value = false
  }
}

const submitRequest = async () => {
  if (!template.value) return

  try {
    submitting.value = true
    
    const result = await submitRequestApi({
      requestTemplateId: template.value.id,
      priority: priority.value,
      formData: formData.value
    })

    alert(`Wniosek ${result.requestNumber} został złożony pomyślnie!`)
    navigateTo('/dashboard/requests')
  } catch (err) {
    console.error('Error submitting request:', err)
    alert('Błąd podczas składania wniosku')
  } finally {
    submitting.value = false
  }
}

onMounted(() => {
  loadTemplate()
})
</script>


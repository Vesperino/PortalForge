<script setup lang="ts">
definePageMeta({
  layout: 'default',
  middleware: ['auth']
})

interface Document {
  id: number
  name: string
  type: string
  size: string
  uploadedBy: string
  uploadedAt: Date
  category: string
}

const documents = ref<Document[]>([
  {
    id: 1,
    name: 'Regulamin pracy.pdf',
    type: 'PDF',
    size: '2.5 MB',
    uploadedBy: 'HR Team',
    uploadedAt: new Date('2024-01-10'),
    category: 'Regulaminy'
  },
  {
    id: 2,
    name: 'Polityka prywatności.pdf',
    type: 'PDF',
    size: '1.8 MB',
    uploadedBy: 'Legal Team',
    uploadedAt: new Date('2024-01-08'),
    category: 'Polityki'
  },
  {
    id: 3,
    name: 'Wzór umowy.docx',
    type: 'DOCX',
    size: '0.5 MB',
    uploadedBy: 'HR Team',
    uploadedAt: new Date('2024-01-05'),
    category: 'Wzory'
  }
])

const selectedCategory = ref<string>('Wszystkie')
const searchQuery = ref<string>('')

const categories = ref<string[]>([
  'Wszystkie',
  'Regulaminy',
  'Polityki',
  'Wzory',
  'Instrukcje'
])

const filteredDocuments = computed(() => {
  let filtered = documents.value

  if (selectedCategory.value !== 'Wszystkie') {
    filtered = filtered.filter(doc => doc.category === selectedCategory.value)
  }

  if (searchQuery.value) {
    filtered = filtered.filter(doc =>
      doc.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    )
  }

  return filtered
})

const formatDate = (date: Date) => {
  return new Intl.DateTimeFormat('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  }).format(date)
}

const getFileIcon = (type: string) => {
  switch (type) {
    case 'PDF':
      return 'M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z'
    case 'DOCX':
      return 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z'
    default:
      return 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z'
  }
}

const downloadDocument = (document: Document) => {
  // TODO: Implement download functionality
  console.log('Downloading:', document.name)
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
        Dokumenty
      </h1>
      <BaseButton variant="primary">
        Dodaj dokument
      </BaseButton>
    </div>

    <!-- Filters -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <!-- Search -->
        <div>
          <label for="search" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Szukaj
          </label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
            <input
              id="search"
              v-model="searchQuery"
              type="text"
              placeholder="Szukaj dokumentów..."
              class="pl-10 w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            >
          </div>
        </div>

        <!-- Category Filter -->
        <div>
          <label for="category" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Kategoria
          </label>
          <select
            id="category"
            v-model="selectedCategory"
            class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
          >
            <option v-for="category in categories" :key="category" :value="category">
              {{ category }}
            </option>
          </select>
        </div>
      </div>
    </div>

    <!-- Documents List -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden">
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
          <thead class="bg-gray-50 dark:bg-gray-700">
            <tr>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Nazwa
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Kategoria
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Rozmiar
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Dodane przez
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Data
              </th>
              <th scope="col" class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Akcje
              </th>
            </tr>
          </thead>
          <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
            <tr
              v-for="document in filteredDocuments"
              :key="document.id"
              class="hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
            >
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex items-center gap-3">
                  <svg
                    class="w-8 h-8 text-blue-500"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      :d="getFileIcon(document.type)"
                    />
                  </svg>
                  <div>
                    <p class="text-sm font-medium text-gray-900 dark:text-white">
                      {{ document.name }}
                    </p>
                    <p class="text-xs text-gray-500 dark:text-gray-400">
                      {{ document.type }}
                    </p>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span class="px-2 py-1 text-xs font-medium rounded-full bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200">
                  {{ document.category }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-700 dark:text-gray-300">
                {{ document.size }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-700 dark:text-gray-300">
                {{ document.uploadedBy }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-700 dark:text-gray-300">
                {{ formatDate(document.uploadedAt) }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                <button
                  type="button"
                  class="text-blue-600 dark:text-blue-400 hover:text-blue-900 dark:hover:text-blue-300 focus:outline-none focus:ring-2 focus:ring-blue-500 rounded px-2 py-1"
                  @click="downloadDocument(document)"
                >
                  Pobierz
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Empty State -->
      <div
        v-if="filteredDocuments.length === 0"
        class="text-center py-12"
      >
        <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
        <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">
          Brak dokumentów
        </h3>
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
          Nie znaleziono dokumentów spełniających kryteria wyszukiwania.
        </p>
      </div>
    </div>
  </div>
</template>

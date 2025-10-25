<script setup lang="ts">
definePageMeta({
  layout: 'default'
  // middleware: ['auth'] // Disabled for testing
})

const { getDocuments, getDocumentsByCategory, searchDocuments } = useMockData()

const selectedCategory = ref<string>('all')
const searchQuery = ref<string>('')

const allDocuments = getDocuments()

const categories = [
  { value: 'all', label: 'Wszystkie' },
  { value: 'policy', label: 'Polityki' },
  { value: 'procedure', label: 'Procedury' },
  { value: 'template', label: 'Szablony' },
  { value: 'report', label: 'Raporty' },
  { value: 'presentation', label: 'Prezentacje' },
  { value: 'manual', label: 'Instrukcje' }
]

const filteredDocuments = computed(() => {
  let filtered = selectedCategory.value === 'all'
    ? allDocuments
    : getDocumentsByCategory(selectedCategory.value)

  if (searchQuery.value) {
    filtered = searchDocuments(searchQuery.value)
    if (selectedCategory.value !== 'all') {
      filtered = filtered.filter(doc => doc.category === selectedCategory.value)
    }
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

const formatFileSize = (bytes: number) => {
  if (bytes < 1024) return bytes + ' B'
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB'
  return (bytes / (1024 * 1024)).toFixed(1) + ' MB'
}

const getFileIcon = (type: string) => {
  const icons: Record<string, string> = {
    'pdf': 'M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z',
    'docx': 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z',
    'xlsx': 'M3 10h18M3 14h18m-9-4v8m-7 0h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z',
    'pptx': 'M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z',
    'txt': 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z'
  }
  return icons[type] || icons['txt']
}

const getFileIconColor = (type: string) => {
  const colors: Record<string, string> = {
    'pdf': 'text-red-500',
    'docx': 'text-blue-500',
    'xlsx': 'text-green-500',
    'pptx': 'text-orange-500',
    'txt': 'text-gray-500'
  }
  return colors[type] || 'text-gray-500'
}

const getCategoryColor = (category: string) => {
  const colors: Record<string, string> = {
    'policy': 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
    'procedure': 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
    'template': 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
    'report': 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200',
    'presentation': 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200',
    'manual': 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200'
  }
  return colors[category] || 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200'
}

const getCategoryLabel = (category: string) => {
  const labels: Record<string, string> = {
    'policy': 'Polityka',
    'procedure': 'Procedura',
    'template': 'Szablon',
    'report': 'Raport',
    'presentation': 'Prezentacja',
    'manual': 'Instrukcja'
  }
  return labels[category] || category
}

const downloadDocument = (document: any) => {
  // TODO: Implement actual download functionality
  console.log('Downloading:', document.name)
  // In real implementation, this would trigger a file download
  alert(`Pobieranie: ${document.name}`)
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
            <option v-for="cat in categories" :key="cat.value" :value="cat.value">
              {{ cat.label }}
            </option>
          </select>
        </div>
      </div>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4">
        <p class="text-sm text-gray-600 dark:text-gray-400">Wszystkie dokumenty</p>
        <p class="text-2xl font-bold text-gray-900 dark:text-white">{{ allDocuments.length }}</p>
      </div>
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4">
        <p class="text-sm text-gray-600 dark:text-gray-400">Wyświetlane</p>
        <p class="text-2xl font-bold text-gray-900 dark:text-white">{{ filteredDocuments.length }}</p>
      </div>
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4">
        <p class="text-sm text-gray-600 dark:text-gray-400">Ten miesiąc</p>
        <p class="text-2xl font-bold text-gray-900 dark:text-white">
          {{ allDocuments.filter(d => d.uploadedAt.getMonth() === new Date().getMonth()).length }}
        </p>
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
                    :class="getFileIconColor(document.fileType)"
                    class="w-8 h-8"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      :d="getFileIcon(document.fileType)"
                    />
                  </svg>
                  <div>
                    <p class="text-sm font-medium text-gray-900 dark:text-white">
                      {{ document.name }}
                    </p>
                    <p class="text-xs text-gray-500 dark:text-gray-400">
                      {{ document.fileType.toUpperCase() }}
                    </p>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span
                  :class="getCategoryColor(document.category)"
                  class="px-2 py-1 text-xs font-medium rounded-full"
                >
                  {{ getCategoryLabel(document.category) }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-700 dark:text-gray-300">
                {{ formatFileSize(document.size) }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex items-center gap-2">
                  <div class="w-8 h-8 rounded-full bg-blue-500 flex items-center justify-center text-white font-semibold text-xs">
                    {{ document.uploader?.firstName?.[0] }}{{ document.uploader?.lastName?.[0] }}
                  </div>
                  <span class="text-sm text-gray-700 dark:text-gray-300">
                    {{ document.uploader?.firstName }} {{ document.uploader?.lastName }}
                  </span>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-700 dark:text-gray-300">
                {{ formatDate(document.uploadedAt) }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                <button
                  type="button"
                  class="text-blue-600 dark:text-blue-400 hover:text-blue-900 dark:hover:text-blue-300 focus:outline-none focus:ring-2 focus:ring-blue-500 rounded px-2 py-1 transition-colors"
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

<template>
  <div class="fixed inset-0 z-[10002] overflow-y-auto" @click.self="$emit('close')">
    <div class="flex items-center justify-center min-h-screen px-4 pt-4 pb-20 text-center sm:block sm:p-0">
      <!-- Background overlay -->
      <div class="fixed inset-0 transition-opacity bg-gray-500 bg-opacity-75 dark:bg-gray-900 dark:bg-opacity-75" @click="$emit('close')"></div>

      <!-- Modal panel -->
      <div class="inline-block align-bottom bg-white dark:bg-gray-800 rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
        <!-- Header -->
        <div class="bg-gray-50 dark:bg-gray-700 px-6 py-4 border-b border-gray-200 dark:border-gray-600">
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-medium text-gray-900 dark:text-white">
              Zarządzaj Kategoriami
            </h3>
            <button
              type="button"
              @click="$emit('close')"
              class="text-gray-400 hover:text-gray-500 dark:hover:text-gray-300"
            >
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>

        <!-- Body -->
        <div class="px-6 py-4">
          <!-- Add New Category Form -->
          <form @submit.prevent="handleAddCategory" class="mb-6">
            <div class="flex gap-2">
              <input
                v-model="newCategoryName"
                type="text"
                placeholder="Nazwa nowej kategorii..."
                required
                class="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
              <button
                type="submit"
                class="px-4 py-2 bg-blue-600 dark:bg-blue-500 text-white rounded-lg hover:bg-blue-700 dark:hover:bg-blue-600"
              >
                Dodaj
              </button>
            </div>
          </form>

          <!-- Categories List -->
          <div class="space-y-2">
            <div v-if="categories.length === 0" class="text-center py-8 text-gray-500 dark:text-gray-400">
              Brak kategorii
            </div>
            <div
              v-for="category in categories"
              :key="category.id"
              class="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-700 rounded-lg"
            >
              <div class="flex items-center gap-2">
                <span v-if="category.icon" class="text-xl">{{ category.icon }}</span>
                <span class="text-sm font-medium text-gray-900 dark:text-white">{{ category.name }}</span>
                <span class="text-xs text-gray-500 dark:text-gray-400">({{ category.services?.length || 0 }} serwisów)</span>
              </div>
              <button
                @click="handleDeleteCategory(category.id)"
                class="text-red-600 dark:text-red-400 hover:text-red-800 dark:hover:text-red-300 text-sm"
              >
                Usuń
              </button>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="bg-gray-50 dark:bg-gray-700 px-6 py-4 border-t border-gray-200 dark:border-gray-600 flex justify-end">
          <button
            type="button"
            @click="$emit('close')"
            class="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-600 border border-gray-300 dark:border-gray-500 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-500"
          >
            Zamknij
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { InternalServiceCategory } from '~/types/internal-services'

const props = defineProps<{
  categories: InternalServiceCategory[]
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'saved'): void
}>()

const { createCategory, deleteCategory } = useInternalServicesApi()
const toast = useNotificationToast()
const confirmModal = useConfirmModal()

const newCategoryName = ref('')

async function handleAddCategory() {
  if (!newCategoryName.value.trim()) return

  try {
    await createCategory({
      name: newCategoryName.value,
      description: '',
      displayOrder: props.categories.length
    })
    newCategoryName.value = ''
    toast.success('Kategoria została dodana')
    emit('saved')
  } catch (err: any) {
    toast.error('Nie udało się dodać kategorii', err.message)
  }
}

async function handleDeleteCategory(id: string) {
  const confirmed = await confirmModal.confirmDelete('tę kategorię', 'Wszystkie serwisy w tej kategorii zostaną przeniesione do kategorii bez przypisania.')

  if (confirmed) {
    try {
      await deleteCategory(id)
      toast.success('Kategoria została usunięta')
      emit('saved')
    } catch (err: any) {
      toast.error('Nie udało się usunąć kategorii', err.message)
    }
  }
}
</script>

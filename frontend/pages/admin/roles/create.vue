<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Header -->
      <div class="mb-8">
        <NuxtLink
          to="/admin/roles"
          class="inline-flex items-center text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300 mb-4"
        >
          <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
          Powrót do grup ról
        </NuxtLink>
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Utwórz Nową Grupę Ról</h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">Dodaj nową grupę ról i przypisz uprawnienia</p>
      </div>

      <!-- Error Message -->
      <div v-if="errorMessage" class="mb-6 bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-800 rounded-lg p-4">
        <p class="text-red-800 dark:text-red-300">{{ errorMessage }}</p>
      </div>

      <!-- Form -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
        <form @submit.prevent="handleSubmit">
          <!-- Name -->
          <div class="mb-6">
            <label for="name" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Nazwa grupy ról *
            </label>
            <input
              id="name"
              v-model="formData.name"
              type="text"
              required
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="np. Moderatorzy"
            >
          </div>

          <!-- Description -->
          <div class="mb-6">
            <label for="description" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Opis *
            </label>
            <textarea
              id="description"
              v-model="formData.description"
              required
              rows="3"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="Opisz rolę i zakres odpowiedzialności..."
            />
          </div>

          <!-- Permissions -->
          <div class="mb-6">
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-4">
              Uprawnienia *
            </label>

            <!-- Loading State -->
            <div v-if="permissionsStore.loading" class="text-center py-8">
              <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 dark:border-blue-400"/>
              <p class="mt-2 text-gray-600 dark:text-gray-400">Ładowanie uprawnień...</p>
            </div>

            <!-- Error State -->
            <div v-else-if="permissionsStore.error" class="bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-800 rounded-lg p-4">
              <p class="text-red-800 dark:text-red-300">{{ permissionsStore.error }}</p>
            </div>

            <!-- Permissions by Category -->
            <div v-else class="space-y-4">
              <div
                v-for="(permissions, category) in permissionsStore.permissionsByCategory"
                :key="category"
                class="border border-gray-200 dark:border-gray-700 rounded-lg p-4"
              >
                <div class="flex items-center justify-between mb-3">
                  <h3 class="text-lg font-semibold text-gray-900 dark:text-white">{{ category }}</h3>
                  <button
                    type="button"
                    class="text-sm text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300"
                    @click="toggleCategory(category)"
                  >
                    {{ isCategorySelected(category) ? 'Odznacz wszystkie' : 'Zaznacz wszystkie' }}
                  </button>
                </div>
                <div class="space-y-2">
                  <label
                    v-for="permission in permissions"
                    :key="permission.id"
                    class="flex items-start cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700/50 p-2 rounded"
                  >
                    <input
                      v-model="formData.permissionIds"
                      type="checkbox"
                      :value="permission.id"
                      class="mt-1 h-4 w-4 text-blue-600 dark:text-blue-400 border-gray-300 dark:border-gray-600 rounded focus:ring-blue-500 dark:focus:ring-blue-400"
                    >
                    <div class="ml-3">
                      <span class="text-sm font-medium text-gray-900 dark:text-white">{{ permission.name }}</span>
                      <p class="text-xs text-gray-600 dark:text-gray-400">{{ permission.description }}</p>
                    </div>
                  </label>
                </div>
              </div>
            </div>

            <p v-if="formData.permissionIds.length > 0" class="mt-2 text-sm text-gray-600 dark:text-gray-400">
              Wybrano {{ formData.permissionIds.length }} {{ formData.permissionIds.length === 1 ? 'uprawnienie' : 'uprawnień' }}
            </p>
          </div>

          <!-- Actions -->
          <div class="flex items-center justify-end space-x-4 pt-6 border-t border-gray-200 dark:border-gray-700">
            <NuxtLink
              to="/admin/roles"
              class="px-6 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
            >
              Anuluj
            </NuxtLink>
            <button
              type="submit"
              :disabled="isSubmitting || formData.permissionIds.length === 0"
              class="px-6 py-2 bg-blue-600 dark:bg-blue-500 text-white rounded-lg hover:bg-blue-700 dark:hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              {{ isSubmitting ? 'Tworzenie...' : 'Utwórz Grupę Ról' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  middleware: ['auth', 'verified', 'admin'],
  layout: 'default',
})

useHead({
  title: 'Utwórz Grupę Ról - Panel Administracyjny',
})

const router = useRouter()
const permissionsStore = usePermissionsStore()
const { createRoleGroup } = useRoleGroupApi()

const formData = ref({
  name: '',
  description: '',
  permissionIds: [] as string[]
})

const isSubmitting = ref(false)
const errorMessage = ref<string | null>(null)

// Fetch permissions on mount
onMounted(() => {
  permissionsStore.fetchPermissions()
})

// Check if all permissions in a category are selected
function isCategorySelected(category: string): boolean {
  const categoryPermissions = permissionsStore.permissionsByCategory[category] || []
  return categoryPermissions.every(p => formData.value.permissionIds.includes(p.id))
}

// Toggle all permissions in a category
function toggleCategory(category: string): void {
  const categoryPermissions = permissionsStore.permissionsByCategory[category] || []
  const allSelected = isCategorySelected(category)

  if (allSelected) {
    // Remove all permissions from this category
    formData.value.permissionIds = formData.value.permissionIds.filter(
      id => !categoryPermissions.some(p => p.id === id)
    )
  } else {
    // Add all permissions from this category
    const categoryIds = categoryPermissions.map(p => p.id)
    formData.value.permissionIds = [
      ...formData.value.permissionIds,
      ...categoryIds.filter(id => !formData.value.permissionIds.includes(id))
    ]
  }
}

async function handleSubmit(): Promise<void> {
  if (formData.value.permissionIds.length === 0) {
    errorMessage.value = 'Musisz wybrać przynajmniej jedno uprawnienie'
    return
  }

  isSubmitting.value = true
  errorMessage.value = null

  try {
    await createRoleGroup({
      name: formData.value.name,
      description: formData.value.description,
      permissionIds: formData.value.permissionIds
    })

    // Redirect to roles list
    await router.push('/admin/roles')
  } catch (error: any) {
    console.error('Error creating role group:', error)
    errorMessage.value = error?.data?.message || 'Nie udało się utworzyć grupy ról'
  } finally {
    isSubmitting.value = false
  }
}
</script>


<script setup lang="ts">
import type { OrganizationEmployee } from '~/composables/useOrganization'

interface Props {
  employee: OrganizationEmployee | null
  visible: boolean
}

defineProps<Props>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  selectEmployee: [employee: OrganizationEmployee]
}>()

const { getInitials } = useOrganization()

const closeModal = () => {
  emit('update:visible', false)
}

const handleSelectEmployee = (employee: OrganizationEmployee) => {
  emit('selectEmployee', employee)
}

const handleOverlayClick = (event: MouseEvent) => {
  if (event.target === event.currentTarget) {
    closeModal()
  }
}
</script>

<template>
  <Teleport to="body">
    <div
      v-if="visible && employee"
      class="fixed inset-0 z-50 overflow-y-auto"
      @click.self="handleOverlayClick"
    >
      <div class="flex items-center justify-center min-h-screen px-4 pt-4 pb-20 text-center sm:block sm:p-0">
        <div
          class="fixed inset-0 transition-opacity bg-gray-500 bg-opacity-75 dark:bg-gray-900 dark:bg-opacity-75"
          @click="closeModal"
        />

        <div class="inline-block align-bottom bg-white dark:bg-gray-800 rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
          <div class="bg-gradient-to-r from-blue-500 to-blue-600 px-6 py-4">
            <div class="flex items-center justify-between">
              <h3 class="text-lg font-semibold text-white">
                Szczegoly pracownika
              </h3>
              <button
                type="button"
                class="text-white hover:text-gray-200 focus:outline-none"
                data-testid="close-employee-modal"
                @click="closeModal"
              >
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
          </div>

          <div class="px-6 py-6 space-y-6">
            <div class="text-center">
              <div class="inline-flex w-24 h-24 rounded-full bg-gradient-to-br from-blue-500 to-blue-600 items-center justify-center text-3xl font-bold text-white shadow-lg overflow-hidden">
                <img
                  v-if="employee.profilePhotoUrl"
                  :src="employee.profilePhotoUrl"
                  :alt="`${employee.firstName} ${employee.lastName}`"
                  class="w-full h-full object-cover"
                >
                <span v-else>
                  {{ getInitials(employee) }}
                </span>
              </div>
              <h4 class="mt-4 text-2xl font-bold text-gray-900 dark:text-white">
                {{ employee.firstName }} {{ employee.lastName }}
              </h4>
              <p class="mt-1 text-gray-600 dark:text-gray-400">
                {{ employee.position }}
              </p>
              <span
                class="inline-block mt-3 px-4 py-1.5 text-sm font-medium rounded-full text-white bg-blue-500 shadow-sm"
              >
                {{ employee.department }}
              </span>
            </div>

            <div class="space-y-4 pt-4 border-t border-gray-200 dark:border-gray-700">
              <div class="flex items-center gap-4">
                <div class="flex-shrink-0 w-10 h-10 rounded-full bg-blue-100 dark:bg-blue-900 flex items-center justify-center">
                  <svg class="w-5 h-5 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                  </svg>
                </div>
                <div class="flex-1 min-w-0">
                  <p class="text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Email</p>
                  <a
                    :href="`mailto:${employee.email}`"
                    class="text-sm text-blue-600 dark:text-blue-400 hover:underline truncate block font-medium"
                  >
                    {{ employee.email }}
                  </a>
                </div>
              </div>

              <div v-if="employee.phoneNumber" class="flex items-center gap-4">
                <div class="flex-shrink-0 w-10 h-10 rounded-full bg-green-100 dark:bg-green-900 flex items-center justify-center">
                  <svg class="w-5 h-5 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
                  </svg>
                </div>
                <div class="flex-1 min-w-0">
                  <p class="text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Telefon</p>
                  <a
                    :href="`tel:${employee.phoneNumber}`"
                    class="text-sm text-green-600 dark:text-green-400 hover:underline font-medium"
                  >
                    {{ employee.phoneNumber }}
                  </a>
                </div>
              </div>
            </div>

            <div v-if="employee.supervisor" class="pt-4 border-t border-gray-200 dark:border-gray-700">
              <p class="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase mb-3">
                Przelozony
              </p>
              <div
                class="flex items-center gap-3 p-3 rounded-lg bg-gray-50 dark:bg-gray-700/50 hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer transition-colors"
                data-testid="supervisor-item"
                @click="handleSelectEmployee(employee.supervisor)"
              >
                <div class="w-12 h-12 rounded-full bg-gradient-to-br from-gray-400 to-gray-500 flex items-center justify-center text-white font-semibold">
                  {{ getInitials(employee.supervisor) }}
                </div>
                <div class="flex-1">
                  <p class="text-sm font-semibold text-gray-900 dark:text-white">
                    {{ employee.supervisor.firstName }} {{ employee.supervisor.lastName }}
                  </p>
                  <p class="text-xs text-gray-600 dark:text-gray-400">
                    {{ employee.supervisor.position }}
                  </p>
                </div>
                <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                </svg>
              </div>
            </div>

            <div v-if="employee.subordinates && employee.subordinates.length > 0" class="pt-4 border-t border-gray-200 dark:border-gray-700">
              <p class="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase mb-3">
                Zarzadza zespolem
              </p>
              <div class="flex items-center gap-2 mb-4">
                <div class="text-3xl font-bold text-gray-900 dark:text-white">
                  {{ employee.subordinates.length }}
                </div>
                <span class="text-sm text-gray-600 dark:text-gray-400">
                  {{ employee.subordinates.length === 1 ? 'osoba' : employee.subordinates.length < 5 ? 'osoby' : 'osob' }}
                </span>
              </div>
              <div class="space-y-2 max-h-48 overflow-y-auto">
                <div
                  v-for="sub in employee.subordinates"
                  :key="sub.id"
                  class="flex items-center gap-3 p-2 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700/50 cursor-pointer transition-colors"
                  data-testid="subordinate-item"
                  @click="handleSelectEmployee(sub)"
                >
                  <div class="w-10 h-10 rounded-full bg-gradient-to-br from-gray-300 to-gray-400 dark:from-gray-600 dark:to-gray-700 flex items-center justify-center text-gray-700 dark:text-gray-300 font-semibold text-sm">
                    {{ getInitials(sub) }}
                  </div>
                  <div class="flex-1">
                    <p class="text-sm font-medium text-gray-900 dark:text-white">
                      {{ sub.firstName }} {{ sub.lastName }}
                    </p>
                    <p class="text-xs text-gray-600 dark:text-gray-400">
                      {{ sub.position }}
                    </p>
                  </div>
                  <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                  </svg>
                </div>
              </div>
            </div>
          </div>

          <div class="bg-gray-50 dark:bg-gray-700/50 px-6 py-4">
            <button
              type="button"
              class="w-full px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors"
              data-testid="close-modal-btn"
              @click="closeModal"
            >
              Zamknij
            </button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

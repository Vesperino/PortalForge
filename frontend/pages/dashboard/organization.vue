<script setup lang="ts">
definePageMeta({
  layout: 'default',
  middleware: ['auth']
})

interface Employee {
  id: number
  name: string
  position: string
  department: string
  email: string
  phone: string
  subordinates?: Employee[]
}

const organizationData = ref<Employee>({
  id: 1,
  name: 'Anna Nowak',
  position: 'CEO',
  department: 'Zarząd',
  email: 'anna.nowak@company.com',
  phone: '+48 100 000 000',
  subordinates: [
    {
      id: 2,
      name: 'Piotr Wiśniewski',
      position: 'CTO',
      department: 'IT',
      email: 'piotr.wisniewski@company.com',
      phone: '+48 100 000 001',
      subordinates: [
        {
          id: 3,
          name: 'Jan Kowalski',
          position: 'Senior Developer',
          department: 'IT',
          email: 'jan.kowalski@company.com',
          phone: '+48 100 000 002'
        },
        {
          id: 4,
          name: 'Maria Kowalczyk',
          position: 'Junior Developer',
          department: 'IT',
          email: 'maria.kowalczyk@company.com',
          phone: '+48 100 000 003'
        }
      ]
    },
    {
      id: 5,
      name: 'Krzysztof Lewandowski',
      position: 'HR Manager',
      department: 'HR',
      email: 'krzysztof.lewandowski@company.com',
      phone: '+48 100 000 004',
      subordinates: [
        {
          id: 6,
          name: 'Agnieszka Wójcik',
          position: 'HR Specialist',
          department: 'HR',
          email: 'agnieszka.wojcik@company.com',
          phone: '+48 100 000 005'
        }
      ]
    }
  ]
})

const selectedEmployee = ref<Employee | null>(null)

const selectEmployee = (employee: Employee) => {
  selectedEmployee.value = employee
}

const closeDetails = () => {
  selectedEmployee.value = null
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
        Struktura organizacyjna
      </h1>
      <BaseButton variant="primary">
        Eksportuj do PDF
      </BaseButton>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <!-- Organization Tree -->
      <div class="lg:col-span-2 bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
        <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-6">
          Schemat organizacyjny
        </h2>

        <!-- CEO Level -->
        <div class="flex flex-col items-center">
          <div
            class="bg-blue-50 dark:bg-blue-900/20 border-2 border-blue-500 rounded-lg p-4 cursor-pointer hover:shadow-lg transition-shadow max-w-xs w-full"
            @click="selectEmployee(organizationData)"
          >
            <h3 class="font-semibold text-gray-900 dark:text-white">
              {{ organizationData.name }}
            </h3>
            <p class="text-sm text-gray-600 dark:text-gray-400">
              {{ organizationData.position }}
            </p>
            <p class="text-xs text-gray-500 dark:text-gray-500 mt-1">
              {{ organizationData.department }}
            </p>
          </div>

          <!-- Direct Reports -->
          <div v-if="organizationData.subordinates" class="mt-8 w-full">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div
                v-for="manager in organizationData.subordinates"
                :key="manager.id"
                class="flex flex-col items-center"
              >
                <!-- Manager -->
                <div
                  class="bg-green-50 dark:bg-green-900/20 border-2 border-green-500 rounded-lg p-4 cursor-pointer hover:shadow-lg transition-shadow w-full"
                  @click="selectEmployee(manager)"
                >
                  <h3 class="font-semibold text-gray-900 dark:text-white">
                    {{ manager.name }}
                  </h3>
                  <p class="text-sm text-gray-600 dark:text-gray-400">
                    {{ manager.position }}
                  </p>
                  <p class="text-xs text-gray-500 dark:text-gray-500 mt-1">
                    {{ manager.department }}
                  </p>
                </div>

                <!-- Team Members -->
                <div v-if="manager.subordinates" class="mt-6 w-full space-y-3">
                  <div
                    v-for="employee in manager.subordinates"
                    :key="employee.id"
                    class="bg-gray-50 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-lg p-3 cursor-pointer hover:shadow-md transition-shadow"
                    @click="selectEmployee(employee)"
                  >
                    <h4 class="font-medium text-gray-900 dark:text-white text-sm">
                      {{ employee.name }}
                    </h4>
                    <p class="text-xs text-gray-600 dark:text-gray-400">
                      {{ employee.position }}
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Employee Details Sidebar -->
      <div class="lg:col-span-1">
        <div
          v-if="selectedEmployee"
          class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 sticky top-6"
        >
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
              Szczegóły pracownika
            </h3>
            <button
              type="button"
              class="text-gray-500 hover:text-gray-700 dark:hover:text-gray-300 focus:outline-none"
              @click="closeDetails"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          <div class="space-y-4">
            <!-- Avatar -->
            <div class="flex justify-center">
              <div class="w-24 h-24 rounded-full bg-blue-500 flex items-center justify-center text-2xl font-bold text-white">
                {{ selectedEmployee.name.split(' ').map(n => n[0]).join('') }}
              </div>
            </div>

            <!-- Name & Position -->
            <div class="text-center">
              <h4 class="text-xl font-bold text-gray-900 dark:text-white">
                {{ selectedEmployee.name }}
              </h4>
              <p class="text-gray-600 dark:text-gray-400">
                {{ selectedEmployee.position }}
              </p>
              <span class="inline-block mt-2 px-3 py-1 text-xs font-medium rounded-full bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200">
                {{ selectedEmployee.department }}
              </span>
            </div>

            <!-- Contact Info -->
            <div class="space-y-3 pt-4 border-t border-gray-200 dark:border-gray-700">
              <div class="flex items-start gap-3">
                <svg class="w-5 h-5 text-gray-500 dark:text-gray-400 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                </svg>
                <div class="flex-1 min-w-0">
                  <p class="text-xs text-gray-500 dark:text-gray-400">Email</p>
                  <a
                    :href="`mailto:${selectedEmployee.email}`"
                    class="text-sm text-blue-600 dark:text-blue-400 hover:underline truncate block"
                  >
                    {{ selectedEmployee.email }}
                  </a>
                </div>
              </div>

              <div class="flex items-start gap-3">
                <svg class="w-5 h-5 text-gray-500 dark:text-gray-400 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
                </svg>
                <div class="flex-1 min-w-0">
                  <p class="text-xs text-gray-500 dark:text-gray-400">Telefon</p>
                  <a
                    :href="`tel:${selectedEmployee.phone}`"
                    class="text-sm text-blue-600 dark:text-blue-400 hover:underline"
                  >
                    {{ selectedEmployee.phone }}
                  </a>
                </div>
              </div>
            </div>

            <!-- Team Info -->
            <div v-if="selectedEmployee.subordinates" class="pt-4 border-t border-gray-200 dark:border-gray-700">
              <p class="text-xs text-gray-500 dark:text-gray-400 mb-2">
                Zarządza zespołem
              </p>
              <p class="text-2xl font-bold text-gray-900 dark:text-white">
                {{ selectedEmployee.subordinates.length }}
                <span class="text-sm font-normal text-gray-600 dark:text-gray-400">
                  {{ selectedEmployee.subordinates.length === 1 ? 'osoba' : 'osób' }}
                </span>
              </p>
            </div>
          </div>
        </div>

        <!-- Empty State -->
        <div
          v-else
          class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 text-center"
        >
          <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">
            Wybierz pracownika
          </h3>
          <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
            Kliknij na kartę pracownika, aby zobaczyć szczegóły.
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

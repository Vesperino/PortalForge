<script setup lang="ts">
import { ref } from 'vue'
import VacationAnalyticsDashboard from '~/components/vacation/VacationAnalyticsDashboard.vue'

definePageMeta({
  middleware: ['auth', 'verified'],
  layout: 'default'
})

useHead({
  title: 'Analityka urlopowa - PortalForge'
})

const authStore = useAuthStore()
</script>

<template>
  <div class="vacation-analytics-page">
    <div class="max-w-7xl mx-auto p-6">
      <!-- Page Header -->
      <div class="mb-6">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">
          Analityka urlopowa
        </h1>
        <p class="mt-1 text-sm text-gray-600 dark:text-gray-400">
          Szczegółowe statystyki i analizy Twoich wniosków urlopowych
        </p>
      </div>

      <!-- Analytics Dashboard -->
      <VacationAnalyticsDashboard :user-id="authStore.user?.id" />
    </div>
  </div>
</template>

<style scoped>
.vacation-analytics-page {
  min-height: calc(100vh - 4rem);
  background-color: #f9fafb;
}

.dark .vacation-analytics-page {
  background-color: #111827;
}
</style>
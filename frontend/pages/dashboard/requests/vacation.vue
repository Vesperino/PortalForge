<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import EnhancedVacationRequest from '~/components/vacation/EnhancedVacationRequest.vue'
import { useRequestsApi } from '~/composables/useRequestsApi'

definePageMeta({
  middleware: ['auth', 'verified'],
  layout: 'default'
})

useHead({
  title: 'Wniosek urlopowy - PortalForge'
})

const router = useRouter()
const { submitRequest } = useRequestsApi()
const toast = useNotificationToast()

const isSubmitting = ref(false)

const handleSubmit = async (formData: Record<string, any>) => {
  isSubmitting.value = true
  
  try {
    // Find vacation request template ID (this would typically be fetched from API)
    const vacationTemplateId = 'vacation-template-id' // This should be dynamic
    
    const requestData = {
      requestTemplateId: vacationTemplateId,
      priority: 'Standard' as const,
      formData: formData
    }
    
    const result = await submitRequest(requestData)
    
    toast.success(`Wniosek urlopowy został złożony. Numer: ${result.requestNumber}`)
    router.push('/dashboard/requests')
  } catch (error) {
    console.error('Error submitting vacation request:', error)
    toast.error('Błąd podczas składania wniosku urlopowego')
  } finally {
    isSubmitting.value = false
  }
}

const handleCancel = () => {
  router.push('/dashboard/requests')
}
</script>

<template>
  <div class="vacation-request-page">
    <div class="max-w-4xl mx-auto p-6">
      <!-- Page Header -->
      <div class="mb-6">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">
          Nowy wniosek urlopowy
        </h1>
        <p class="mt-1 text-sm text-gray-600 dark:text-gray-400">
          Złóż wniosek urlopowy z pełną analizą dostępności i konfliktów
        </p>
      </div>

      <!-- Enhanced Vacation Request Component -->
      <EnhancedVacationRequest 
        @submit="handleSubmit"
        @cancel="handleCancel"
      />
    </div>
  </div>
</template>

<style scoped>
.vacation-request-page {
  min-height: calc(100vh - 4rem);
  background-color: #f9fafb;
}

.dark .vacation-request-page {
  background-color: #111827;
}
</style>
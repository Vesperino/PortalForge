<script setup lang="ts">
definePageMeta({
  // middleware: 'verified', // Disabled for testing
})

// Redirect to dashboard home page
navigateTo('/dashboard')
</script>

<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
    <div class="text-center">
      <div class="inline-block animate-spin rounded-full h-12 w-12 border-4 border-blue-500 border-t-transparent" />
      <p class="mt-4 text-gray-600 dark:text-gray-400">
        Przekierowywanie...
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
useHead({
  title: 'PortalForge - Autoryzacja',
  meta: [
    { name: 'description', content: 'Portal wewnętrzny dla organizacji' }
  ]
})
</script>

<template>
  <div class="min-h-screen bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
    <div class="sm:mx-auto sm:w-full sm:max-w-md">
      <div class="flex justify-center">
        <div class="text-center">
          <h1 class="text-4xl font-bold text-blue-600">
            PortalForge
          </h1>
          <p class="mt-2 text-sm text-gray-600">
            Portal wewnętrzny organizacji
          </p>
        </div>
      </div>
    </div>

    <div class="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
      <div class="bg-white py-8 px-4 shadow sm:rounded-lg sm:px-10">
        <slot />
      </div>
    </div>

    <div class="mt-8 text-center text-sm text-gray-600">
      <p>&copy; 2025 PortalForge. Wszelkie prawa zastrzeżone.</p>
    </div>
  </div>
</template>

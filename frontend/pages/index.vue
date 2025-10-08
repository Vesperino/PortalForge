<script setup lang="ts">
const config = useRuntimeConfig()

useHead({
  title: 'Home - PortalForge',
})
</script>

<template>
  <div class="min-h-screen flex flex-col items-center justify-center">
    <div class="max-w-2xl mx-auto p-8 text-center">
      <h1 class="text-4xl font-bold text-primary-600 mb-4">
        Welcome to PortalForge
      </h1>
      <p class="text-lg text-gray-600 mb-8">
        Your internal company portal for organizational management, events, and news.
      </p>

      <div class="bg-white rounded-lg shadow-md p-6">
        <h2 class="text-2xl font-semibold mb-4">Getting Started</h2>
        <p class="text-gray-600 mb-4">
          This is the MVP development version of PortalForge.
        </p>
        <ul class="text-left space-y-2 text-gray-700">
          <li class="flex items-start">
            <span class="text-primary-500 mr-2">✓</span>
            <span>Nuxt 3 initialized with TypeScript</span>
          </li>
          <li class="flex items-start">
            <span class="text-primary-500 mr-2">✓</span>
            <span>Tailwind CSS configured</span>
          </li>
          <li class="flex items-start">
            <span class="text-gray-400 mr-2">○</span>
            <span>Supabase authentication (pending)</span>
          </li>
          <li class="flex items-start">
            <span class="text-gray-400 mr-2">○</span>
            <span>API integration (pending)</span>
          </li>
        </ul>
      </div>

      <div class="mt-8 text-sm text-gray-500">
        <p>API URL: {{ config.public.apiUrl }}</p>
      </div>
    </div>
  </div>
</template>

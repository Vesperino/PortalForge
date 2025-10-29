<script setup lang="ts">
useHead({
  title: 'PortalForge - Autoryzacja',
  meta: [
    { name: 'description', content: 'Portal wewnetrzny dla organizacji' }
  ]
})
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50">
    <slot />
  </div>
</template>

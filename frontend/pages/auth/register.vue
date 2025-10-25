<script setup lang="ts">
definePageMeta({
  layout: 'auth'
  // middleware: ['guest'] // Disabled for testing
})

useHead({
  title: 'Rejestracja - PortalForge',
  meta: [
    { name: 'description', content: 'Utwórz nowe konto w portalu wewnętrznym organizacji' }
  ]
})
</script>

<template>
  <div>
    <AuthRegisterForm />
  </div>
</template>

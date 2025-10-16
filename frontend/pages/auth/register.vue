<script setup lang="ts">
definePageMeta({
  layout: 'auth',
  middleware: ['guest']
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

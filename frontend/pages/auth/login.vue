<script setup lang="ts">
definePageMeta({
  layout: 'auth',
  middleware: ['guest']
})

useHead({
  title: 'Logowanie - PortalForge',
  meta: [
    { name: 'description', content: 'Zaloguj się do portalu wewnętrznego organizacji' }
  ]
})
</script>

<template>
  <div>
    <AuthLoginForm />
  </div>
</template>

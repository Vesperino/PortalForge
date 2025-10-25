<script setup lang="ts">
definePageMeta({
  layout: 'auth'
  // middleware: ['guest'] // Disabled for testing
})

useHead({
  title: 'Resetowanie hasła - PortalForge',
  meta: [
    { name: 'description', content: 'Zresetuj swoje hasło do portalu wewnętrznego organizacji' }
  ]
})
</script>

<template>
  <div>
    <AuthResetPasswordForm />
  </div>
</template>

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2024-11-01',

  devtools: { enabled: true },

  // Disable SSR - build as static SPA for path-based deployment
  ssr: false,

  modules: [
    '@nuxtjs/tailwindcss',
    '@pinia/nuxt',
    '@vueuse/nuxt',
    '@nuxtjs/color-mode',
    '@primevue/nuxt-module',
  ],

  colorMode: {
    classSuffix: '',
    preference: 'system',
    fallback: 'light',
  },

  css: ['~/assets/css/main.css'],

  typescript: {
    strict: true,
    typeCheck: true,
  },

  runtimeConfig: {
    // Private keys (only available server-side)
    // supabaseServiceKey: process.env.SUPABASE_SERVICE_KEY,

    // Public keys (exposed to frontend)
    public: {
      apiUrl: process.env.NUXT_PUBLIC_API_URL || 'http://localhost:5000',
      supabaseUrl: process.env.NUXT_PUBLIC_SUPABASE_URL || '',
      supabaseKey: process.env.NUXT_PUBLIC_SUPABASE_KEY || '',
    }
  },

  app: {
    // Base URL for path-based deployment
    baseURL: '/portalforge/fe/',

    head: {
      title: 'PortalForge',
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { name: 'description', content: 'Internal company portal for organizational management' }
      ],
    }
  },

  // Build configuration
  build: {
    transpile: [],
  },

  vite: {
    build: {
      target: 'esnext',
    },
  },
})

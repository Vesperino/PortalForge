import type { IconifyJSON } from '@iconify/types'
import heroicons from '@iconify-json/heroicons/icons.json'
import fluentEmojiFlat from '@iconify-json/fluent-emoji-flat/icons.json'
import svgSpinners from '@iconify-json/svg-spinners/icons.json'

const createIconSubset = (collection: IconifyJSON, names: string[]): IconifyJSON => {
  const icons = collection.icons || {}
  const aliases = collection.aliases || {}
  const chars = collection.chars || {}

  const subsetIcons: typeof icons = {}
  const subsetAliases: typeof aliases = {}
  const subsetChars: typeof chars = {}
  const visited = new Set<string>()

  const includeIcon = (name: string) => {
    if (visited.has(name)) {
      return
    }
    visited.add(name)

    if (icons[name]) {
      subsetIcons[name] = icons[name]
      if (chars[name]) {
        subsetChars[name] = chars[name]
      }
      return
    }

    const alias = aliases[name]
    if (alias) {
      subsetAliases[name] = { ...alias }
      if (alias.parent) {
        includeIcon(alias.parent)
      }
      if (Array.isArray((alias as any).aliases)) {
        ;((alias as any).aliases as string[]).forEach(includeIcon)
      }
      return
    }

    throw new Error(`Icon "${collection.prefix}:${name}" was not found in the source collection.`)
  }

  names.forEach(includeIcon)

  const result: IconifyJSON = {
    prefix: collection.prefix,
    icons: subsetIcons
  }

  if (Object.keys(subsetAliases).length) {
    result.aliases = subsetAliases
  }

  if (Object.keys(subsetChars).length) {
    result.chars = subsetChars
  }

  if (collection.info) {
    result.info = collection.info
  }
  if (collection.width) {
    result.width = collection.width
  }
  if (collection.height) {
    result.height = collection.height
  }

  return result
}

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
    '@nuxtjs/supabase',
    '@nuxt/icon',
  ],

  supabase: {
    url: process.env.NUXT_PUBLIC_SUPABASE_URL,
    key: process.env.NUXT_PUBLIC_SUPABASE_KEY,
    redirect: false,
  },

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
      googleMapsApiKey: process.env.NUXT_PUBLIC_GOOGLE_MAPS_API_KEY || '',
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

  icon: {
    customCollections: [
      createIconSubset(heroicons as IconifyJSON, [
        'x-mark',
        'information-circle',
        'trash',
        'bell',
        'bell-slash',
        'check-circle',
        'exclamation-circle',
        'check',
        'calendar-days',
        'clipboard-document-list',
        'document-text',
        'shield-check',
        'exclamation-triangle',
        'presentation-chart-line',
        'academic-cap',
        'printer',
        'computer-desktop',
        'cpu-chip',
        'user-group',
        'envelope',
        'envelope-open',
        'wrench-screwdriver',
        'book-open',
        'folder',
        'question-mark-circle'
      ]),
      createIconSubset(fluentEmojiFlat as IconifyJSON, [
        'beach-with-umbrella',
        'airplane'
      ]),
      createIconSubset(svgSpinners as IconifyJSON, ['ring-resize'])
    ]
  },

  // Nitro configuration - disable prerendering since SSR is disabled
  nitro: {
    prerender: {
      crawlLinks: false,
      routes: [],
    },
  },
})

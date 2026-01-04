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
      const aliasWithNested = alias as { aliases?: string[] }
      if (Array.isArray(aliasWithNested.aliases)) {
        aliasWithNested.aliases.forEach(includeIcon)
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

  // Component auto-import configuration
  // Folders where filename includes folder name (e.g., base/BaseButton.vue) use pathPrefix: false
  // Folders where prefix is used in template (e.g., <AdminStructureDepartmentTree>) use pathPrefix: true
  components: {
    dirs: [
      // These folders don't need path prefix (filename includes folder name or is unique)
      { path: '~/components/organization', pathPrefix: false },
      { path: '~/components/requests', pathPrefix: false },
      { path: '~/components/base', pathPrefix: false },
      { path: '~/components/vacation', pathPrefix: false },
      // Root components
      { path: '~/components', pathPrefix: false, ignore: ['**/admin/**', '**/common/**', '**/internal-services/**', '**/organization/**', '**/requests/**', '**/base/**', '**/vacation/**'] },
      // These folders use path prefix in template (e.g., <CommonUserAutocomplete>)
      { path: '~/components/admin', pathPrefix: true },
      { path: '~/components/common', pathPrefix: true },
      { path: '~/components/internal-services', pathPrefix: true },
    ],
  },

  css: ['~/assets/css/main.css'],

  typescript: {
    strict: true,
    typeCheck: false,
  },

  runtimeConfig: {
    // Private keys (only available server-side)
    // supabaseServiceKey: process.env.SUPABASE_SERVICE_KEY,

    // Public keys (exposed to frontend)
    public: {
      apiUrl: process.env.NUXT_PUBLIC_API_URL || 'http://localhost:5155',
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

  postcss: {
    plugins: {
      tailwindcss: {},
      autoprefixer: {},
    },
  },

  vite: {
    build: {
      target: 'es2020',
    },
    optimizeDeps: {
      exclude: ['vue-pdf-embed', 'docx-preview'],
    },
    define: {
      __BUILD_DATE__: JSON.stringify(new Date().toISOString()),
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
        // Urlopy i czas wolny
        'beach-with-umbrella',
        'airplane',
        'palm-tree',
        'camping',
        'bed',
        'calendar',
        'sunglasses',
        'mountain',
        'house',
        'alarm-clock',
        // Sprzęt IT i technologia
        'laptop',
        'desktop-computer',
        'mobile-phone',
        'printer',
        'keyboard',
        'toolbox',
        'battery',
        // HR i Kadry
        'briefcase',
        'identification-card',
        'people-hugging',
        'handshake',
        'clipboard',
        // Finanse i budżet
        'money-bag',
        'credit-card',
        'dollar-banknote',
        'chart-increasing',
        'receipt',
        // Biuro i infrastruktura
        'office-building',
        'chair',
        'key',
        'automobile',
        'light-bulb',
        // Zdrowie i bezpieczeństwo
        'hospital',
        'adhesive-bandage',
        'face-with-thermometer',
        'shield',
        'warning',
        // Szkolenia i rozwój
        'graduation-cap',
        'books',
        'trophy',
        'teacher',
        'rocket',
        // Dokumenty
        'page-facing-up',
        'file-folder',
        'memo',
        'pen',
        // Ogólne
        'check-mark-button',
        'bell',
        'megaphone'
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

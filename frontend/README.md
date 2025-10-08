# PortalForge Frontend

Frontend application built with Nuxt 3 (Vue 3).

## Setup (To be completed)

```bash
# Install dependencies
npm install

# Development server
npm run dev

# Build for production
npm run build
```

## Tech Stack

- **Framework**: Nuxt 3
- **UI Library**: Tailwind CSS
- **State Management**: Pinia
- **Testing**: Vitest + Playwright
- **TypeScript**: Yes

## Structure (Planned)

```
frontend/
├── components/      # Vue components
├── composables/     # Vue composables
├── layouts/         # Nuxt layouts
├── pages/           # Nuxt pages (auto-routing)
├── stores/          # Pinia stores
├── types/           # TypeScript types
└── tests/           # Unit and E2E tests
```

## Next Steps

1. Initialize Nuxt 3 project: `npx nuxi@latest init .`
2. Install Tailwind CSS: `npm install -D @nuxtjs/tailwindcss`
3. Install Pinia: `npm install @pinia/nuxt`
4. Configure authentication with Supabase
5. Setup API client for backend communication

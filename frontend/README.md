# PortalForge Frontend

Frontend application built with Nuxt 3 (Vue 3) and TypeScript.

## Setup

### Install dependencies

```bash
npm install
```

### Create environment file

```bash
cp .env.example .env
```

Then edit `.env` with your configuration values.

### Development server

Start the development server on `http://localhost:3000`:

```bash
npm run dev
```

## Build

Build the application for production:

```bash
npm run build
```

Preview production build locally:

```bash
npm run preview
```

## Testing

Run unit tests:

```bash
npm run test
```

Run E2E tests:

```bash
npm run test:e2e
```

## Tech Stack

- **Nuxt 3** - Vue 3 meta-framework
- **TypeScript** - Type safety
- **Tailwind CSS** - Utility-first CSS
- **Pinia** - State management
- **VueUse** - Composition utilities
- **Vitest** - Unit testing
- **Playwright** - E2E testing

## Project Structure

```
frontend/
├── assets/         # Uncompiled assets (CSS, images)
├── components/     # Vue components
├── composables/    # Composition functions
├── layouts/        # Layout components
├── pages/          # File-based routing
├── public/         # Static assets
├── stores/         # Pinia stores
├── types/          # TypeScript types
└── utils/          # Utility functions
```

## Documentation

See [.ai/frontend/README.md](../.ai/frontend/README.md) for detailed documentation.

---

**Version**: 0.1.0
**Last Updated**: 2025-10-08

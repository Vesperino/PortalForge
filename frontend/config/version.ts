// Frontend version - update this when deploying new versions
export const APP_VERSION = '1.0.11'

// Build timestamp - will be set during build
export const BUILD_DATE = __BUILD_DATE__ ?? new Date().toISOString()

declare const __BUILD_DATE__: string | undefined

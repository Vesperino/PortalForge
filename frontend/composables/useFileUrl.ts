/**
 * Composable for handling file URLs
 * Converts relative paths to full API URLs
 */
export function useFileUrl() {
  const config = useRuntimeConfig()
  const apiUrl = config.public.apiUrl

  /**
   * Convert a relative file path to a full URL
   * If the path is already a full URL (starts with http/https), return as-is
   * @param relativePath - The relative file path from backend
   * @returns Full URL to access the file
   */
  function getFileUrl(relativePath: string | null | undefined): string {
    if (!relativePath) return ''

    // If already a full URL, return as-is
    if (relativePath.startsWith('http://') || relativePath.startsWith('https://')) {
      return relativePath
    }

    // Remove leading slash if present
    const cleanPath = relativePath.startsWith('/') ? relativePath.substring(1) : relativePath

    // Construct full URL
    return `${apiUrl}/uploads/${cleanPath}`
  }

  return {
    getFileUrl
  }
}

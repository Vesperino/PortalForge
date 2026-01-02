import type { News, NewsCategory } from '~/types'

export interface CreateNewsRequest {
  title: string
  content: string
  excerpt: string
  imageUrl?: string
  category: string
  eventId?: number
  isEvent: boolean
  eventHashtag?: string
  eventDateTime?: string
  eventLocation?: string
  eventPlaceId?: string
  eventLatitude?: number
  eventLongitude?: number
  departmentId?: number
  hashtags?: string[]
}

export interface UpdateNewsRequest {
  title: string
  content: string
  excerpt: string
  imageUrl?: string
  category: string
  eventId?: number
  isEvent: boolean
  eventHashtag?: string
  eventDateTime?: string
  eventLocation?: string
  eventPlaceId?: string
  eventLatitude?: number
  eventLongitude?: number
  departmentId?: number
  hashtags?: string[]
}

export function useNewsApi() {
  const config = useRuntimeConfig()
  const apiUrl = config.public.apiUrl || 'http://localhost:5155'
  const { handleError } = useApiError()

  const authStore = useAuthStore()

  const getAuthHeaders = (): Record<string, string> | undefined => {
    const token = authStore.accessToken

    if (token) {
      return { Authorization: `Bearer ${token}` }
    }
    return undefined
  }

  async function fetchAllNews(
    category?: NewsCategory,
    departmentId?: number,
    isEvent?: boolean,
    hashtags?: string[]
  ): Promise<News[]> {
    const params = new URLSearchParams()
    if (category) params.append('category', category)
    if (departmentId !== undefined) params.append('departmentId', departmentId.toString())
    if (isEvent !== undefined) params.append('isEvent', isEvent.toString())
    if (hashtags && hashtags.length > 0) {
      params.append('hashtags', hashtags.join(','))
    }

    const query = params.toString() ? `?${params.toString()}` : ''
    try {
      const headers = getAuthHeaders()
      const response = await $fetch(`${apiUrl}/api/news${query}`, {
        headers
      }) as { items: News[] }
      return response.items
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie pobrac listy aktualnosci' })
      throw error
    }
  }

  async function fetchNewsById(id: number): Promise<News> {
    try {
      const headers = getAuthHeaders()
      const response = await $fetch(`${apiUrl}/api/news/${id}`, {
        headers
      }) as News
      return response
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie pobrac aktualnosci' })
      throw error
    }
  }

  async function createNews(request: CreateNewsRequest): Promise<number> {
    try {
      const headers = getAuthHeaders()
      const response = await $fetch(`${apiUrl}/api/news`, {
        method: 'POST',
        headers,
        body: request
      }) as number
      return response
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie utworzyc aktualnosci' })
      throw error
    }
  }

  async function updateNews(id: number, request: UpdateNewsRequest): Promise<void> {
    try {
      const headers = getAuthHeaders()
      await $fetch(`${apiUrl}/api/news/${id}`, {
        method: 'PUT',
        headers,
        body: request
      })
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie zaktualizowac aktualnosci' })
      throw error
    }
  }

  async function deleteNews(id: number): Promise<void> {
    try {
      const headers = getAuthHeaders()
      await $fetch(`${apiUrl}/api/news/${id}`, {
        method: 'DELETE',
        headers
      })
    } catch (error) {
      handleError(error, { customMessage: 'Nie udalo sie usunac aktualnosci' })
      throw error
    }
  }

  return {
    fetchAllNews,
    fetchNewsById,
    createNews,
    updateNews,
    deleteNews
  }
}

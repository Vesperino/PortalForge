import type { News, NewsCategory } from '~/types'

export interface CreateNewsRequest {
  title: string
  content: string
  excerpt: string
  imageUrl?: string
  category: string
  eventId?: number
}

export interface UpdateNewsRequest {
  title: string
  content: string
  excerpt: string
  imageUrl?: string
  category: string
  eventId?: number
}

export function useNewsApi() {
  const config = useRuntimeConfig()
  const apiUrl = config.public.apiUrl || 'http://localhost:5155'

  const authStore = useAuthStore()

  const getAuthHeaders = (): Record<string, string> | undefined => {
    const token = authStore.accessToken

    if (token) {
      return { Authorization: `Bearer ${token}` }
    }
    return undefined
  }

  async function fetchAllNews(category?: NewsCategory): Promise<News[]> {
    const query = category ? `?category=${category}` : ''
    const headers = getAuthHeaders()
    const response = await $fetch(`${apiUrl}/api/news${query}`, {
      headers
    }) as { items: News[] }
    return response.items
  }

  async function fetchNewsById(id: number): Promise<News> {
    const headers = getAuthHeaders()
    const response = await $fetch(`${apiUrl}/api/news/${id}`, {
      headers
    }) as News
    return response
  }

  async function createNews(request: CreateNewsRequest): Promise<number> {
    const headers = getAuthHeaders()
    const response = await $fetch(`${apiUrl}/api/news`, {
      method: 'POST',
      headers,
      body: request
    }) as number
    return response
  }

  async function updateNews(id: number, request: UpdateNewsRequest): Promise<void> {
    const headers = getAuthHeaders()
    await $fetch(`${apiUrl}/api/news/${id}`, {
      method: 'PUT',
      headers,
      body: request
    })
  }

  async function deleteNews(id: number): Promise<void> {
    const headers = getAuthHeaders()
    await $fetch(`${apiUrl}/api/news/${id}`, {
      method: 'DELETE',
      headers
    })
  }

  return {
    fetchAllNews,
    fetchNewsById,
    createNews,
    updateNews,
    deleteNews
  }
}

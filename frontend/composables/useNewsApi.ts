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

  const { data: authData } = useAuth()

  const getAuthHeaders = () => {
    const token = authData.value?.session?.access_token
    return token ? { Authorization: `Bearer ${token}` } : {}
  }

  async function fetchAllNews(category?: NewsCategory): Promise<News[]> {
    const query = category ? `?category=${category}` : ''
    const response = await $fetch<News[]>(`${apiUrl}/api/news${query}`, {
      headers: getAuthHeaders()
    })
    return response
  }

  async function fetchNewsById(id: number): Promise<News> {
    const response = await $fetch<News>(`${apiUrl}/api/news/${id}`, {
      headers: getAuthHeaders()
    })
    return response
  }

  async function createNews(request: CreateNewsRequest): Promise<number> {
    const response = await $fetch<number>(`${apiUrl}/api/news`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: request
    })
    return response
  }

  async function updateNews(id: number, request: UpdateNewsRequest): Promise<void> {
    await $fetch(`${apiUrl}/api/news/${id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: request
    })
  }

  async function deleteNews(id: number): Promise<void> {
    await $fetch(`${apiUrl}/api/news/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
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

import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useNewsApi } from '~/composables/useNewsApi'
import { useAuthStore } from '~/stores/auth'

describe('useNewsApi', () => {
  beforeEach(() => {
    // Create a fresh pinia instance for each test
    setActivePinia(createPinia())
    vi.clearAllMocks()

    // Setup auth store with mock token
    const authStore = useAuthStore()
    authStore.accessToken = 'mock-token'
  })

  describe('fetchAllNews', () => {
    it('should fetch all news without category filter', async () => {
      const mockNews = [
        {
          id: 1,
          title: 'Test News 1',
          content: 'Content 1',
          excerpt: 'Excerpt 1',
          authorId: 1,
          createdAt: new Date(),
          updatedAt: new Date(),
          views: 10,
          category: 'announcement'
        },
        {
          id: 2,
          title: 'Test News 2',
          content: 'Content 2',
          excerpt: 'Excerpt 2',
          authorId: 2,
          createdAt: new Date(),
          updatedAt: new Date(),
          views: 5,
          category: 'product'
        }
      ]

      global.$fetch = vi.fn().mockResolvedValue(mockNews)

      const { fetchAllNews } = useNewsApi()
      const result = await fetchAllNews()

      expect(result).toEqual(mockNews)
      expect(global.$fetch).toHaveBeenCalledWith(
        'http://localhost:5155/api/news',
        expect.objectContaining({
          headers: expect.objectContaining({
            Authorization: 'Bearer mock-token'
          })
        })
      )
    })

    it('should fetch news with category filter', async () => {
      const mockNews = [
        {
          id: 1,
          title: 'HR News',
          content: 'HR Content',
          excerpt: 'HR Excerpt',
          authorId: 1,
          createdAt: new Date(),
          updatedAt: new Date(),
          views: 10,
          category: 'hr'
        }
      ]

      global.$fetch = vi.fn().mockResolvedValue(mockNews)

      const { fetchAllNews } = useNewsApi()
      const result = await fetchAllNews('hr')

      expect(result).toEqual(mockNews)
      expect(global.$fetch).toHaveBeenCalledWith(
        'http://localhost:5155/api/news?category=hr',
        expect.any(Object)
      )
    })
  })

  describe('fetchNewsById', () => {
    it('should fetch a single news item by ID', async () => {
      const mockNews = {
        id: 1,
        title: 'Test News',
        content: 'Test Content',
        excerpt: 'Test Excerpt',
        authorId: 1,
        createdAt: new Date(),
        updatedAt: new Date(),
        views: 10,
        category: 'announcement'
      }

      global.$fetch = vi.fn().mockResolvedValue(mockNews)

      const { fetchNewsById } = useNewsApi()
      const result = await fetchNewsById(1)

      expect(result).toEqual(mockNews)
      expect(global.$fetch).toHaveBeenCalledWith(
        'http://localhost:5155/api/news/1',
        expect.any(Object)
      )
    })
  })

  describe('createNews', () => {
    it('should create a new news item', async () => {
      const mockNewsId = 5

      global.$fetch = vi.fn().mockResolvedValue(mockNewsId)

      const { createNews } = useNewsApi()
      const result = await createNews({
        title: 'New News',
        content: '<p>Rich content</p>',
        excerpt: 'New excerpt',
        category: 'announcement'
      })

      expect(result).toBe(mockNewsId)
      expect(global.$fetch).toHaveBeenCalledWith(
        'http://localhost:5155/api/news',
        expect.objectContaining({
          method: 'POST',
          body: expect.objectContaining({
            title: 'New News',
            category: 'announcement'
          })
        })
      )
    })
  })

  describe('updateNews', () => {
    it('should update an existing news item', async () => {
      global.$fetch = vi.fn().mockResolvedValue(undefined)

      const { updateNews } = useNewsApi()
      await updateNews(1, {
        title: 'Updated News',
        content: '<p>Updated content</p>',
        excerpt: 'Updated excerpt',
        category: 'product'
      })

      expect(global.$fetch).toHaveBeenCalledWith(
        'http://localhost:5155/api/news/1',
        expect.objectContaining({
          method: 'PUT',
          body: expect.objectContaining({
            title: 'Updated News'
          })
        })
      )
    })
  })

  describe('deleteNews', () => {
    it('should delete a news item', async () => {
      global.$fetch = vi.fn().mockResolvedValue(undefined)

      const { deleteNews } = useNewsApi()
      await deleteNews(1)

      expect(global.$fetch).toHaveBeenCalledWith(
        'http://localhost:5155/api/news/1',
        expect.objectContaining({
          method: 'DELETE'
        })
      )
    })
  })
})

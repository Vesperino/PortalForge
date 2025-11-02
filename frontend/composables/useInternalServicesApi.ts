import type {
  InternalService,
  InternalServiceCategory,
  CreateInternalServiceRequest,
  UpdateInternalServiceRequest,
  CreateCategoryRequest,
  UpdateCategoryRequest
} from '~/types/internal-services'

export function useInternalServicesApi() {
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

  // ========== SERVICES ==========

  /**
   * Get all services (admin only)
   */
  async function fetchAllServices(): Promise<InternalService[]> {
    const headers = getAuthHeaders()
    const response = await $fetch(`${apiUrl}/api/internal-services`, {
      headers
    }) as InternalService[]
    return response
  }

  /**
   * Get services for current user (filtered by department)
   */
  async function fetchMyServices(): Promise<InternalService[]> {
    const headers = getAuthHeaders()
    const response = await $fetch(`${apiUrl}/api/internal-services/my-services`, {
      headers
    }) as InternalService[]
    return response
  }

  /**
   * Get service by ID
   */
  async function fetchServiceById(id: string): Promise<InternalService> {
    const headers = getAuthHeaders()
    const response = await $fetch(`${apiUrl}/api/internal-services/${id}`, {
      headers
    }) as InternalService
    return response
  }

  /**
   * Create a new service (admin only)
   */
  async function createService(request: CreateInternalServiceRequest): Promise<string> {
    const headers = getAuthHeaders()
    const response = await $fetch(`${apiUrl}/api/internal-services`, {
      method: 'POST',
      headers,
      body: request
    }) as string
    return response
  }

  /**
   * Update existing service (admin only)
   */
  async function updateService(id: string, request: UpdateInternalServiceRequest): Promise<boolean> {
    const headers = getAuthHeaders()
    const response = await $fetch(`${apiUrl}/api/internal-services/${id}`, {
      method: 'PUT',
      headers,
      body: request
    }) as boolean
    return response
  }

  /**
   * Delete service (admin only)
   */
  async function deleteService(id: string): Promise<boolean> {
    const headers = getAuthHeaders()
    const response = await $fetch(`${apiUrl}/api/internal-services/${id}`, {
      method: 'DELETE',
      headers
    }) as boolean
    return response
  }

  // ========== CATEGORIES ==========

  /**
   * Get all categories
   */
  async function fetchAllCategories(): Promise<InternalServiceCategory[]> {
    const headers = getAuthHeaders()
    const response = await $fetch(`${apiUrl}/api/internal-services/categories`, {
      headers
    }) as InternalServiceCategory[]
    return response
  }

  /**
   * Create a new category (admin only)
   */
  async function createCategory(request: CreateCategoryRequest): Promise<string> {
    const headers = getAuthHeaders()
    const response = await $fetch(`${apiUrl}/api/internal-services/categories`, {
      method: 'POST',
      headers,
      body: request
    }) as string
    return response
  }

  /**
   * Update existing category (admin only)
   */
  async function updateCategory(id: string, request: UpdateCategoryRequest): Promise<boolean> {
    const headers = getAuthHeaders()
    const response = await $fetch(`${apiUrl}/api/internal-services/categories/${id}`, {
      method: 'PUT',
      headers,
      body: request
    }) as boolean
    return response
  }

  /**
   * Delete category (admin only)
   */
  async function deleteCategory(id: string): Promise<boolean> {
    const headers = getAuthHeaders()
    const response = await $fetch(`${apiUrl}/api/internal-services/categories/${id}`, {
      method: 'DELETE',
      headers
    }) as boolean
    return response
  }

  return {
    // Services
    fetchAllServices,
    fetchMyServices,
    fetchServiceById,
    createService,
    updateService,
    deleteService,

    // Categories
    fetchAllCategories,
    createCategory,
    updateCategory,
    deleteCategory
  }
}

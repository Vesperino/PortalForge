import type { Position, CreatePositionDto } from '~/types/position'

interface GetPositionsResult {
  positions: Position[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export function usePositions() {
  const config = useRuntimeConfig()
  const baseUrl = config.public.apiUrl || 'http://localhost:5155'
  const authStore = useAuthStore()
  const getAuthHeaders = (): Record<string, string> | undefined => {
    const token = authStore.accessToken
    return token ? { Authorization: `Bearer ${token}` } : undefined
  }

  async function searchPositions(searchTerm: string): Promise<Position[]> {
    try {
      const response = await $fetch<Position[]>(`${baseUrl}/api/positions/search`, {
        params: { searchTerm },
        headers: getAuthHeaders()
      })
      return response
    } catch (error) {
      console.error('Error searching positions:', error)
      return []
    }
  }

  async function getAllPositions(isActive: boolean | null = true): Promise<Position[]> {
    try {
      const response = await $fetch<GetPositionsResult>(`${baseUrl}/api/positions`, {
        params: { isActive, pageSize: 1000 },
        headers: getAuthHeaders()
      })
      return response.positions
    } catch (error) {
      console.error('Error fetching positions:', error)
      return []
    }
  }

  async function getPositionsPaginated(
    searchTerm?: string,
    isActive: boolean | null = true,
    pageNumber: number = 1,
    pageSize: number = 20
  ): Promise<GetPositionsResult> {
    try {
      const response = await $fetch<GetPositionsResult>(`${baseUrl}/api/positions`, {
        params: { searchTerm, isActive, pageNumber, pageSize },
        headers: getAuthHeaders()
      })
      return response
    } catch (error) {
      console.error('Error fetching positions:', error)
      return { positions: [], totalCount: 0, pageNumber: 1, pageSize: 20, totalPages: 0, hasPreviousPage: false, hasNextPage: false }
    }
  }

  /**
   * Create a new position
   */
  async function createPosition(dto: CreatePositionDto): Promise<string | null> {
    try {
      const response = await $fetch<string>(`${baseUrl}/api/positions`, {
        method: 'POST',
        body: dto,
        headers: getAuthHeaders()
      })
      return response
    } catch (error) {
      console.error('Error creating position:', error)
      return null
    }
  }

  return {
    searchPositions,
    getAllPositions,
    getPositionsPaginated,
    createPosition
  }
}

import type { Position, CreatePositionDto } from '~/types/position'

export function usePositions() {
  const config = useRuntimeConfig()
  const baseUrl = config.public.apiBaseUrl

  /**
   * Search positions by name for autocomplete
   */
  async function searchPositions(searchTerm: string): Promise<Position[]> {
    try {
      const response = await $fetch<Position[]>(`${baseUrl}/api/positions/search`, {
        params: { searchTerm }
      })
      return response
    } catch (error) {
      console.error('Error searching positions:', error)
      return []
    }
  }

  /**
   * Get all active positions
   */
  async function getAllPositions(activeOnly: boolean = true): Promise<Position[]> {
    try {
      const response = await $fetch<Position[]>(`${baseUrl}/api/positions`, {
        params: { activeOnly }
      })
      return response
    } catch (error) {
      console.error('Error fetching positions:', error)
      return []
    }
  }

  /**
   * Create a new position
   */
  async function createPosition(dto: CreatePositionDto): Promise<string | null> {
    try {
      const response = await $fetch<string>(`${baseUrl}/api/positions`, {
        method: 'POST',
        body: dto
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
    createPosition
  }
}

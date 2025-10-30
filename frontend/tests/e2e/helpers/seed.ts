import { Page } from '@playwright/test'

const API_URL = process.env.NUXT_PUBLIC_API_URL || 'http://localhost:5155'

/**
 * Seed news data for testing
 */
export async function seedNewsData() {
  try {
    const response = await fetch(`${API_URL}/api/news/seed`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
    })
    
    if (!response.ok) {
      console.warn('Failed to seed news data:', response.statusText)
      return false
    }
    
    const result = await response.json()
    console.log('Seeded news:', result.message)
    return true
  } catch (error) {
    console.error('Error seeding news data:', error)
    return false
  }
}

/**
 * Seed request templates data for testing
 * Requires admin authentication
 */
export async function seedRequestTemplates(page: Page) {
  try {
    // Use page.evaluate to make the request with the page's authentication context
    const result = await page.evaluate(async (apiUrl) => {
      const response = await fetch(`${apiUrl}/api/request-templates/seed`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include', // Include cookies
      })

      if (!response.ok) {
        const text = await response.text()
        return { success: false, error: `${response.status}: ${text}` }
      }

      const data = await response.json()
      return { success: true, data }
    }, API_URL)

    if (!result.success) {
      console.warn('Failed to seed request templates:', result.error)
      return false
    }

    console.log('Seeded request templates:', result.data?.message || 'Success')
    return true
  } catch (error) {
    console.error('Error seeding request templates:', error)
    return false
  }
}

/**
 * Seed all test data
 */
export async function seedAllTestData(page: Page) {
  console.log('Seeding test data...')
  
  const newsSeeded = await seedNewsData()
  const templatesSeeded = await seedRequestTemplates(page)
  
  return {
    news: newsSeeded,
    templates: templatesSeeded,
  }
}

/**
 * Clear all test data (if needed for cleanup)
 * Note: This would need corresponding backend endpoints
 */
export async function clearTestData(page: Page) {
  // Implementation would depend on backend cleanup endpoints
  console.log('Clear test data not implemented yet')
}



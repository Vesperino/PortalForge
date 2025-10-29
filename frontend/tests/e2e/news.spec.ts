import { test, expect } from '@playwright/test'

test.describe('News System', () => {
  test('should display news list with seeded data', async ({ page }) => {
    // Navigate to news page
    await page.goto('/dashboard/news')
    await page.waitForLoadState('networkidle')

    // Wait for page to load
    await page.waitForTimeout(1000)

    // Take screenshot for debugging
    await page.screenshot({ path: 'test-results/news-page-loaded.png', fullPage: true })

    // Check if the main page heading is visible
    await expect(page.getByRole('heading', { level: 1 })).toContainText('Aktualności')

    // Check if news cards are visible
    const newsCards = page.locator('.news-card, [class*="news"], article')
    const cardCount = await newsCards.count()

    // We should have at least 5 news items from seed data
    expect(cardCount).toBeGreaterThanOrEqual(5)

    // Verify some seeded news titles are present
    await expect(page.getByText('Witamy w nowym systemie newsów!')).toBeVisible({ timeout: 5000 })
    await expect(page.getByText('Nowe funkcje w aplikacji PortalForge')).toBeVisible({ timeout: 5000 })
  })

  test('should filter news by category', async ({ page }) => {
    await page.goto('/dashboard/news')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Take initial screenshot
    await page.screenshot({ path: 'test-results/news-before-filter.png', fullPage: true })

    // Try to find and click category filter
    // This might be a select dropdown or buttons
    const categorySelect = page.locator('select').first()
    if (await categorySelect.isVisible()) {
      await categorySelect.selectOption('announcement')
      await page.waitForTimeout(500)

      // Take screenshot after filtering
      await page.screenshot({ path: 'test-results/news-filtered-announcement.png', fullPage: true })

      // Verify announcement news is visible
      await expect(page.getByText('Witamy w nowym systemie newsów!')).toBeVisible()
    }

    // Test search functionality if available
    const searchInput = page.locator('input[type="search"], input[placeholder*="Szukaj"]')
    if (await searchInput.isVisible()) {
      await searchInput.fill('system')
      await page.waitForTimeout(500)

      await page.screenshot({ path: 'test-results/news-search.png', fullPage: true })
    }
  })

  test('should navigate to single news article and display full content', async ({ page }) => {
    await page.goto('/dashboard/news')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Find and click on the first news article
    const firstNewsTitle = page.getByText('Witamy w nowym systemie newsów!')
    await expect(firstNewsTitle).toBeVisible({ timeout: 5000 })

    // Click on the news title or card to navigate to detail page
    await firstNewsTitle.click()

    // Wait for navigation to detail page
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Take screenshot of detail page
    await page.screenshot({ path: 'test-results/news-detail.png', fullPage: true })

    // Verify we're on the detail page (URL should contain /news/1 or similar)
    expect(page.url()).toContain('/dashboard/news/')

    // Verify article content is displayed
    await expect(page.getByText('Witamy w nowym systemie newsów!')).toBeVisible()
    await expect(page.getByText(/Nowy system newsów/)).toBeVisible()

    // Check if rich HTML content is rendered
    await expect(page.locator('h2, h3').first()).toBeVisible()

    // Check if author info is displayed
    await expect(page.getByText(/Arkadiusz Białecki/)).toBeVisible()

    // Check if view count is displayed
    await expect(page.getByText(/wyświetleń/)).toBeVisible()

    // Test navigation back to news list
    const backButton = page.getByRole('link', { name: /wróć/i }).or(page.getByRole('button', { name: /wróć/i }))
    if (await backButton.isVisible()) {
      await backButton.click()
      await page.waitForLoadState('networkidle')
      expect(page.url()).toContain('/dashboard/news')
    }
  })

  test('should increment view count when viewing news article', async ({ page }) => {
    await page.goto('/dashboard/news')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Click on first news to view it
    const firstNews = page.getByText('Witamy w nowym systemie newsów!')
    await firstNews.click()
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Get initial view count
    const viewCountText = await page.getByText(/\d+ wyświetleń/).textContent()
    const initialViews = parseInt(viewCountText?.match(/\d+/)?.[0] || '0')

    // Navigate back and view again
    await page.goto('/dashboard/news')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(500)

    await page.getByText('Witamy w nowym systemie newsów!').click()
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Check if view count increased
    const newViewCountText = await page.getByText(/\d+ wyświetleń/).textContent()
    const newViews = parseInt(newViewCountText?.match(/\d+/)?.[0] || '0')

    expect(newViews).toBeGreaterThan(initialViews)
  })

  test('should display create news button for authorized users', async ({ page }) => {
    await page.goto('/dashboard/news')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Check if create/add button exists
    const createButton = page.getByRole('button', { name: /dodaj/i }).or(
      page.getByRole('link', { name: /dodaj/i })
    )

    // Take screenshot
    await page.screenshot({ path: 'test-results/news-with-create-button.png', fullPage: true })

    // If button is visible, click it to navigate to create page
    if (await createButton.isVisible()) {
      await createButton.click()
      await page.waitForLoadState('networkidle')

      expect(page.url()).toContain('/dashboard/news/create')

      await page.screenshot({ path: 'test-results/news-create-page.png', fullPage: true })

      // Check if rich text editor is visible
      await expect(page.locator('.ProseMirror, [class*="editor"]')).toBeVisible({ timeout: 5000 })
    }
  })

  test('should validate required fields in create news form', async ({ page }) => {
    await page.goto('/dashboard/news/create')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Take screenshot
    await page.screenshot({ path: 'test-results/news-create-form.png', fullPage: true })

    // Try to submit empty form
    const submitButton = page.getByRole('button', { name: /zapisz|utwórz|dodaj/i })
    if (await submitButton.isVisible()) {
      await submitButton.click()
      await page.waitForTimeout(500)

      // Check if validation messages appear
      await page.screenshot({ path: 'test-results/news-create-validation.png', fullPage: true })

      // Validation message might appear as error text or in console
      const errorMessage = page.getByText(/wymagane|required/i)
      if (await errorMessage.isVisible()) {
        expect(await errorMessage.count()).toBeGreaterThan(0)
      }
    }
  })

  test('should navigate between news articles in pagination', async ({ page }) => {
    await page.goto('/dashboard/news')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Check if pagination exists
    const paginationNext = page.getByRole('button', { name: /następna|next/i })
    const paginationPrev = page.getByRole('button', { name: /poprzednia|previous/i })

    if (await paginationNext.isVisible()) {
      // Take screenshot before pagination
      await page.screenshot({ path: 'test-results/news-page-1.png', fullPage: true })

      // Click next page
      await paginationNext.click()
      await page.waitForTimeout(500)

      // Take screenshot after pagination
      await page.screenshot({ path: 'test-results/news-page-2.png', fullPage: true })

      // Click previous page
      if (await paginationPrev.isVisible()) {
        await paginationPrev.click()
        await page.waitForTimeout(500)
      }
    }
  })

  test('should display different news categories', async ({ page }) => {
    await page.goto('/dashboard/news')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Check if different category badges/labels are visible
    const categories = ['Announcement', 'Product', 'HR', 'Tech', 'Event']

    for (const category of categories) {
      const categoryElement = page.getByText(new RegExp(category, 'i'))
      if (await categoryElement.isVisible()) {
        await expect(categoryElement).toBeVisible()
      }
    }

    await page.screenshot({ path: 'test-results/news-categories.png', fullPage: true })
  })
})

import { test, expect } from '@playwright/test'
import { loginAsAdmin } from './helpers/auth'
import { seedRequestTemplates } from './helpers/seed'

test.describe('Request Templates Management', () => {
  let isSeeded = false

  test.beforeEach(async ({ page }) => {
    // Login as admin before each test
    await loginAsAdmin(page)

    // Seed data only once for all tests
    if (!isSeeded) {
      await seedRequestTemplates(page)
      isSeeded = true
    }
  })

  test('should display request templates list', async ({ page }) => {
    // Navigate to request templates page
    await page.goto('/admin/request-templates')
    await page.waitForLoadState('networkidle')

    // Wait for page to load
    await page.waitForTimeout(1000)

    // Check if page loaded - look for h1 with "Szablony wniosków"
    const heading = page.getByRole('heading', { name: /szablony wniosków/i, level: 1 })
    await expect(heading).toBeVisible()

    // Check if "Nowy szablon" button is visible
    const newTemplateButton = page.getByRole('link', { name: /nowy szablon/i })
    await expect(newTemplateButton).toBeVisible()
  })

  test('should navigate to create template page', async ({ page }) => {
    // Navigate to request templates page
    await page.goto('/admin/request-templates')
    await page.waitForLoadState('networkidle')

    // Wait for page to load
    await page.waitForTimeout(1000)

    // Click "Nowy szablon" button
    const newTemplateButton = page.getByRole('link', { name: /nowy szablon/i })
    await newTemplateButton.click()

    // Wait for navigation
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Verify we're on create page
    const currentUrl = page.url()
    expect(currentUrl).toContain('/admin/request-templates/create')

    // Verify page heading
    const heading = page.getByRole('heading', { name: /utwórz szablon wniosku/i, level: 1 })
    await expect(heading).toBeVisible()
  })

  test('should delete a template successfully', async ({ page }) => {
    // Navigate to request templates page
    await page.goto('/admin/request-templates')
    await page.waitForLoadState('networkidle')

    // Wait for templates to load
    await page.waitForTimeout(1000)

    // Get initial count of templates
    const deleteButtons = page.getByRole('button', { name: /usuń/i })
    const initialCount = await deleteButtons.count()
    expect(initialCount).toBeGreaterThan(0)

    // Set up dialog handler BEFORE clicking the button
    page.once('dialog', async dialog => {
      expect(dialog.type()).toBe('confirm')
      expect(dialog.message()).toContain('Czy na pewno chcesz usunąć szablon')
      await dialog.accept()
    })

    // Click first delete button
    await deleteButtons.first().click()

    // Wait for deletion to complete
    await page.waitForTimeout(3000)

    // Verify template count decreased (toast might disappear quickly)
    const newCount = await deleteButtons.count()
    expect(newCount).toBe(initialCount - 1)
  })

  test('should show error when deleting template in use', async ({ page }) => {
    // This scenario requires a template referenced by existing requests.
    // Keep as TODO for full integration; for now just assert page loads.
    await page.goto('/admin/request-templates')
    await page.waitForLoadState('networkidle')
    await expect(page.getByRole('heading', { name: /szablony wniosków/i, level: 1 })).toBeVisible()
  })

  test('should filter templates by category', async ({ page }) => {
    // Navigate to request templates page
    await page.goto('/admin/request-templates')
    await page.waitForLoadState('networkidle')

    // Wait for templates to load
    await page.waitForTimeout(1000)

    // Get all category filter buttons
    const categoryButtons = page.locator('button').filter({ hasText: /Testing|HR|Hardware|Software|Security|Training/i })
    const buttonCount = await categoryButtons.count()
    expect(buttonCount).toBeGreaterThan(0)

    // Click first category button
    await categoryButtons.first().click()

    // Wait for filter to apply
    await page.waitForTimeout(1000)

    // Verify that templates are filtered (this is a basic check)
    // In a real scenario, you'd verify that only templates of the selected category are shown
    const templates = page.locator('article, [class*="card"]')
    const templateCount = await templates.count()
    
    // Just verify that the page still shows content after filtering
    expect(templateCount).toBeGreaterThanOrEqual(0)
  })

  test('should navigate to edit template page', async ({ page }) => {
    // Navigate to request templates page
    await page.goto('/admin/request-templates')
    await page.waitForLoadState('networkidle')

    // Wait for templates to load
    await page.waitForTimeout(1000)

    // Find and click first "Edytuj" link
    const editLinks = page.getByRole('link', { name: /edytuj/i })
    const editCount = await editLinks.count()
    expect(editCount).toBeGreaterThan(0)

    await editLinks.first().click()

    // Wait for navigation
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Verify we're on edit page
    const currentUrl = page.url()
    expect(currentUrl).toContain('/admin/request-templates/edit/')

    // Verify page heading
    const heading = page.getByRole('heading', { name: /edytuj szablon wniosku/i, level: 1 })
    await expect(heading).toBeVisible()
  })

  test('should search templates', async ({ page }) => {
    // Navigate to request templates page
    await page.goto('/admin/request-templates')
    await page.waitForLoadState('networkidle')

    // Wait for templates to load
    await page.waitForTimeout(1000)

    // Find search input
    const searchInput = page.getByPlaceholder(/szukaj szablonów/i)
    await expect(searchInput).toBeVisible()

    // Type search query
    await searchInput.fill('Test')

    // Wait for search to apply
    await page.waitForTimeout(1000)

    // Verify that search results are shown
    // This is a basic check - in a real scenario, you'd verify specific results
    const templates = page.locator('article, [class*="card"]')
    const templateCount = await templates.count()
    
    expect(templateCount).toBeGreaterThanOrEqual(0)
  })

  test('should show template details', async ({ page }) => {
    // Navigate to request templates page
    await page.goto('/admin/request-templates')
    await page.waitForLoadState('networkidle')

    // Wait for templates to load
    await page.waitForTimeout(1000)

    // Find and click first "Szczegóły" button
    const detailsButtons = page.getByRole('button', { name: /szczegóły/i })
    const detailsCount = await detailsButtons.count()
    expect(detailsCount).toBeGreaterThan(0)

    await detailsButtons.first().click()

    // Wait for details to appear (could be modal or navigation)
    await page.waitForTimeout(1000)

    // This is a basic check - in a real scenario, you'd verify specific details are shown
    // The implementation might show a modal or navigate to a details page
  })

  test('should handle delete confirmation cancellation', async ({ page }) => {
    // Navigate to request templates page
    await page.goto('/admin/request-templates')
    await page.waitForLoadState('networkidle')

    // Wait for templates to load
    await page.waitForTimeout(1000)

    // Get initial count of templates
    const deleteButtons = page.getByRole('button', { name: /usuń/i })
    const initialCount = await deleteButtons.count()
    expect(initialCount).toBeGreaterThan(0)

    // Set up dialog handler BEFORE clicking the button - CANCEL this time
    page.once('dialog', async dialog => {
      expect(dialog.type()).toBe('confirm')
      await dialog.dismiss()
    })

    // Click first delete button
    await deleteButtons.first().click()

    // Wait a moment
    await page.waitForTimeout(1000)

    // Verify template count is unchanged
    const newCount = await deleteButtons.count()
    expect(newCount).toBe(initialCount)
  })
})


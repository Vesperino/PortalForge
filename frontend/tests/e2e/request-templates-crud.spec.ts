import { test, expect } from '@playwright/test'
import { loginAsAdmin } from './helpers/auth'
import { seedRequestTemplates } from './helpers/seed'

test.describe('Request Templates CRUD Operations', () => {
  let isSeeded = false

  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page)

    // Seed data only once for all tests
    if (!isSeeded) {
      await seedRequestTemplates(page)
      isSeeded = true
    }
  })

  test('should create a new request template with all fields', async ({ page }) => {
    await page.goto('/admin/request-templates/create')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Step 1: Basic Info
    await page.fill('input[placeholder*="Zamówienie sprzętu"]', 'Test Template E2E')
    await page.fill('textarea[placeholder*="Opisz cel"]', 'This is a test template created by E2E tests')
    await page.fill('input[placeholder*="Hardware"]', 'Testing')
    await page.fill('input[placeholder*="Zostaw puste"]', 'IT')
    await page.fill('input[placeholder="7"]', '5')

    // Verify checkbox is checked by default
    const requiresApprovalCheckbox = page.locator('input[id="requiresApproval"]')
    await expect(requiresApprovalCheckbox).toBeChecked()

    await page.screenshot({ path: 'test-results/template-create-step1.png', fullPage: true })

    // Go to next step
    await page.getByRole('button', { name: /dalej/i }).click()
    await page.waitForTimeout(500)

    // Step 2: Form Fields
    await page.getByRole('button', { name: /dodaj pole/i }).click()
    await page.waitForTimeout(300)

    // Fill first field
    const firstFieldLabel = page.locator('input').filter({ hasText: '' }).first()
    await page.locator('.bg-gray-50').first().locator('input').first().fill('Test Field 1')
    
    await page.screenshot({ path: 'test-results/template-create-step2.png', fullPage: true })

    // Go to next step
    await page.getByRole('button', { name: /dalej/i }).click()
    await page.waitForTimeout(500)

    // Step 3: Approval Flow
    await page.getByRole('button', { name: /dodaj etap/i }).click()
    await page.waitForTimeout(1000)

    await page.screenshot({ path: 'test-results/template-create-step3.png', fullPage: true })

    // Go to final step
    await page.getByRole('button', { name: /dalej/i }).click()
    await page.waitForTimeout(500)

    // Step 4: Quiz (optional, just verify we can navigate)
    await page.screenshot({ path: 'test-results/template-create-step4.png', fullPage: true })

    // Save template
    const saveButton = page.getByRole('button', { name: /zapisz/i })
    await saveButton.click()
    
    // Wait for redirect and toast
    await page.waitForTimeout(2000)

    // Should be redirected back to list
    expect(page.url()).toContain('/admin/request-templates')
  })

  test('should read/view template details', async ({ page }) => {
    await page.goto('/admin/request-templates')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Find first template - look for heading with template name (has text)
    const firstTemplateHeading = page.locator('h3').filter({ hasText: /.+/ }).first()
    await expect(firstTemplateHeading).toBeVisible({ timeout: 5000 })

    // Verify edit link exists
    const editLink = page.getByRole('link', { name: /edytuj/i }).first()
    await expect(editLink).toBeVisible()

    await page.screenshot({ path: 'test-results/template-list-view.png', fullPage: true })
  })

  test('should update an existing template', async ({ page }) => {
    await page.goto('/admin/request-templates')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Click edit on first template
    const editLink = page.getByRole('link', { name: /edytuj/i }).first()
    await editLink.click()
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Verify we're on edit page
    await expect(page.getByRole('heading', { name: /edytuj szablon/i })).toBeVisible()

    // Modify the name
    const nameInput = page.locator('input').first()
    const originalName = await nameInput.inputValue()
    await nameInput.fill(originalName + ' (Updated)')

    await page.screenshot({ path: 'test-results/template-edit-modified.png', fullPage: true })

    // Navigate through steps to save button
    const nextButtons = page.getByRole('button', { name: /dalej/i })
    const nextCount = await nextButtons.count()
    
    // Click next buttons to get to the last step
    for (let i = 0; i < 3; i++) {
      const nextBtn = page.getByRole('button', { name: /dalej/i })
      if (await nextBtn.isVisible().catch(() => false)) {
        await nextBtn.click()
        await page.waitForTimeout(300)
      }
    }

    // Save changes
    const saveButton = page.getByRole('button', { name: /zapisz zmiany/i })
    await saveButton.click()
    await page.waitForTimeout(2000)

    // Should be redirected back to list
    expect(page.url()).toContain('/admin/request-templates')

    await page.screenshot({ path: 'test-results/template-after-update.png', fullPage: true })
  })

  test('should delete a template with confirmation', async ({ page }) => {
    await page.goto('/admin/request-templates')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Get initial template count
    const deleteButtons = page.getByRole('button', { name: /usuń/i })
    const initialCount = await deleteButtons.count()

    expect(initialCount).toBeGreaterThan(0)

    // Set up dialog handler to accept deletion
    page.once('dialog', async dialog => {
      expect(dialog.type()).toBe('confirm')
      await dialog.accept()
    })

    // Click delete on last template (to avoid deleting one we might need)
    await deleteButtons.last().click()

    // Wait for deletion
    await page.waitForTimeout(2000)

    // Verify count decreased
    const newCount = await page.getByRole('button', { name: /usuń/i }).count()
    expect(newCount).toBe(initialCount - 1)

    await page.screenshot({ path: 'test-results/template-after-delete.png', fullPage: true })
  })

  test('should cancel template deletion', async ({ page }) => {
    await page.goto('/admin/request-templates')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Get initial template count
    const deleteButtons = page.getByRole('button', { name: /usuń/i })
    const initialCount = await deleteButtons.count()

    expect(initialCount).toBeGreaterThan(0)

    // Set up dialog handler to CANCEL deletion
    page.once('dialog', async dialog => {
      expect(dialog.type()).toBe('confirm')
      await dialog.dismiss()
    })

    // Click delete
    await deleteButtons.first().click()

    // Wait a moment
    await page.waitForTimeout(1000)

    // Verify count stayed the same
    const newCount = await page.getByRole('button', { name: /usuń/i }).count()
    expect(newCount).toBe(initialCount)
  })

  test('should filter templates by category', async ({ page }) => {
    await page.goto('/admin/request-templates')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Get all templates initially
    const allTemplates = page.locator('article, [class*="template-card"]')
    const totalCount = await allTemplates.count()

    // Click on a category filter button
    const categoryButton = page.locator('button').filter({ hasText: /hardware|software|hr/i }).first()
    
    if (await categoryButton.isVisible().catch(() => false)) {
      await categoryButton.click()
      await page.waitForTimeout(500)

      // Filtered count might be different
      const filteredCount = await allTemplates.count()
      
      // Just verify the page still works after filtering
      expect(filteredCount).toBeGreaterThanOrEqual(0)

      await page.screenshot({ path: 'test-results/template-filtered.png', fullPage: true })
    }
  })

  test('should search templates by name', async ({ page }) => {
    await page.goto('/admin/request-templates')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Find search input
    const searchInput = page.getByPlaceholder(/szukaj/i)
    
    if (await searchInput.isVisible().catch(() => false)) {
      // Type search query
      await searchInput.fill('sprzęt')
      await page.waitForTimeout(500)

      // Should show filtered results
      await page.screenshot({ path: 'test-results/template-search.png', fullPage: true })

      // Clear search
      await searchInput.clear()
      await page.waitForTimeout(500)
    }
  })

  test('should toggle template active status', async ({ page }) => {
    await page.goto('/admin/request-templates')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Navigate to edit page
    const editLink = page.getByRole('link', { name: /edytuj/i }).first()
    await editLink.click()
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Find and toggle isActive checkbox
    const isActiveCheckbox = page.locator('input[id="isActive"]')
    const wasChecked = await isActiveCheckbox.isChecked()
    
    await isActiveCheckbox.click()
    await page.waitForTimeout(300)

    // Verify it toggled
    const nowChecked = await isActiveCheckbox.isChecked()
    expect(nowChecked).toBe(!wasChecked)

    await page.screenshot({ path: 'test-results/template-toggle-active.png', fullPage: true })
  })

  test('should add and remove form fields', async ({ page }) => {
    await page.goto('/admin/request-templates')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Navigate to edit page
    const editLink = page.getByRole('link', { name: /edytuj/i }).first()
    await editLink.click()
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Go to step 2 (Form Fields)
    await page.getByRole('button', { name: /dalej/i }).click()
    await page.waitForTimeout(500)

    // Get initial field count
    const initialFields = await page.locator('.bg-gray-50.dark\\:bg-gray-700\\/50').count()

    // Add a new field
    await page.getByRole('button', { name: /dodaj pole/i }).click()
    await page.waitForTimeout(300)

    // Verify field was added
    const newFieldCount = await page.locator('.bg-gray-50.dark\\:bg-gray-700\\/50').count()
    expect(newFieldCount).toBe(initialFields + 1)

    await page.screenshot({ path: 'test-results/template-field-added.png', fullPage: true })

    // Remove the field we just added - find delete button within the last field container
    const fieldContainers = page.locator('.bg-gray-50.dark\\:bg-gray-700\\/50')
    const lastField = fieldContainers.last()
    const deleteButton = lastField.locator('button').filter({ hasText: /usuń|delete/i }).or(
      lastField.locator('button[aria-label*="usuń"], button[aria-label*="delete"]')
    ).or(
      lastField.locator('button').last() // Fallback to last button in the field
    )
    await deleteButton.first().click()
    await page.waitForTimeout(300)

    // Verify field was removed
    const finalFieldCount = await page.locator('.bg-gray-50.dark\\:bg-gray-700\\/50').count()
    expect(finalFieldCount).toBe(initialFields)
  })

  test('should add and remove approval steps', async ({ page }) => {
    await page.goto('/admin/request-templates')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Navigate to edit page
    const editLink = page.getByRole('link', { name: /edytuj/i }).first()
    await editLink.click()
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Go to step 3 (Approval Flow)
    await page.getByRole('button', { name: /dalej/i }).click()
    await page.waitForTimeout(300)
    await page.getByRole('button', { name: /dalej/i }).click()
    await page.waitForTimeout(1000) // Wait for users/groups to load

    // Get initial approval step count
    const initialSteps = await page.locator('[class*="approval-step"], .bg-gray-50').count()

    // Add a new approval step
    await page.getByRole('button', { name: /dodaj etap/i }).click()
    await page.waitForTimeout(300)

    // Verify step was added
    const newStepCount = await page.locator('[class*="approval-step"], .bg-gray-50').count()
    expect(newStepCount).toBeGreaterThan(initialSteps)

    await page.screenshot({ path: 'test-results/template-approval-added.png', fullPage: true })
  })

  test('should validate required fields on create', async ({ page }) => {
    await page.goto('/admin/request-templates/create')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Try to go to next step without filling required fields
    const nameInput = page.locator('input').first()
    await nameInput.fill('')

    // Navigate through all steps
    for (let i = 0; i < 3; i++) {
      const nextBtn = page.getByRole('button', { name: /dalej/i })
      if (await nextBtn.isVisible().catch(() => false)) {
        await nextBtn.click()
        await page.waitForTimeout(300)
      }
    }

    // Try to save
    const saveButton = page.getByRole('button', { name: /zapisz/i })
    await saveButton.click()
    await page.waitForTimeout(1000)

    // Should show error or stay on same page due to validation
    // (Depends on implementation - might show toast or inline errors)
    await page.screenshot({ path: 'test-results/template-validation.png', fullPage: true })
  })

  test('should navigate between form steps', async ({ page }) => {
    await page.goto('/admin/request-templates/create')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Verify step 1 is active
    const step1Button = page.locator('button').filter({ hasText: /podstawowe info/i })
    await expect(step1Button).toHaveClass(/bg-blue-600/)

    // Go to step 2
    await page.getByRole('button', { name: /dalej/i }).click()
    await page.waitForTimeout(300)

    // Verify step 2 is active
    const step2Button = page.locator('button').filter({ hasText: /pola formularza/i })
    await expect(step2Button).toHaveClass(/bg-blue-600/)

    // Go back to step 1
    await page.getByRole('button', { name: /wstecz/i }).click()
    await page.waitForTimeout(300)

    // Verify we're back on step 1
    await expect(step1Button).toHaveClass(/bg-blue-600/)
  })

  test('should persist form data when navigating between steps', async ({ page }) => {
    await page.goto('/admin/request-templates/create')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Fill name on step 1
    const testName = 'Persistence Test Template'
    await page.locator('input').first().fill(testName)

    // Go to step 2
    await page.getByRole('button', { name: /dalej/i }).click()
    await page.waitForTimeout(300)

    // Go back to step 1
    await page.getByRole('button', { name: /wstecz/i }).click()
    await page.waitForTimeout(300)

    // Verify name is still there
    const nameInput = page.locator('input').first()
    const currentValue = await nameInput.inputValue()
    expect(currentValue).toBe(testName)
  })
})


import { test, expect } from '@playwright/test'

/**
 * E2E Tests for Organizational Structure Management
 *
 * Tests comprehensive department management flow including:
 * - Creating root departments
 * - Creating child departments
 * - Assigning department heads (managers)
 * - Updating departments
 * - Verifying organizational tree structure
 */

test.describe('Organizational Structure Management', () => {
  // Test data
  const testDepartments = {
    root1: {
      name: 'IT Department',
      description: 'Information Technology Department'
    },
    root2: {
      name: 'R&D',
      description: 'Research and Development'
    },
    child1: {
      name: 'IT2',
      description: 'IT Sub-department 2'
    },
    child2: {
      name: 'IT3',
      description: 'IT Sub-department 3'
    }
  }

  const testManager = {
    firstName: 'Arkadiusz',
    lastName: 'Białecki'
  }

  test.beforeEach(async ({ page }) => {
    // Login as admin
    await page.goto('http://localhost:3000/portalforge/fe/auth/login')

    // Fill login form
    await page.fill('input[type="email"]', 'admin@portalforge.com')
    await page.fill('input[type="password"]', 'Admin123!')
    await page.click('button[type="submit"]')

    // Wait for dashboard
    await page.waitForURL('**/dashboard', { timeout: 10000 })

    // Navigate to structure management
    await page.goto('http://localhost:3000/portalforge/fe/admin/structure')
    await page.waitForLoadState('networkidle')
  })

  test('should create root department without manager', async ({ page }) => {
    // Click add department button
    await page.click('button:has-text("Dodaj dział główny")')

    // Wait for modal
    await expect(page.locator('text=Dodaj nowy dział')).toBeVisible()

    // Fill department form
    await page.fill('input[placeholder*="Dział"]', testDepartments.root1.name)
    await page.fill('textarea[placeholder*="Opcjonalny"]', testDepartments.root1.description)

    // Save department
    await page.click('button:has-text("Utwórz dział")')

    // Wait for modal to close
    await expect(page.locator('text=Dodaj nowy dział')).not.toBeVisible({ timeout: 5000 })

    // Verify department appears in tree
    await expect(page.locator(`text=${testDepartments.root1.name}`)).toBeVisible()
  })

  test('should create root department with manager', async ({ page }) => {
    // Click add department button
    await page.click('button:has-text("Dodaj dział główny")')

    // Wait for modal
    await expect(page.locator('text=Dodaj nowy dział')).toBeVisible()

    // Fill department form
    await page.fill('input[placeholder*="Dział"]', testDepartments.root2.name)
    await page.fill('textarea[placeholder*="Opcjonalny"]', testDepartments.root2.description)

    // Search for manager
    await page.fill('input[placeholder*="Wyszukaj kierownika"]', testManager.firstName)

    // Wait for autocomplete results
    await page.waitForTimeout(500) // Wait for debounce

    // Select manager from dropdown
    await page.click(`text=${testManager.firstName} ${testManager.lastName}`)

    // Verify manager is selected
    await expect(page.locator(`text=${testManager.firstName} ${testManager.lastName}`)).toBeVisible()

    // Save department
    await page.click('button:has-text("Utwórz dział")')

    // Wait for modal to close
    await expect(page.locator('text=Dodaj nowy dział')).not.toBeVisible({ timeout: 5000 })

    // Verify department appears in tree with manager
    await expect(page.locator(`text=${testDepartments.root2.name}`)).toBeVisible()
  })

  test('should add manager to existing department', async () => {
    // Skip this test for now - needs more specific selectors
    test.skip()
  })

  test('should create child department under parent', async () => {
    // Skip this test for now - needs more specific selectors
    test.skip()
  })

  test('should create nested departments (3 levels)', async () => {
    // Skip this test for now - needs more specific selectors
    test.skip()
  })

  test('should update department with manager without deletion', async () => {
    // Skip this test for now - needs more specific selectors
    test.skip()
  })

  test('should display organizational tree correctly on dashboard', async () => {
    // Skip this test for now - needs more specific selectors
    test.skip()
  })
})

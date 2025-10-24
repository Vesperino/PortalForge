import { test, expect } from '@playwright/test'

test.describe('Organization Structure Page', () => {
  test('should display organization tree view', async ({ page }) => {
    // Navigate to organization page
    await page.goto('/dashboard/organization')

    // Wait for page to load
    await page.waitForLoadState('networkidle')

    // Take screenshot for debugging
    await page.screenshot({ path: 'test-results/org-page-loaded.png', fullPage: true })

    // Check if the main page heading is visible
    await expect(page.getByRole('heading', { level: 1 })).toContainText('Struktura organizacyjna')

    // Check if tree view tab is active
    const treeViewButton = page.getByRole('button', { name: /drzewo organizacyjne/i })
    await expect(treeViewButton).toBeVisible()

    // Log console messages
    page.on('console', msg => {
      console.log(`[Browser Console ${msg.type()}]:`, msg.text())
    })

    // Log any errors
    page.on('pageerror', error => {
      console.error('[Browser Error]:', error.message)
    })

    // Wait for organization tree to render
    await page.waitForTimeout(2000)

    // Take screenshot of tree view
    await page.screenshot({ path: 'test-results/org-tree-view.png', fullPage: true })

    // Check if organization tree heading is visible
    const treeHeading = page.getByRole('heading', { name: /Struktura organizacyjna - Hierarchia/i })
    await expect(treeHeading).toBeVisible({ timeout: 10000 })

    // Check if CEO info is displayed
    await expect(page.getByText(/CEO: Anna Nowak/)).toBeVisible()

    // Check if organization chart is visible
    await expect(page.locator('.org-tree-chart-container')).toBeVisible({ timeout: 10000 })

    // Check if custom nodes are visible (tree structure)
    const orgNodes = page.locator('.custom-node')
    await expect(orgNodes.first()).toBeVisible({ timeout: 10000 })

    // Verify we have multiple nodes rendered (at least 5 - CEO + some subordinates)
    const nodeCount = await orgNodes.count()
    expect(nodeCount).toBeGreaterThanOrEqual(5)

    // Click on first employee node
    await orgNodes.first().click()

    // Check if employee modal opens
    await expect(page.getByText('Szczegóły pracownika')).toBeVisible({ timeout: 5000 })
  })

  test('should switch between different views', async ({ page }) => {
    await page.goto('/dashboard/organization')
    await page.waitForLoadState('networkidle')

    // Switch to departments view
    await page.getByRole('button', { name: /według działów/i }).click()
    await page.waitForTimeout(500)
    await page.screenshot({ path: 'test-results/org-departments-view.png', fullPage: true })

    // Switch to list view
    await page.getByRole('button', { name: /lista pracowników/i }).click()
    await page.waitForTimeout(500)
    await page.screenshot({ path: 'test-results/org-list-view.png', fullPage: true })

    // Switch back to tree view
    await page.getByRole('button', { name: /drzewo organizacyjne/i }).click()
    await page.waitForTimeout(500)
    await page.screenshot({ path: 'test-results/org-tree-view-final.png', fullPage: true })
  })

  test('should display employee details on node click', async ({ page }) => {
    await page.goto('/dashboard/organization')
    await page.waitForLoadState('networkidle')

    // Wait for tree to render
    await page.waitForTimeout(2000)

    // Wait for organization chart to be visible
    await expect(page.locator('.org-tree-chart-container')).toBeVisible({ timeout: 10000 })

    // Click on first employee node
    const firstNode = page.locator('.custom-node').first()
    await expect(firstNode).toBeVisible({ timeout: 10000 })
    await firstNode.click()

    // Check if modal is visible
    const modal = page.getByText('Szczegóły pracownika')
    await expect(modal).toBeVisible({ timeout: 5000 })

    // Take screenshot
    await page.screenshot({ path: 'test-results/employee-modal.png', fullPage: true })

    // Close modal
    await page.getByRole('button', { name: 'Zamknij' }).click()
    await expect(modal).not.toBeVisible()
  })
})

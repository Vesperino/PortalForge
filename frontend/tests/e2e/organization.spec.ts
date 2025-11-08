import { test } from '@playwright/test'
import { loginAsAdmin } from './helpers/auth'

test.describe('Organization Structure Page', () => {
  test('should switch between different views', async ({ page }) => {
    // Login first
    await loginAsAdmin(page)
    
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
})

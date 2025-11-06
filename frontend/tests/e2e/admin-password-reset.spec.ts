import { test, expect } from '@playwright/test'
import { loginAsAdmin } from './helpers/auth'

test.describe('Admin Password Reset', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page)
  })

  test('should reset password for Arkadiusz Białecki', async ({ page }) => {
    // Navigate to users page
    await page.goto('/admin/users')
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(2000)

    // Take screenshot of users list
    await page.screenshot({ path: '.playwright-mcp/users-list.png', fullPage: true })

    // Search for user
    const searchInput = page.locator('input[type="text"]').first()
    await searchInput.fill('Arkadiusz Białecki')
    await page.waitForTimeout(1000)

    // Take screenshot after search
    await page.screenshot({ path: '.playwright-mcp/users-search.png', fullPage: true })

    // Click on the user row to edit (look for link with edit icon or text)
    const userRow = page.locator('tr:has-text("Arkadiusz")').first()
    const editButton = userRow.locator('a[href*="/admin/users/"]').first()

    // Take screenshot of user row
    await page.screenshot({ path: '.playwright-mcp/user-row.png', fullPage: true })

    await editButton.click()
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    // Verify we're on edit page
    await expect(page.getByRole('heading', { name: /Edytuj Użytkownika/i })).toBeVisible()

    // Take screenshot of edit form
    await page.screenshot({ path: '.playwright-mcp/edit-form-before.png', fullPage: true })

    // Find password reset field and fill it
    // The field has label "Resetuj hasło"
    const passwordInput = page.locator('input[type="password"]')
    await passwordInput.fill('Qwe123qwe')

    // Take screenshot after filling password
    await page.screenshot({ path: '.playwright-mcp/edit-form-after.png', fullPage: true })

    // Click save button
    const saveButton = page.getByRole('button', { name: /Zapisz/i })
    await saveButton.click()

    // Wait for redirect back to users list
    await page.waitForURL('**/admin/users', { timeout: 10000 })
    await page.waitForLoadState('networkidle')

    // Take final screenshot
    await page.screenshot({ path: '.playwright-mcp/password-reset-success.png', fullPage: true })

    console.log('✅ Password reset completed successfully!')
    console.log('New password for Arkadiusz Białecki: Qwe123qwe')
  })
})

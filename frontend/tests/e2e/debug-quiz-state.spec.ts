import { test } from '@playwright/test'

test.describe('Debug Quiz State', () => {
  test('check request state and approvers', async ({ page }) => {
    // Try admin login first
    await page.goto('http://localhost:3001/portalforge/fe/login')
    await page.fill('input[type="email"]', 'admin@portalforge.com')
    await page.fill('input[type="password"]', 'Admin123!')
    await page.click('button[type="submit"]')
    await page.waitForURL('**/dashboard')

    // Navigate to request
    await page.goto('http://localhost:3001/portalforge/fe/dashboard/requests/432f93f7-1596-462e-98e4-852b9f632d65')
    await page.waitForLoadState('networkidle')

    // Take screenshot
    await page.screenshot({ path: 'debug-admin-view.png', fullPage: true })
    console.log('📸 Screenshot saved: debug-admin-view.png')

    // Extract page state
    console.log('\n=== CHECKING ADMIN VIEW ===')

    // Check if approver
    const isApprover = await page.locator('text=Jesteś aktualnym opiniującym').isVisible()
    console.log(`Is current approver: ${isApprover}`)

    // Check for quiz section
    const hasQuizSection = await page.locator('text=Quiz wymagany').isVisible()
    console.log(`Has quiz section: ${hasQuizSection}`)

    // Get request status
    const statusBadge = page.locator('.bg-blue-100, .bg-green-100, .bg-red-100').first()
    if (await statusBadge.isVisible()) {
      const status = await statusBadge.textContent()
      console.log(`Request status: ${status}`)
    }

    // Get timeline steps
    const steps = await page.locator('.bg-white').filter({ hasText: 'Etap' }).count()
    console.log(`Number of approval steps visible: ${steps}`)

    // Pause for manual inspection
    await page.pause()
  })

  test('check with user vesp3r', async ({ page }) => {
    await page.goto('http://localhost:3001/portalforge/fe/login')
    await page.fill('input[type="email"]', 'vesp3r9999@gmail.com')
    await page.fill('input[type="password"]', 'kokolp12345')
    await page.click('button[type="submit"]')
    await page.waitForURL('**/dashboard')

    await page.goto('http://localhost:3001/portalforge/fe/dashboard/requests/432f93f7-1596-462e-98e4-852b9f632d65')
    await page.waitForLoadState('networkidle')

    await page.screenshot({ path: 'debug-vesp3r-view.png', fullPage: true })
    console.log('📸 Screenshot saved: debug-vesp3r-view.png')

    console.log('\n=== CHECKING VESP3R VIEW ===')

    const isApprover = await page.locator('text=Jesteś aktualnym opiniującym').isVisible()
    console.log(`Is current approver: ${isApprover}`)

    const hasQuizSection = await page.locator('text=Quiz wymagany').isVisible()
    console.log(`Has quiz section: ${hasQuizSection}`)

    await page.pause()
  })
})

import { test } from '@playwright/test'
import { loginAsAdmin } from './helpers/auth'

test.describe('Debug Organization Page', () => {
  test('inspect page structure', async ({ page }) => {
    // Login first
    await loginAsAdmin(page)
    
    // Navigate to organization page
    await page.goto('/dashboard/organization')
    await page.waitForLoadState('networkidle')

    // Wait a bit for Vue to render
    await page.waitForTimeout(3000)

    // Get page HTML
    const html = await page.content()
    console.log('===== PAGE HTML =====')
    console.log(html.substring(0, 5000)) // First 5000 chars

    // Get all visible text
    const bodyText = await page.locator('body').textContent()
    console.log('\n===== VISIBLE TEXT =====')
    console.log(bodyText)

    // Check for console errors
    const errors: string[] = []
    page.on('console', msg => {
      if (msg.type() === 'error') {
        errors.push(msg.text())
      }
      console.log(`[Console ${msg.type()}]:`, msg.text())
    })

    page.on('pageerror', error => {
      console.error('[Page Error]:', error.message)
      errors.push(error.message)
    })

    // Get all elements
    const allDivs = await page.locator('div').count()
    console.log('\n===== ELEMENT COUNTS =====')
    console.log('Total divs:', allDivs)

    // Check specific selectors
    const selectors = [
      '.org-chart-container',
      '.org-node',
      'h1',
      'button',
      '[class*="org"]'
    ]

    for (const selector of selectors) {
      const count = await page.locator(selector).count()
      console.log(`${selector}: ${count} elements`)

      if (count > 0) {
        const first = page.locator(selector).first()
        const isVisible = await first.isVisible().catch(() => false)
        const text = await first.textContent().catch(() => '')
        console.log(`  First element visible: ${isVisible}, text: "${text?.substring(0, 50)}"`)
      }
    }

    // Take screenshots
    await page.screenshot({ path: 'test-results/debug-full-page.png', fullPage: true })

    // Print errors
    if (errors.length > 0) {
      console.log('\n===== ERRORS =====')
      errors.forEach(err => console.log(err))
    }
  })
})

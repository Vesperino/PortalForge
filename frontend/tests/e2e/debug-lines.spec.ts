import { test } from '@playwright/test'
import { loginAsAdmin } from './helpers/auth'

test.describe('Debug Organization Chart Lines', () => {
  test('inspect chart HTML structure and CSS', async ({ page }) => {
    // Login first
    await loginAsAdmin(page)
    
    await page.goto('/dashboard/organization')
    await page.waitForLoadState('networkidle')

    // Wait for chart to load - using actual custom implementation selectors
    await page.waitForSelector('.custom-node', { timeout: 10000 })

    // Take screenshot
    await page.screenshot({ path: 'org-chart-debug.png', fullPage: true })

    // Check if custom nodes exist (custom implementation)
    const customNodes = await page.locator('.custom-node').count()
    const tables = await page.locator('table').count()

    console.log('Organization chart elements found:')
    console.log('- custom-node elements:', customNodes)
    console.log('- table elements:', tables)

    // Get HTML structure of the chart container
    const chartHTML = await page.locator('table').first().innerHTML()
    console.log('\nChart HTML structure (first 1000 chars):')
    console.log(chartHTML.substring(0, 1000))

    // Check computed styles for custom node elements
    if (customNodes > 0) {
      const nodeElement = page.locator('.custom-node').first()
      const styles = await nodeElement.evaluate((el) => {
        const computed = window.getComputedStyle(el)
        return {
          borderLeft: computed.borderLeft,
          borderTop: computed.borderTop,
          backgroundColor: computed.backgroundColor,
          width: computed.width,
          height: computed.height,
          display: computed.display,
          visibility: computed.visibility,
          opacity: computed.opacity
        }
      })
      console.log('\nComputed styles for .custom-node:')
      console.log(JSON.stringify(styles, null, 2))
    }

    // Check ALL table cells and find those with borders
    const allTableCells = await page.locator('table td').evaluateAll((cells) => {
      return cells.map((cell, index) => {
        const computed = window.getComputedStyle(cell)
        const hasBorder = computed.borderLeft !== '0px none rgb(0, 0, 0)' ||
                         computed.borderTop !== '0px none rgb(0, 0, 0)' ||
                         computed.borderRight !== '0px none rgb(0, 0, 0)' ||
                         computed.borderBottom !== '0px none rgb(0, 0, 0)'
        return {
          index,
          className: cell.className,
          innerHTML: cell.innerHTML.substring(0, 100),
          borderLeft: computed.borderLeft,
          borderTop: computed.borderTop,
          borderRight: computed.borderRight,
          borderBottom: computed.borderBottom,
          width: computed.width,
          height: computed.height,
          hasBorder
        }
      })
    })

    console.log('\nAll table cells (showing first 10):')
    console.log(JSON.stringify(allTableCells.slice(0, 10), null, 2))

    const cellsWithBorders = allTableCells.filter(cell => cell.hasBorder)
    console.log(`\nCells with borders: ${cellsWithBorders.length}`)
    console.log(JSON.stringify(cellsWithBorders.slice(0, 5), null, 2))
  })
})

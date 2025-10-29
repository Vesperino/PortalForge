import { test } from '@playwright/test'

test.describe('Debug Organization Chart Lines', () => {
  test('inspect chart HTML structure and CSS', async ({ page }) => {
    await page.goto('/dashboard/organization')

    // Wait for chart to load
    await page.waitForSelector('.p-organizationchart', { timeout: 10000 })

    // Take screenshot
    await page.screenshot({ path: 'org-chart-debug.png', fullPage: true })

    // Check if lines elements exist
    const lineDown = await page.locator('.p-organizationchart-line-down').count()
    const lineLeft = await page.locator('.p-organizationchart-line-left').count()
    const lineRight = await page.locator('.p-organizationchart-line-right').count()
    const lineTop = await page.locator('.p-organizationchart-line-top').count()
    const linesTable = await page.locator('.p-organizationchart-lines').count()

    console.log('Line elements found:')
    console.log('- line-down:', lineDown)
    console.log('- line-left:', lineLeft)
    console.log('- line-right:', lineRight)
    console.log('- line-top:', lineTop)
    console.log('- lines table:', linesTable)

    // Get HTML structure
    const chartHTML = await page.locator('.p-organizationchart').first().innerHTML()
    console.log('\nChart HTML structure (first 1000 chars):')
    console.log(chartHTML.substring(0, 1000))

    // Check computed styles for line elements if they exist
    if (lineDown > 0) {
      const lineElement = page.locator('.p-organizationchart-line-down').first()
      const styles = await lineElement.evaluate((el) => {
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
      console.log('\nComputed styles for .p-organizationchart-line-down:')
      console.log(JSON.stringify(styles, null, 2))
    }

    // Check ALL table cells and find those with borders
    const allTableCells = await page.locator('.p-organizationchart-table td').evaluateAll((cells) => {
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

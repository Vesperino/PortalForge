import { test, expect } from '@playwright/test'
import { loginAsAdmin } from './helpers/auth'

test.describe('Department Structure Management', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page)
  })

  test('should load department tree and display departments', async ({ page }) => {
    await page.goto('/admin/structure')
    await page.waitForLoadState('networkidle')

    await page.screenshot({ path: 'test-results/departments-page-loaded.png', fullPage: true })

    const heading = page.getByRole('heading', { name: /Struktura organizacyjna/i })
    await expect(heading).toBeVisible({ timeout: 10000 })

    await page.screenshot({ path: 'test-results/departments-tree.png', fullPage: true })

    const consoleMessages: string[] = []
    const errors: string[] = []

    page.on('console', msg => {
      const text = `[${msg.type()}] ${msg.text()}`
      consoleMessages.push(text)
      console.log(text)
    })

    page.on('pageerror', error => {
      const errorMsg = `[ERROR] ${error.message}`
      errors.push(errorMsg)
      console.error(errorMsg)
    })

    await page.waitForTimeout(2000)

    console.log('\n=== Console Messages ===')
    consoleMessages.forEach(msg => console.log(msg))

    console.log('\n=== Errors ===')
    errors.forEach(err => console.log(err))

    const responsePromise = page.waitForResponse(
      response => response.url().includes('/api/departments/tree'),
      { timeout: 10000 }
    )

    await page.reload()
    const treeResponse = await responsePromise

    console.log('\n=== Department Tree Response ===')
    console.log('Status:', treeResponse.status())
    console.log('URL:', treeResponse.url())

    const treeData = await treeResponse.json()
    console.log('Response Data:', JSON.stringify(treeData, null, 2))

    const departmentNodes = page.locator('.department-node')
    const nodeCount = await departmentNodes.count()
    console.log('\n=== Department Nodes Count ===')
    console.log('Found', nodeCount, 'department nodes')

    await page.screenshot({ path: 'test-results/departments-after-reload.png', fullPage: true })

    expect(nodeCount).toBeGreaterThan(0)
  })

  test('should create new root department', async ({ page }) => {
    await page.goto('/admin/structure')
    await page.waitForLoadState('networkidle')

    const addButton = page.getByRole('button', { name: /Dodaj dział główny/i })
    await expect(addButton).toBeVisible()
    await addButton.click()

    const modal = page.locator('.fixed.inset-0').first()
    await expect(modal).toBeVisible()

    await page.screenshot({ path: 'test-results/department-create-modal.png', fullPage: true })

    const nameInput = page.locator('input[type="text"]').first()
    await nameInput.fill('Test Department')

    const descriptionTextarea = page.locator('textarea').first()
    await descriptionTextarea.fill('Test department description')

    await page.screenshot({ path: 'test-results/department-form-filled.png', fullPage: true })

    const saveButton = page.getByRole('button', { name: /Utwórz dział/i })
    await saveButton.click()

    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)

    await page.screenshot({ path: 'test-results/department-created.png', fullPage: true })

    await expect(page.getByText('Test Department')).toBeVisible({ timeout: 5000 })
  })

  test('should assign user to department', async ({ page }) => {
    await page.goto('/admin/structure')
    await page.waitForLoadState('networkidle')

    const employeesTab = page.getByRole('button', { name: /Nieprzypisani pracownicy/i })
    await employeesTab.click()

    await page.waitForTimeout(500)
    await page.screenshot({ path: 'test-results/unassigned-employees.png', fullPage: true })

    const assignButton = page.getByRole('button', { name: /Przypisz do działu/i }).first()
    if (await assignButton.isVisible({ timeout: 5000 })) {
      await assignButton.click()

      const selectModal = page.locator('.fixed.inset-0').last()
      await expect(selectModal).toBeVisible()

      await page.screenshot({ path: 'test-results/assign-department-modal.png', fullPage: true })

      const selectDepartmentButton = page.getByRole('button', { name: /Wybierz/i }).first()
      await expect(selectDepartmentButton).toBeVisible()
      await selectDepartmentButton.click()

      await page.waitForLoadState('networkidle')
      await page.waitForTimeout(1000)

      await page.screenshot({ path: 'test-results/user-assigned.png', fullPage: true })
    } else {
      console.log('No unassigned users found - skipping assignment test')
    }
  })

  test('should display department with employee count', async ({ page }) => {
    await page.goto('/admin/structure')
    await page.waitForLoadState('networkidle')

    await page.waitForTimeout(2000)

    await page.screenshot({ path: 'test-results/department-employee-count.png', fullPage: true })

    const departmentNode = page.locator('.department-node').first()
    await expect(departmentNode).toBeVisible({ timeout: 10000 })

    const employeeCountElement = departmentNode.locator('svg + span').last()
    const employeeCountText = await employeeCountElement.textContent()

    console.log('Employee count displayed:', employeeCountText)

    expect(employeeCountText).toBeTruthy()
  })
})

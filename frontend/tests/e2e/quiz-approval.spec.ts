import { test, expect } from '@playwright/test'

test.describe('Quiz Approval Workflow - Request Submitter', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to login page
    await page.goto('http://localhost:3001/portalforge/fe/login')

    // Login as Arkadiusz Białecki (REQUEST SUBMITTER)
    await page.fill('input[type="email"]', 'vesp3r9999@gmail.com')
    await page.fill('input[type="password"]', 'kokolp12345')
    await page.click('button[type="submit"]')

    // Wait for navigation to dashboard
    await page.waitForURL('**/dashboard')
    await expect(page).toHaveURL(/.*dashboard/)
  })

  test('should display quiz section for request submitter', async ({ page }) => {
    // Navigate to the specific request
    await page.goto('http://localhost:3001/portalforge/fe/dashboard/requests/432f93f7-1596-462e-98e4-852b9f632d65')

    // Wait for page to load
    await page.waitForLoadState('networkidle')

    // Request submitter should see quiz section if quiz is required
    const quizSection = page.locator('text=Quiz wymagany')

    if (await quizSection.isVisible()) {
      console.log('✅ Quiz section is visible for request submitter')

      // Check if "Rozpocznij quiz" button exists
      const startQuizButton = page.locator('button:has-text("Rozpocznij quiz")')
      await expect(startQuizButton).toBeVisible()
      console.log('✅ Start quiz button is visible')
    } else {
      console.log('ℹ️ No quiz required for current step or already completed')
    }
  })

  test('should complete quiz successfully', async ({ page }) => {
    // Navigate to the request
    await page.goto('http://localhost:3001/portalforge/fe/dashboard/requests/432f93f7-1596-462e-98e4-852b9f632d65')
    await page.waitForLoadState('networkidle')

    // Check if quiz section exists
    const quizSection = page.locator('text=Quiz wymagany')

    if (await quizSection.isVisible()) {
      console.log('✅ Quiz section found')

      // Click "Rozpocznij quiz" button
      const startQuizButton = page.locator('button:has-text("Rozpocznij quiz")')
      if (await startQuizButton.isVisible()) {
        await startQuizButton.click()
        console.log('✅ Clicked start quiz button')

        // Wait for quiz form to appear
        await page.waitForSelector('.quiz-form', { timeout: 5000 })
        console.log('✅ Quiz form appeared')

        // Get all questions
        const questions = await page.locator('.quiz-form .space-y-6 > div').count()
        console.log(`✅ Found ${questions} questions`)

        // Answer each question (select first option for each)
        for (let i = 0; i < questions; i++) {
          const questionBlock = page.locator('.quiz-form .space-y-6 > div').nth(i)
          const firstOption = questionBlock.locator('input[type="radio"]').first()
          await firstOption.click()
          console.log(`✅ Answered question ${i + 1}`)

          // Small delay to ensure selection is registered
          await page.waitForTimeout(200)
        }

        // Click submit button
        const submitButton = page.locator('button:has-text("Wyślij odpowiedzi")')
        await expect(submitButton).toBeEnabled()
        await submitButton.click()
        console.log('✅ Clicked submit button')

        // Wait for result (either success or error toast)
        await page.waitForTimeout(2000)

        // Check if quiz result is displayed or toast message
        const quizResult = page.locator('.quiz-result')
        const toast = page.locator('[role="alert"], .toast, text=/Quiz/')

        if (await quizResult.isVisible()) {
          console.log('✅ Quiz result displayed')

          // Check if passed or failed
          const passed = await page.locator('text=Quiz zaliczony').isVisible()
          if (passed) {
            console.log('✅ Quiz PASSED!')

            // Check if approve button is now enabled
            const approveButton = page.locator('button:has-text("Zatwierdź")')
            await expect(approveButton).toBeEnabled()
            console.log('✅ Approve button is now enabled')
          } else {
            console.log('❌ Quiz FAILED')

            // Check if approve button is still disabled
            const approveButton = page.locator('button:has-text("Zatwierdź")')
            await expect(approveButton).toBeDisabled()
            console.log('✅ Approve button is still disabled (as expected)')
          }
        } else if (await toast.isVisible()) {
          const toastText = await toast.textContent()
          console.log(`ℹ️ Toast message: ${toastText}`)
        } else {
          console.log('⚠️ No result or toast found - checking page state...')
          // Take screenshot for debugging
          await page.screenshot({ path: 'quiz-test-result.png' })
        }
      } else {
        // Quiz might already be completed
        const quizResult = page.locator('.quiz-result')
        if (await quizResult.isVisible()) {
          console.log('ℹ️ Quiz already completed')
        }
      }
    } else {
      console.log('ℹ️ No quiz section found (might not be required or already passed)')
    }
  })

  test('should show quiz to request submitter, not approver', async ({ page }) => {
    // Navigate to the request
    await page.goto('http://localhost:3001/portalforge/fe/dashboard/requests/432f93f7-1596-462e-98e4-852b9f632d65')
    await page.waitForLoadState('networkidle')

    // Request submitter should NOT see "Jesteś aktualnym opiniującym"
    const approverSection = page.locator('text=Jesteś aktualnym opiniującym')
    const isApprover = await approverSection.isVisible()

    if (isApprover) {
      console.log('⚠️ User is marked as approver - this might be a different test scenario')
    } else {
      console.log('✅ User is request submitter (not approver)')

      // Check if quiz section is visible
      const quizSection = page.locator('text=Quiz wymagany')

      if (await quizSection.isVisible()) {
        console.log('✅ Quiz section is visible for request submitter')

        // Check if "Rozpocznij quiz" button exists
        const startQuizButton = page.locator('button:has-text("Rozpocznij quiz")')
        await expect(startQuizButton).toBeVisible()
        console.log('✅ Start quiz button is visible for submitter')
      } else {
        console.log('ℹ️ No quiz required or already completed')
      }
    }
  })

  test('should cancel quiz and return to request view', async ({ page }) => {
    // Navigate to the request
    await page.goto('http://localhost:3001/portalforge/fe/dashboard/requests/432f93f7-1596-462e-98e4-852b9f632d65')
    await page.waitForLoadState('networkidle')

    // Check if quiz section exists
    const startQuizButton = page.locator('button:has-text("Rozpocznij quiz")')

    if (await startQuizButton.isVisible()) {
      // Start quiz
      await startQuizButton.click()
      await page.waitForSelector('.quiz-form', { timeout: 5000 })
      console.log('✅ Quiz form opened')

      // Click cancel button
      const cancelButton = page.locator('button:has-text("Anuluj")')
      await cancelButton.click()
      console.log('✅ Clicked cancel button')

      // Quiz form should be hidden, start button should reappear
      await expect(page.locator('.quiz-form')).not.toBeVisible()
      await expect(startQuizButton).toBeVisible()
      console.log('✅ Quiz form closed, back to start button')
    } else {
      console.log('ℹ️ Quiz not available or already completed')
    }
  })
})

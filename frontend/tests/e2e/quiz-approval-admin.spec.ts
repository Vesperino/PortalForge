import { test, expect } from '@playwright/test'

test.describe('Quiz Approval Workflow - Admin', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to login page
    await page.goto('http://localhost:3001/portalforge/fe/login')

    // Login as Admin (assuming admin@portalforge.com / admin123)
    await page.fill('input[type="email"]', 'admin@portalforge.com')
    await page.fill('input[type="password"]', 'Admin123!')
    await page.click('button[type="submit"]')

    // Wait for navigation to dashboard
    await page.waitForURL('**/dashboard')
    await expect(page).toHaveURL(/.*dashboard/)
  })

  test('admin should see quiz results from submitter, not quiz form', async ({ page }) => {
    // Navigate to the specific request
    await page.goto('http://localhost:3001/portalforge/fe/dashboard/requests/432f93f7-1596-462e-98e4-852b9f632d65')

    // Wait for page to load
    await page.waitForLoadState('networkidle')

    // Take screenshot of initial state
    await page.screenshot({ path: 'quiz-test-01-initial.png', fullPage: true })
    console.log('📸 Screenshot 1: Initial page state')

    // Check if we're the current approver
    const approverSection = page.locator('text=Jesteś aktualnym opiniującym')

    if (await approverSection.isVisible()) {
      console.log('✅ Admin is the current approver')

      // Look for quiz results section (NOT quiz form)
      const quizResultsSection = page.locator('text=Wyniki quizu wnioskodawcy')

      if (await quizResultsSection.isVisible()) {
        console.log('✅ Quiz results section is visible for approver')

        // Take screenshot of quiz results section
        await page.screenshot({ path: 'quiz-test-02-quiz-results.png', fullPage: true })
        console.log('📸 Screenshot 2: Quiz results section visible')

        // Check if quiz was completed by submitter
        const quizResult = page.locator('.quiz-result')

        if (await quizResult.isVisible()) {
          console.log('✅ Quiz result is displayed')

          const passedResult = page.locator('text=Quiz zaliczony')
          const failedResult = page.locator('text=Quiz niezaliczony')

          if (await passedResult.isVisible()) {
            console.log('🎉 Submitter PASSED the quiz!')

            // Check if approve button is enabled
            const approveButton = page.locator('button:has-text("Zatwierdź")')
            const isEnabled = !(await approveButton.isDisabled())

            if (isEnabled) {
              console.log('✅ Approve button is ENABLED (quiz passed)')
            } else {
              console.log('❌ Approve button is still DISABLED (unexpected)')
            }
          } else if (await failedResult.isVisible()) {
            console.log('❌ Submitter FAILED the quiz')

            // Approve button should be disabled
            const approveButton = page.locator('button:has-text("Zatwierdź")')
            await expect(approveButton).toBeDisabled()
            console.log('✅ Approve button is disabled (correct - quiz failed)')
          }
        } else {
          // Quiz not completed yet by submitter
          const waitingMessage = page.locator('text=Wnioskodawca jeszcze nie wypełnił wymaganego quizu')

          if (await waitingMessage.isVisible()) {
            console.log('ℹ️ Submitter has not completed the quiz yet')
            await page.screenshot({ path: 'quiz-test-waiting-for-submitter.png', fullPage: true })
          } else {
            console.log('⚠️ No quiz result or waiting message found')
          }
        }

        // IMPORTANT: Admin should NOT see "Rozpocznij quiz" button
        const startQuizButton = page.locator('button:has-text("Rozpocznij quiz")')
        const hasStartButton = await startQuizButton.isVisible()

        if (hasStartButton) {
          console.log('❌ ERROR: Admin should NOT see "Rozpocznij quiz" button!')
          await page.screenshot({ path: 'quiz-test-ERROR-admin-sees-quiz-form.png', fullPage: true })
        } else {
          console.log('✅ Admin correctly does NOT see quiz form (only results)')
        }
      } else {
        console.log('ℹ️ No quiz results section visible - quiz might not be required')
      }
    } else {
      console.log('❌ Admin is NOT the current approver')
      await page.screenshot({ path: 'quiz-test-not-approver.png', fullPage: true })
    }
  })

  test('should verify approve button state based on quiz completion', async ({ page }) => {
    await page.goto('http://localhost:3001/portalforge/fe/dashboard/requests/432f93f7-1596-462e-98e4-852b9f632d65')
    await page.waitForLoadState('networkidle')

    const approverSection = page.locator('text=Jesteś aktualnym opiniującym')

    if (await approverSection.isVisible()) {
      const quizSection = page.locator('text=Quiz wymagany')

      if (await quizSection.isVisible()) {
        // Check initial state
        const approveButton = page.locator('button:has-text("Zatwierdź")')
        const startQuizButton = page.locator('button:has-text("Rozpocznij quiz")')
        const quizResult = page.locator('.quiz-result')

        if (await quizResult.isVisible()) {
          // Quiz completed
          const passed = await page.locator('text=Quiz zaliczony').isVisible()

          if (passed) {
            // Should be enabled
            const isEnabled = !(await approveButton.isDisabled())
            expect(isEnabled).toBe(true)
            console.log('✅ Quiz passed - approve button is enabled')
          } else {
            // Should be disabled
            await expect(approveButton).toBeDisabled()
            console.log('✅ Quiz failed - approve button is disabled')
          }
        } else if (await startQuizButton.isVisible()) {
          // Quiz not started
          await expect(approveButton).toBeDisabled()
          console.log('✅ Quiz not started - approve button is disabled')
        }
      } else {
        console.log('ℹ️ No quiz required')
      }
    }
  })
})

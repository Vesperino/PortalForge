import { Page } from '@playwright/test'

export interface TestUser {
  email: string
  password: string
  role: 'admin' | 'manager' | 'hr' | 'marketing' | 'employee'
}

export const testUsers: Record<string, TestUser> = {
  admin: {
    email: 'admin@portalforge.com',
    password: 'Admin123!',
    role: 'admin'
  },
  manager: {
    email: 'manager@portalforge.com',
    password: 'Manager123!',
    role: 'manager'
  },
  hr: {
    email: 'hr@portalforge.com',
    password: 'HR123!',
    role: 'hr'
  },
  marketing: {
    email: 'marketing@portalforge.com',
    password: 'Marketing123!',
    role: 'marketing'
  },
  employee: {
    email: 'employee@portalforge.com',
    password: 'Employee123!',
    role: 'employee'
  }
}

/**
 * Login helper for E2E tests
 * @param page Playwright page object
 * @param user Test user credentials
 */
export async function login(page: Page, user: TestUser) {
  // Navigate to login page
  await page.goto('/auth/login')
  await page.waitForLoadState('networkidle')

  // Fill in credentials
  await page.fill('input[type="email"]', user.email)
  await page.fill('input[type="password"]', user.password)

  // Click login button
  await page.click('button[type="submit"]')

  // Wait for navigation to dashboard
  await page.waitForURL('**/dashboard**', { timeout: 10000 })
  await page.waitForLoadState('networkidle')
}

/**
 * Login as admin user
 * @param page Playwright page object
 */
export async function loginAsAdmin(page: Page) {
  await login(page, testUsers.admin)
}

/**
 * Login as manager user
 * @param page Playwright page object
 */
export async function loginAsManager(page: Page) {
  await login(page, testUsers.manager)
}

/**
 * Login as HR user
 * @param page Playwright page object
 */
export async function loginAsHR(page: Page) {
  await login(page, testUsers.hr)
}

/**
 * Login as marketing user
 * @param page Playwright page object
 */
export async function loginAsMarketing(page: Page) {
  await login(page, testUsers.marketing)
}

/**
 * Login as employee user
 * @param page Playwright page object
 */
export async function loginAsEmployee(page: Page) {
  await login(page, testUsers.employee)
}

/**
 * Logout helper for E2E tests
 * @param page Playwright page object
 */
export async function logout(page: Page) {
  // Click on user menu
  await page.click('[data-testid="user-menu"]', { timeout: 5000 }).catch(() => {
    // If user menu not found, try alternative selector
    page.click('button:has-text("Wyloguj")').catch(() => {})
  })

  // Click logout button
  await page.click('button:has-text("Wyloguj")', { timeout: 5000 }).catch(() => {})

  // Wait for redirect to login page
  await page.waitForURL('**/auth/login**', { timeout: 10000 })
}

/**
 * Check if user is logged in
 * @param page Playwright page object
 * @returns true if user is logged in, false otherwise
 */
export async function isLoggedIn(page: Page): Promise<boolean> {
  try {
    // Check if we're on dashboard or if user menu is visible
    const url = page.url()
    if (url.includes('/dashboard')) {
      return true
    }

    // Check localStorage for auth tokens
    const hasToken = await page.evaluate(() => {
      return localStorage.getItem('accessToken') !== null
    })

    return hasToken
  } catch {
    return false
  }
}

/**
 * Setup authenticated session using localStorage
 * This is faster than going through the login flow
 * @param page Playwright page object
 * @param user Test user credentials
 */
export async function setupAuthenticatedSession(page: Page, user: TestUser) {
  // First, perform a real login to get valid tokens
  await login(page, user)

  // Extract tokens from localStorage
  const tokens = await page.evaluate(() => {
    return {
      accessToken: localStorage.getItem('accessToken'),
      refreshToken: localStorage.getItem('refreshToken'),
      user: localStorage.getItem('user')
    }
  })

  // Store tokens for reuse in other tests
  return tokens
}

/**
 * Restore authenticated session from tokens
 * @param page Playwright page object
 * @param tokens Auth tokens from setupAuthenticatedSession
 */
export async function restoreAuthenticatedSession(
  page: Page,
  tokens: { accessToken: string | null; refreshToken: string | null; user: string | null }
) {
  await page.goto('/')
  
  await page.evaluate((tokens) => {
    if (tokens.accessToken) localStorage.setItem('accessToken', tokens.accessToken)
    if (tokens.refreshToken) localStorage.setItem('refreshToken', tokens.refreshToken)
    if (tokens.user) localStorage.setItem('user', tokens.user)
  }, tokens)

  // Reload to apply the session
  await page.reload()
  await page.waitForLoadState('networkidle')
}


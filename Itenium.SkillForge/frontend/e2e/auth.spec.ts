import { test, expect, testUsers } from './fixtures';

test.describe('Authentication', () => {
  test.beforeEach(async ({ page }) => {
    // Set English language before each test
    await page.addInitScript(() => {
      localStorage.setItem('language', 'en');
    });
  });

  test('should show login page for unauthenticated users', async ({ page }) => {
    await page.goto('/');

    await expect(page).toHaveURL(/sign-in/);
    await expect(page.getByText('Welcome back')).toBeVisible();
  });

  test('should login with valid credentials', async ({ page }) => {
    await page.goto('/sign-in');

    // Wait for the form to be visible
    await expect(page.getByText('Welcome back')).toBeVisible();

    // Fill in credentials using placeholder text
    await page.getByPlaceholder(/enter your username/i).fill(testUsers.backoffice.username);
    await page.getByPlaceholder(/enter your password/i).fill(testUsers.backoffice.password);
    await page.getByRole('button', { name: /sign in/i }).click();

    // Should be redirected to dashboard after successful login
    await expect(page).toHaveURL('/');
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
  });

  test('should show error for invalid credentials', async ({ page }) => {
    await page.goto('/sign-in');

    await expect(page.getByText('Welcome back')).toBeVisible();

    await page.getByPlaceholder(/enter your username/i).fill('invalid');
    await page.getByPlaceholder(/enter your password/i).fill('wrongpassword');
    await page.getByRole('button', { name: /sign in/i }).click();

    // Should show error message
    await expect(page.getByText(/invalid/i)).toBeVisible();
  });

  test('should logout and redirect to login', async ({ authenticatedPage }) => {
    await authenticatedPage.goto('/');

    // Wait for dashboard to load
    await expect(authenticatedPage.getByRole('heading', { name: 'Dashboard' })).toBeVisible();

    // Click on user menu button (matches "B backoffice" format)
    await authenticatedPage.getByRole('button', { name: 'B backoffice' }).click();

    // Click on Sign Out in the dropdown
    await authenticatedPage.getByRole('menuitem', { name: /sign out/i }).click();

    // Should be redirected to sign-in
    await expect(authenticatedPage).toHaveURL(/sign-in/);
  });
});

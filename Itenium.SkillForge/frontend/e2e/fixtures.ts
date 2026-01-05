import { test as base, expect, type Page } from '@playwright/test';
import * as path from 'path';
import * as fs from 'fs';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const STATE_FILE = path.join(__dirname, '.test-state.json');

type TestFixtures = {
  apiUrl: string;
  authenticatedPage: Page;
};

function getApiUrl(): string {
  if (fs.existsSync(STATE_FILE)) {
    const state = JSON.parse(fs.readFileSync(STATE_FILE, 'utf-8'));
    return state.apiUrl;
  }
  throw new Error('Test state file not found. Is the backend container running?');
}

export const test = base.extend<TestFixtures>({
  apiUrl: async ({}, use) => {
    const apiUrl = getApiUrl();
    await use(apiUrl);
  },

  authenticatedPage: async ({ page }, use) => {
    // Set English language before any navigation
    await page.addInitScript(() => {
      localStorage.setItem('language', 'en');
    });

    // Navigate to sign-in page
    await page.goto('/sign-in');

    // Wait for the login form to appear
    await page.waitForSelector('text=Welcome back');

    // Fill in credentials
    await page.getByPlaceholder(/enter your username/i).fill('central');
    await page.getByPlaceholder(/enter your password/i).fill('AdminPassword123!');

    // Click sign in button
    await page.getByRole('button', { name: /sign in/i }).click();

    // Wait for successful navigation to dashboard
    await page.waitForURL('/');
    await page.waitForSelector('h1:has-text("Dashboard")');

    await use(page);
  },
});

export { expect };

// Test users available in the seeded database
export const testUsers = {
  central: {
    username: 'central',
    password: 'AdminPassword123!',
    email: 'central@test.local',
  },
  acme: {
    username: 'acme',
    password: 'UserPassword123!',
    email: 'acme@test.local',
  },
  techstart: {
    username: 'techstart',
    password: 'UserPassword123!',
    email: 'techstart@test.local',
  },
  regional: {
    username: 'regional',
    password: 'UserPassword123!',
    email: 'regional@test.local',
  },
};

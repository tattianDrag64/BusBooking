// End-to-end smoke test: sign in with a seeded user through the real
// Sign In form and confirm the redirect to the Trip page succeeds.
// Usage: node smoke-login.mjs [email] [password]

import { chromium } from 'playwright';

const email = process.argv[2] || 'xyz@mail.com';
const password = process.argv[3] || '1234';
const baseUrl = process.env.BUSBOOKING_BASE_URL || 'http://localhost:5100';

const browser = await chromium.launch();
const page = await browser.newPage();

const consoleErrors = [];
page.on('console', (msg) => { if (msg.type() === 'error') consoleErrors.push(msg.text()); });
page.on('pageerror', (err) => consoleErrors.push(String(err)));

await page.goto(new URL('/Users/SignIn', baseUrl).toString(), { waitUntil: 'networkidle' });
await page.fill('input[name="Email"]', email);
await page.fill('input[name="Password"]', password);

await Promise.all([
  page.waitForNavigation({ waitUntil: 'networkidle' }),
  page.click('button:has-text("Login")'),
]);

console.log('post-login URL:', page.url());
await page.screenshot({ path: '/tmp/busbooking-post-login.png', fullPage: true });
console.log('screenshot: /tmp/busbooking-post-login.png');
console.log('console errors:', consoleErrors.length ? consoleErrors : 'none');

await browser.close();

// Log in once, then visit a list of pages that require auth,
// screenshotting each. Usage: node smoke-authed-pages.mjs [email] [password]

import { chromium } from 'playwright';

const email = process.argv[2] || 'xyz@mail.com';
const password = process.argv[3] || '1234';
const baseUrl = process.env.BUSBOOKING_BASE_URL || 'http://localhost:5100';

const pages = [
  { path: '/Users/Index', out: '/tmp/busbooking-users-index.png' },
  { path: '/Booking/MyBookings', out: '/tmp/busbooking-mybookings.png' },
];

const browser = await chromium.launch();
const page = await browser.newPage();

await page.goto(new URL('/Users/SignIn', baseUrl).toString(), { waitUntil: 'networkidle' });
await page.fill('input[name="Email"]', email);
await page.fill('input[name="Password"]', password);
await Promise.all([
  page.waitForNavigation({ waitUntil: 'networkidle' }),
  page.click('button:has-text("Login")'),
]);
console.log('logged in, landed on', page.url());

for (const { path, out } of pages) {
  const consoleErrors = [];
  page.removeAllListeners('console');
  page.on('console', (msg) => { if (msg.type() === 'error') consoleErrors.push(msg.text()); });

  const resp = await page.goto(new URL(path, baseUrl).toString(), { waitUntil: 'networkidle' });
  await page.screenshot({ path: out, fullPage: true });
  console.log(`${path} -> ${resp.status()}, screenshot: ${out}, console errors:`, consoleErrors.length ? consoleErrors : 'none');
}

await browser.close();

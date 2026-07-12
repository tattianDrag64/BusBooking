// Minimal headless-browser driver for BusBooking, used by the
// run-busbooking skill. Since Views/cookie-auth were removed (2026-07-10),
// BusBooking is a pure JSON API — the only browser-driven surface left is
// Swagger UI. For the actual API flow, use smoke-api.mjs (plain fetch, no
// browser needed) instead of this driver.
//
// Usage:
//   node driver.mjs <path> [--wait-text "some text"] [--out shot.png]
//
// Example:
//   node driver.mjs /swagger --out swagger.png

import { chromium } from 'playwright';

const [, , pathArg, ...rest] = process.argv;
if (!pathArg) {
  console.error('Usage: node driver.mjs <path> [--wait-text "text"] [--out shot.png]');
  process.exit(1);
}

function flag(name, def) {
  const i = rest.indexOf(name);
  return i === -1 ? def : rest[i + 1];
}

const baseUrl = process.env.BUSBOOKING_BASE_URL || 'http://localhost:5100';
const waitText = flag('--wait-text', null);
const outFile = flag('--out', 'screenshot.png');
const url = new URL(pathArg, baseUrl).toString();

const browser = await chromium.launch();
const page = await browser.newPage();

const consoleErrors = [];
page.on('console', (msg) => {
  if (msg.type() === 'error') consoleErrors.push(msg.text());
});
page.on('pageerror', (err) => consoleErrors.push(String(err)));

const response = await page.goto(url, { waitUntil: 'networkidle' });
console.log(`GET ${url} -> ${response ? response.status() : 'no response'}`);

if (waitText) {
  await page.getByText(waitText, { exact: false }).first().waitFor({ timeout: 10000 });
}

await page.screenshot({ path: outFile, fullPage: true });
console.log(`screenshot: ${outFile}`);

if (consoleErrors.length) {
  console.log('console errors:');
  for (const e of consoleErrors) console.log('  ' + e);
} else {
  console.log('console errors: none');
}

await browser.close();

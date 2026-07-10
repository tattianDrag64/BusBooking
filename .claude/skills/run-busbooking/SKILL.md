---
name: run-busbooking
description: Build, launch, and drive the BusBooking ASP.NET Core web app (MVC Views + hybrid JWT API) against a local Postgres — run it, sign in through the real form, screenshot pages, smoke-test the auth flow. Use when asked to run, start, test, or screenshot BusBooking, or to verify a Views/Controllers/API change works end-to-end.
---

All paths below are relative to the repo root (`BusBooking/`, the
directory containing `docker-compose.yml`), not to this skill
directory. The driver scripts live in
`.claude/skills/run-busbooking/` — call them with that full relative
path, or `cd` into the skill dir first (that's what the examples do).

## What this is

`BusBooking/BusBooking.csproj` — ASP.NET Core 10 app, currently a
hybrid: server-rendered Razor Views + cookie auth (old, still the only
working UI) alongside a growing `api/...` JSON surface secured with
JWT (`Controllers/Api/AuthController.cs`). Data lives in Postgres,
run via `docker-compose.yml` at the repo root. There is no frontend
dev server (no Next.js yet) — the driver here is a headless-Chromium
script via **Playwright** (`chromium-cli` is not installed in this
environment; this skill's `driver.mjs`/`smoke-login.mjs` fill that
role instead).

## Prerequisites

```bash
docker compose up -d          # Postgres, from repo root
cd .claude/skills/run-busbooking && npm install   # installs playwright (already vendored here)
npx playwright install chromium   # one-time browser download, if not already cached
```

## Build

```bash
dotnet build BusBooking/BusBooking.csproj
```

## Run (agent path — the driver)

Launch the app in the background on port 5100 (5000 collides with
macOS AirPlay Receiver; 5050 is the long-running docker-compose
instance — use 5100 to avoid both):

```bash
cd /Users/velinstaykov/BusBooking
dotnet run --project BusBooking --urls http://localhost:5100 > /tmp/busbooking-run.log 2>&1 &
echo $! > /tmp/busbooking-run.pid
```

Poll for readiness — **`timeout` is not available on stock macOS
bash**, use a manual loop:

```bash
n=0
until curl -sf http://localhost:5100/Users/SignIn >/dev/null 2>&1 || [ $n -ge 30 ]; do sleep 1; n=$((n+1)); done
```

Drive it with the two committed scripts (both verified working this
session):

```bash
cd .claude/skills/run-busbooking

# Load any page, wait for text, screenshot, report console errors
node driver.mjs /Users/SignIn --wait-text "Sign In" --out /tmp/busbooking-signin.png
node driver.mjs /Users/SignUp --wait-text "Sign Up" --out /tmp/busbooking-signup.png

# End-to-end: fill the real Sign In form with a seeded user, submit,
# confirm the redirect
node smoke-login.mjs xyz@mail.com 1234
```

`driver.mjs <path> [--wait-text "text"] [--out file.png]` — generic
page loader. `smoke-login.mjs [email] [password]` — the one
representative user flow (defaults to the seeded admin
`xyz@mail.com` / `1234`). Both print `console errors: none` or the
list — check that before declaring success, not just the screenshot.

Stop the app when done:

```bash
kill $(cat /tmp/busbooking-run.pid)
```

## Run (human path)

```bash
docker compose up -d
dotnet run --project BusBooking --urls http://localhost:5100
```
Open `http://localhost:5100/Users/SignIn` in a real browser. Ctrl-C to stop.

Seeded users (from `ApplicationDbContext.cs`): `xyz@mail.com` /
`1234` (role Admin, username `tati`), `xyz1@mail.com` / `1234` (role
Customer, username `anni`).

## Test

No automated test suite exists yet (planned in `ROADMAP.md` Фаза 9).

## Gotchas

- **`timeout` doesn't exist on stock macOS bash** — polling loops
  must use a manual counter (see above), not `timeout N cmd`.
- **`chromium-cli` is not installed in this environment** — use the
  Playwright scripts in this skill dir instead. Chromium itself is
  already downloaded (`~/Library/Caches/ms-playwright/`); if missing,
  `npx playwright install chromium`.
- **Port 5000 is taken by macOS AirPlay Receiver** (`ControlCenter`
  process) — always bind to something else (5100 here) for local
  `dotnet run`, independent of whatever port the docker-compose
  service uses (5050).
- **Two ways to run it simultaneously**: the long-running
  `docker-compose` stack (port 5050, built image, may be stale after
  code changes) and a local `dotnet run` (port 5100, always current
  source). For verifying a fresh code change, use `dotnet run` on
  5100 — rebuilding the Docker image is slower and easy to forget.
- **`Trip/Index` (the post-login landing page) renders a blank white
  page by design** — the action returns `Ok()` with no View. A blank
  screenshot after a successful login is expected, not a driver
  failure; check the URL and HTTP status, not just the screenshot
  content, to confirm the redirect worked.
- **`Views/Users/SignIn.cshtml` and `SignUp.cshtml` are standalone
  full HTML pages** (their own `<html>`/`<head>`/`<body>`), not using
  `_Layout.cshtml` via the normal `@RenderBody()` mechanism — yet the
  rendered page still shows the shared header/nav. If you see
  duplicate `<html>` tags or unexpected layout behavior while editing
  these views, that's why.
- **Two 404s and a JS error were fixed in these views** (2026-07-10):
  `~/css/home.css` never existed (only `site.css` does — both
  `SignIn.cshtml`/`SignUp.cshtml` now point at `site.css`);
  `_Layout.cshtml` referenced the scoped-CSS bundle under a
  leftover old project name (`AirlineSeatReservationSystem.styles.css`)
  instead of the real assembly name (`BusBooking.styles.css`); and
  `SignIn.cshtml` had a dead inline `<script>` calling
  `document.getElementById('loginForm')` on a form with no such id,
  POSTing to a nonexistent `/signin` route — removed, since the real
  login already works via the plain form POST to `UsersController.SignIn`.

## Troubleshooting

| Symptom | Fix |
|---|---|
| `curl` to `localhost:5100` never succeeds | Check `/tmp/busbooking-run.log` — usually a Postgres connection failure because `docker compose up -d` wasn't run first, or user-secrets `ConnectionStrings:DefaultConnection` isn't set (`dotnet user-secrets list --project BusBooking`) |
| `driver.mjs` throws `browserType.launch: Executable doesn't exist` | `npx playwright install chromium` |
| Login smoke test doesn't redirect / stays on Sign In | Seed data or password hash mismatch — reseed via `dotnet ef database update --project BusBooking`, confirm seeded hash in `ApplicationDbContext.cs` still matches password `1234` |
| Port 5100 already in use | An earlier `dotnet run` is still backgrounded — `kill $(cat /tmp/busbooking-run.pid)` or `lsof -nP -iTCP:5100 -sTCP:LISTEN` to find and kill it |

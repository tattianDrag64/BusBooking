---
name: run-busbooking
description: Build, launch, and drive the BusBooking ASP.NET Core JSON API (JWT auth, Postgres) — run it, exercise the full signup/signin/trips/seats/bookings flow via fetch, screenshot Swagger UI. Use when asked to run, start, test, or verify BusBooking, or to check an API/Controllers change works end-to-end.
---

All paths below are relative to the repo root (`BusBooking/`, the
directory containing `docker-compose.yml`), not to this skill
directory. The driver scripts live in
`.claude/skills/run-busbooking/` — call them with that full relative
path, or `cd` into the skill dir first (that's what the examples do).

## What this is

`BusBooking/BusBooking.csproj` — ASP.NET Core 10 app. **As of
2026-07-10 this is a pure JSON API** — the old server-rendered Razor
Views and cookie authentication were removed entirely in favor of JWT
(`Controllers/AuthController.cs`, `TripsController.cs`,
`SeatsController.cs`, `BookingsController.cs`, `UsersController.cs`,
all under `api/...`). There is no browsable UI and no frontend dev
server yet (Next.js frontend is a separate, not-yet-started phase).
Data lives in Postgres, run via `docker-compose.yml` at the repo root.

The primary driver is **plain `fetch`** (`smoke-api.mjs`) — no browser
needed for a JSON API. Playwright (`driver.mjs`) is kept only for the
one remaining browser-driven surface: Swagger UI, useful for a quick
visual sanity check.

## Prerequisites

```bash
docker compose up -d          # Postgres, from repo root
cd .claude/skills/run-busbooking && npm install   # installs playwright (only needed for Swagger UI screenshots)
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
until curl -sf http://localhost:5100/health >/dev/null 2>&1 || [ $n -ge 30 ]; do sleep 1; n=$((n+1)); done
```

### Full API smoke test (primary check — no browser)

```bash
cd .claude/skills/run-busbooking
node smoke-api.mjs
```

Exercises, in order, against the real running app and real Postgres:
`GET /health` → `POST /api/auth/signin` (seeded admin) →
`GET /api/trips/search` (public, no token) →
`GET /api/users` (401 with no token, 200 with admin token) →
`GET /api/seats/trip/{tripId}` (auto-generates 24 seats on first call
if none exist) → `POST /api/seats/book` → booking the same seat again
(`409`) → `GET /api/bookings/my` → `POST /api/auth/refresh` →
`POST /api/auth/revoke`. Prints `[OK]`/`[FAIL]` per check and exits
non-zero if anything failed — check the exit code, not just the
printed summary.

Needs at least one `Trip` row to exercise seats/bookings; if none
exists it creates none itself (needs a `busId` you'd have to look up)
and skips those checks with `[SKIP]` — see Gotchas for how to create
one via curl.

### Swagger UI screenshot (secondary — visual sanity check)

```bash
node driver.mjs /swagger --out /tmp/busbooking-swagger.png
```

`driver.mjs <path> [--wait-text "text"] [--out file.png]` — generic
Playwright page loader, prints HTTP status, screenshot path, and any
browser console errors. Swagger UI has no simple stable text to
`--wait-text` on (tag groups are dynamic) — just omit that flag and
look at the screenshot.

Stop the app when done:

```bash
kill $(cat /tmp/busbooking-run.pid)
```

## Run (human path)

```bash
docker compose up -d
dotnet run --project BusBooking --urls http://localhost:5100
```
Open `http://localhost:5100/swagger` in a real browser to explore/call
endpoints interactively. Ctrl-C to stop.

Seeded users (from `ApplicationDbContext.cs`): `xyz@mail.com` /
`1234` (role Admin, username `tati`), `xyz1@mail.com` / `1234` (role
Customer, username `anni`). No `Trip` rows are seeded — only
`RouteInfo`/`Bus` — create one via `POST /api/trips` (admin token
required) before exercising seats/bookings, see Gotchas.

## Test

No automated test suite exists yet (planned in `ROADMAP.md` Фаза 9).

## Gotchas

- **`timeout` doesn't exist on stock macOS bash** — polling loops
  must use a manual counter (see above), not `timeout N cmd`.
- **Port 5000 is taken by macOS AirPlay Receiver** (`ControlCenter`
  process) — always bind to something else (5100 here) for local
  `dotnet run`, independent of whatever port the docker-compose
  service uses (5050).
- **Two ways to run it simultaneously**: the long-running
  `docker-compose` stack (port 5050, built image, may be stale after
  code changes) and a local `dotnet run` (port 5100, always current
  source). For verifying a fresh code change, use `dotnet run` on
  5100 — rebuilding the Docker image is slower and easy to forget.
- **No `Trip` rows are seeded** — only `RouteInfo` and `Bus`. To
  create one for testing (needs a real `busId` from the DB):
  ```bash
  BUS_ID=$(docker compose exec -T postgres psql -U busbookinguser -d busbookingdb -t -c "SELECT \"Id\" FROM \"Buses\" LIMIT 1;" | tr -d ' \n')
  TOKEN=$(curl -s -X POST http://localhost:5100/api/auth/signin -H "Content-Type: application/json" -d '{"email":"xyz@mail.com","password":"1234"}' | python3 -c "import sys,json; print(json.load(sys.stdin)['accessToken'])")
  curl -s -X POST http://localhost:5100/api/trips -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
    -d "{\"from\":\"Sofia\",\"to\":\"Berlin\",\"busId\":\"$BUS_ID\",\"departureDate\":\"2026-08-01T08:00:00\",\"arrivalDate\":\"2026-08-01T20:00:00\",\"isReturnTrip\":false,\"price\":45.0}"
  ```
- **Dates sent without a timezone break trip create/edit.** `TripsController.Create`/`Edit`
  explicitly do `DateTime.SpecifyKind(model.X, DateTimeKind.Utc)` to work around
  `Npgsql.ArgumentException: Cannot write DateTime with Kind=Unspecified to PostgreSQL
  type 'timestamp with time zone'` — `System.Text.Json` parses a bare
  `"2026-08-01T08:00:00"` (no `Z`/offset) as `Kind=Unspecified`, and the
  `timestamptz` column requires `Kind=Utc`. Already fixed in the controllers;
  just know why if it resurfaces elsewhere (e.g. a future controller touching
  `DateTime` columns).
- **Role claims are an enum, serialized as a string.** `UserRole` (`Admin`/`Customer`)
  is a C# enum end-to-end (`Entity/UserRole.cs`), and `Program.cs` registers a
  `JsonStringEnumConverter` so API responses show `"role": "Admin"`, not `0`. If a
  new enum field appears in a VM and shows up as a number instead, check that
  converter is still registered.
- **`chromium-cli` is not installed in this environment** — `driver.mjs` uses
  Playwright directly instead. Chromium is already downloaded
  (`~/Library/Caches/ms-playwright/`); if missing, `npx playwright install chromium`.
- **`Views/`, `wwwroot/`, and `HomeController.cs` are gone** — don't try to
  screenshot old paths like `/Users/SignIn` or `/Home/Privacy`; they now 404
  (no route registered at all, not even a broken one).

## Troubleshooting

| Symptom | Fix |
|---|---|
| `curl`/`smoke-api.mjs` to `localhost:5100` never succeeds | Check `/tmp/busbooking-run.log` — usually a Postgres connection failure because `docker compose up -d` wasn't run first, or user-secrets `ConnectionStrings:DefaultConnection` isn't set (`dotnet user-secrets list --project BusBooking`) |
| App fails at startup with `Unable to resolve service for type 'ApplicationDbContext'` | `builder.Services.AddDbContext<ApplicationDbContext>(...)` got removed from `Program.cs` (happened once during a refactor) — check it's still there, right after `AddControllers()` |
| `smoke-api.mjs` prints `[SKIP]` for seats/bookings checks | No `Trip` row in the DB — create one first, see Gotchas |
| `driver.mjs` throws `browserType.launch: Executable doesn't exist` | `npx playwright install chromium` |
| Port 5100 already in use | An earlier `dotnet run` is still backgrounded — `kill $(cat /tmp/busbooking-run.pid)` or `lsof -nP -iTCP:5100 -sTCP:LISTEN` to find and kill it |

# Pometco — Frontend

Next.js (App Router) frontend for the Pometco bus booking platform. Talks to the ASP.NET Core API in `../BusBooking/`.

> ⚠️ This project is on a recent Next.js version with real breaking changes vs. older docs/training data (e.g. `middleware.js` → `proxy.js`). See `AGENTS.md` and check `node_modules/next/dist/docs/` before assuming an API still works the way it used to.

## Getting started

From the repo root, `./dev.sh` starts everything (Postgres + backend + this frontend) in one command — see the root `README.md`.

To run just the frontend against an already-running backend:

```bash
npm install
npm run dev
```

Open [http://localhost:3000](http://localhost:3000). Requires `NEXT_PUBLIC_API_BASE_URL` in `.env.local` pointing at the backend (default `http://localhost:5100`).

## What's here

- **App Router**, all routes under `app/[locale]/` — i18n via `next-intl` (EN unprefixed, RU/RO prefixed: `/ru/...`, `/ro/...`).
- **JWT auth** — `auth/` (`AuthProvider`/`useAuth`, `tokenStorage` in `localStorage`, `AdminGuard` for the role-gated `/admin/*` section).
- **Multi-currency** — `currency/` (`CurrencyProvider`/`useCurrency`, rates from the backend, choice persisted in `localStorage`).
- **Data fetching** — TanStack Query (`lib/QueryProvider.tsx`); `api/*.ts` is one thin client file per backend controller.
- **Map** — Leaflet + OpenStreetMap (`components/routes/`), client-only (`dynamic(..., { ssr: false })`).
- **Admin panel** — `app/[locale]/admin/*`, gated by `auth/AdminGuard.tsx` (role check, not just auth check).

## Conventions

- **`index + vm` pattern** for interactive pages: `page.tsx` (markup only) + `use<Name>PageVM.ts` (state/effects/API calls). See `app/[locale]/login/` for the canonical example.
- **One `api/*.ts` file per backend controller**, not a single generic HTTP client.
- **`types/*.ts` mirrors backend VMs** — one file per entity/resource, camelCase to match ASP.NET Core's default JSON serialization.
- Internal links/navigation use `@/i18n/navigation` (`Link`, `useRouter`, `usePathname`), **not** `next/link`/`next/navigation` directly — those don't carry the locale prefix.

Full architecture reasoning (rendering strategy, why these patterns, Next.js-version-specific gotchas encountered along the way) — **`ARCHITECTURE_NOTES.md`**. Start there for anything non-obvious.

## Other docs

- `../ROADMAP.md` — overall project status.
- `../FRONTEND_PLAN.md`, `../FRONTEND_AUTH_PLAN.md`, `../FRONTEND_STRUCTURE.md` — original scaffolding decisions.
- `../BOOKING_FLOW_PLAN.md`, `../ADMIN_UI_PLAN.md`, `../DESIGN_UNIFICATION_PLAN.md` — feature-specific design notes, frontend-relevant.

# 🚌 Pometco (BusBooking) — Bus Ticket Booking Platform

A full-stack bus ticket booking platform for a real transport company (Moldova ↔ Bulgaria/Romania/Europe): search routes, pick a seat, pay, get a confirmed booking — plus an admin panel to manage routes, buses, schedules, trips, users, and bookings.

- **Backend** (`BusBooking/`) — ASP.NET Core 10 JSON API, JWT auth, PostgreSQL, Stripe payments.
- **Frontend** (`busbooking-frontend/`) — Next.js (App Router) + TypeScript, TanStack Query, JWT auth, i18n (EN/RU/RO), multi-currency (EUR/MDL/USD/RON).

> Design references (early mockups) are in `design/`. The actual implementation has since diverged from and, in places, gone beyond those mockups (see `DESIGN_UNIFICATION_PLAN.md`).

---

## Quick start

```bash
./dev.sh
```

Starts Postgres (Docker), applies EF Core migrations, runs the backend on `:5100` and the frontend on `:3000`. Requires Docker Desktop running. `Ctrl+C` stops backend/frontend; Postgres keeps running.

Manual/step-by-step setup, port caveats, and how to generate seed trips — see `busbooking-frontend/ARCHITECTURE_NOTES.md`, section 18.

Dev admin login (seeded): `xyz@mail.com` / `1234`. Regular user: `xyz1@mail.com` / `1234`.

---

## Tech stack

| Layer               | Choice                                                                          |
| ------------------- | ------------------------------------------------------------------------------- |
| Backend framework   | ASP.NET Core 10 Web API                                                         |
| ORM / DB            | Entity Framework Core 10 + PostgreSQL                                           |
| Auth                | JWT (access + refresh), `PasswordHasher`/BCrypt, account lockout, rate limiting |
| Payments            | Stripe Checkout + webhooks                                                      |
| Frontend framework  | Next.js (App Router) + TypeScript                                               |
| Styling             | Tailwind CSS                                                                    |
| Data fetching/cache | TanStack Query                                                                  |
| Map                 | Leaflet + OpenStreetMap                                                         |
| i18n                | next-intl (EN default, RU/RO)                                                   |
| Containerization    | Docker (multi-stage), `docker-compose.yml` for local Postgres/backend           |

Full rationale for each choice — `ROADMAP.md`, "Технологический стек" section.

---

## What's implemented

- **Public site** — Home, About, Routes (with a real interactive map + decorative intermediate-stop preview), search → seat selection → checkout → booking confirmation, "My bookings" with cancellation/refund.
- **Auth** — signup/signin/refresh/revoke, JWT in `localStorage`, account lockout after failed attempts, breached-password check (Have I Been Pwned, k-anonymity).
- **Payments** — Stripe Checkout session + webhook-driven confirmation (source of truth is the webhook, not the success-page redirect — see `busbooking-frontend/ARCHITECTURE_NOTES.md`, section 16).
- **Admin panel** (`/admin/*`, role-gated) — Routes, Buses, Schedules, Trips (incl. auto-generation from schedules), Users (read-only), Bookings (admin-cancel with mandatory reason).
- **i18n** — EN (unprefixed URLs)/RU/RO, header/footer/home/about translated; deeper pages still English-only.
- **Multi-currency** — EUR/MDL/USD/RON, live rates (cached server-side), user's choice persisted client-side.
- **Security** — rate limiting, CORS from config (not hardcoded), security response headers, account lockout, refresh-token hashing + reuse detection, HIBP password check.

Full, up-to-date status (including what's _not_ done yet) — `ROADMAP.md`. Detailed design/implementation notes for specific features — the various `*_PLAN.md` files at the repo root (booking flow, cancellation/refund, admin UI, email notifications, responsive audit, design unification, etc.) and `busbooking-frontend/ARCHITECTURE_NOTES.md` for frontend architecture decisions and reasoning.

---

## Project structure

```
BusBooking/                  # ASP.NET Core API
  Controllers/                 # one per resource (Auth, Routes, Buses, Schedules, Trips, Seats, Bookings, Payments, Users, Currency)
  Entity/                      # EF Core entities
  Models/                      # request/response VMs
  Repository/                  # repository + unit-of-work pattern
  Services/                    # token, password hashing, payments, currency rates, breach checking
  Migrations/

busbooking-frontend/         # Next.js App Router frontend
  app/[locale]/                 # all routes, under the i18n locale segment
  components/                   # ui/, home/, about/, routes/ — by feature area
  api/                           # one thin client file per backend controller
  auth/, currency/, i18n/       # cross-cutting client-side concerns
  types/                         # mirrors backend VMs

design/                       # early design mockups (superseded in places, see DESIGN_UNIFICATION_PLAN.md)
docker-compose.yml            # Postgres (+ optional containerized backend) for local dev
dev.sh                        # one-command local startup
```

---

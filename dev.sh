#!/usr/bin/env bash
# Starts Postgres (Docker), the backend (dotnet run) and the frontend (npm run dev)
# with one command. Ctrl+C stops the backend/frontend; Postgres is left running
# (same `docker compose up -d` habit used everywhere else in this repo).
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")"

echo "==> Starting Postgres (docker compose)…"
docker compose up -d postgres

echo "==> Waiting for Postgres…"
until docker compose exec -T postgres pg_isready -U "${POSTGRES_USER:-busbookinguser}" >/dev/null 2>&1; do
  sleep 1
done

echo "==> Applying EF Core migrations…"
(cd BusBooking && dotnet ef database update)

echo "==> Starting backend (dotnet run, http://localhost:5100)…"
(cd BusBooking && dotnet run --urls http://localhost:5100) &
BACKEND_PID=$!

cleanup() {
  echo
  echo "==> Stopping backend…"
  kill "$BACKEND_PID" 2>/dev/null || true
}
trap cleanup EXIT INT TERM

echo "==> Waiting for backend health check…"
until curl -sf http://localhost:5100/health >/dev/null 2>&1; do
  sleep 1
done
echo "==> Backend is up."

echo "==> Starting frontend (npm run dev, http://localhost:3000)…"
(cd busbooking-frontend && npm run dev)

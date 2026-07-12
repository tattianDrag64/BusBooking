import { authFetch } from "@/api/authFetch";
import type { RouteCreatePayload, RouteSummary } from "@/types/route";

// GET /api/routes is public (api/routes.ts) — these three are the admin-only
// mutations added alongside the admin UI (RoutesController used to be read-only).
export function createRoute(payload: RouteCreatePayload): Promise<RouteSummary> {
  return authFetch<RouteSummary>("/api/routes", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
}

export function updateRoute(id: string, payload: RouteCreatePayload): Promise<void> {
  return authFetch<void>(`/api/routes/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
}

export function deleteRoute(id: string): Promise<void> {
  return authFetch<void>(`/api/routes/${id}`, { method: "DELETE" });
}

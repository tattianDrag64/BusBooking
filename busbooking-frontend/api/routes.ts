import { apiFetch } from "@/api/client";
import type { RouteSummary } from "@/types/route";

export async function getRoutes(): Promise<RouteSummary[]> {
  try {
    return await apiFetch<RouteSummary[]>("/api/routes", {
      next: { revalidate: 3600 },
    });
  } catch (error) {
    // Keeps `next build` (and ISR revalidation) from failing outright when the
    // API is unreachable — the home page renders its empty state instead.
    console.error("getRoutes: falling back to an empty list", error);
    return [];
  }
}

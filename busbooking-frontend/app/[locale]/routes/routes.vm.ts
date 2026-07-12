import { getRoutes } from "@/api/routes";
import type { RouteSummary } from "@/types/route";

export interface RoutesFilters {
  from?: string;
  to?: string;
}

export interface RoutesPageVM {
  routes: RouteSummary[];
  filters: RoutesFilters;
}

export async function getRoutesVM(
  filters: RoutesFilters = {},
): Promise<RoutesPageVM> {
  const routes = await getRoutes();
  const from = filters.from?.trim().toLowerCase();
  const to = filters.to?.trim().toLowerCase();

  const filtered = routes.filter((route) => {
    const matchesFrom =
      !from || route.departureCity.toLowerCase().includes(from);
    const matchesTo = !to || route.arrivalCity.toLowerCase().includes(to);
    return matchesFrom && matchesTo;
  });

  return { routes: filtered, filters: { from: filters.from, to: filters.to } };
}

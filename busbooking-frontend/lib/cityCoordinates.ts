// Hardcoded lat/lng for cities the map needs to plot — routes only carry city
// names (`RouteSummary.departureCity`/`arrivalCity`), not coordinates, and
// geocoding every city on the client via Nominatim isn't worth it for a small,
// known set of destinations (see FRONTEND_PLAN.md, section 5).
export const CITY_COORDINATES: Record<string, [number, number]> = {
  chisinau: [47.0105, 28.8638],
  bucharest: [44.4268, 26.1025],
  sofia: [42.6977, 23.3219],
  varna: [43.2141, 27.9147],
  berlin: [52.52, 13.405],
  munich: [48.1351, 11.582],
  vienna: [48.2082, 16.3738],
  budapest: [47.4979, 19.0402],
  belgrade: [44.7866, 20.4489],
  "novi sad": [45.2671, 19.8335],
};

export function coordinatesFor(city: string): [number, number] | null {
  return CITY_COORDINATES[city.trim().toLowerCase()] ?? null;
}

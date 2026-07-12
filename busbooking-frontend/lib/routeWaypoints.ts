// Purely decorative intermediate stops shown on the route map — the data model
// has no concept of a stop yet (`RouteInfo` only has departure/arrival city, see
// ROADMAP.md, "Промежуточные остановки и бронирование по участкам" — a much
// bigger, not-yet-built feature: a real `RouteStop` entity + segment-based
// booking/pricing). This is only a visual preview of what that could look like
// on the map, not a functional stop — no booking-by-segment behind it.
const ROUTE_WAYPOINTS: Record<string, string[]> = {
  "sofia|berlin": ["Belgrade", "Novi Sad", "Budapest", "Vienna"],
};

export function waypointsFor(departureCity: string, arrivalCity: string): string[] {
  const key = `${departureCity.trim().toLowerCase()}|${arrivalCity.trim().toLowerCase()}`;
  const reverseKey = `${arrivalCity.trim().toLowerCase()}|${departureCity.trim().toLowerCase()}`;
  return ROUTE_WAYPOINTS[key] ?? [...(ROUTE_WAYPOINTS[reverseKey] ?? [])].reverse();
}

import type { RouteSummary, ScheduleSummary, Weekday } from "@/types/route";

export function outboundSchedules(route: RouteSummary): ScheduleSummary[] {
  return route.schedules.filter((s) => !s.isReturnTrip);
}

export function returnSchedules(route: RouteSummary): ScheduleSummary[] {
  return route.schedules.filter((s) => s.isReturnTrip);
}

// e.g. "Every day, 08:00, 13:00" or "Monday, Wednesday, 08:00" — used when the
// caller already knows which direction's schedules it has (see app/routes/page.tsx).
export function formatSchedules(schedules: ScheduleSummary[]): string {
  if (schedules.length === 0) {
    return "Not scheduled";
  }
  const isDaily = schedules.some((s) => s.dayOfWeek === null);
  const dayLabel = isDaily
    ? "Every day"
    : [...new Set(schedules.map((s) => s.dayOfWeek))].join(", ");
  const times = [...new Set(schedules.map((s) => s.departureTime))]
    .sort()
    .join(", ");
  return `${dayLabel}, ${times}`;
}

export function routeRunsOnDay(route: RouteSummary, day: Weekday): boolean {
  return outboundSchedules(route).some(
    (s) => s.dayOfWeek === day || s.dayOfWeek === null,
  );
}

// e.g. "Every day, 08:00, 13:00, 16:00" or "Monday, 08:00"
export function formatScheduleSummary(route: RouteSummary): string {
  return formatSchedules(outboundSchedules(route));
}

export function earliestDepartureTime(routes: RouteSummary[]): string {
  const times = routes.flatMap((route) =>
    outboundSchedules(route).map((s) => s.departureTime),
  );
  return times.sort().at(0) ?? "—";
}

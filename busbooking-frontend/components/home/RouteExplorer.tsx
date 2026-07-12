"use client";

import { Link } from "@/i18n/navigation";
import { useMemo, useState } from "react";
import {
  earliestDepartureTime,
  formatScheduleSummary,
  routeRunsOnDay,
} from "@/lib/schedule";
import { useCurrency } from "@/currency/useCurrency";
import type { RouteSummary, Weekday } from "@/types/route";

const WEEKDAY_ORDER: readonly Weekday[] = [
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
  "Sunday",
];

export function RouteExplorer({ routes }: { routes: RouteSummary[] }) {
  const { format } = useCurrency();
  const availableDays = useMemo(
    () =>
      WEEKDAY_ORDER.filter((day) =>
        routes.some((route) => routeRunsOnDay(route, day)),
      ),
    [routes],
  );
  const [selectedDay, setSelectedDay] = useState<Weekday | null>(null);

  const visibleRoutes = useMemo(() => {
    const filtered = selectedDay
      ? routes.filter((route) => routeRunsOnDay(route, selectedDay))
      : routes;
    return filtered.slice(0, 4);
  }, [routes, selectedDay]);

  const nextDeparture = useMemo(() => earliestDepartureTime(routes), [routes]);

  return (
    <section className="mx-auto max-w-[1280px] px-4 py-24 sm:px-12">
      <div className="mb-10 flex flex-col items-end justify-between gap-6 md:flex-row">
        <div>
          <h2 className="font-headline text-3xl font-bold text-on-surface">
            View routes
          </h2>
          <p className="mt-2 text-on-surface-variant">
            Filter by departure day and see the closest upcoming trips.
          </p>
        </div>
        <Link
          href="/routes"
          className="flex items-center gap-2 font-semibold text-primary transition-transform hover:translate-x-1"
        >
          View Full Network
          <span className="material-symbols-outlined">arrow_forward</span>
        </Link>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="rounded-card bg-surface-container-low p-6 shadow-sm lg:col-span-1">
          <div className="mb-4 flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary">
              <span className="material-symbols-outlined text-white">
                hub
              </span>
            </div>
            <div>
              <p className="text-sm font-semibold text-primary">Main Hub</p>
              <p className="font-headline text-lg font-bold">
                Chisinau Station
              </p>
            </div>
          </div>
          <div className="space-y-2 border-t border-outline-variant/30 pt-4 text-sm">
            <div className="flex justify-between">
              <span className="text-on-surface-variant">
                Earliest Departure
              </span>
              <span className="font-bold text-primary">{nextDeparture}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-on-surface-variant">Active Routes</span>
              <span className="font-bold">{routes.length} Lines</span>
            </div>
          </div>
        </div>

        <div className="rounded-card border border-outline-variant/30 p-6 lg:col-span-2">
          <div className="mb-6 flex flex-wrap gap-2">
            <button
              type="button"
              onClick={() => setSelectedDay(null)}
              className={`rounded-full px-4 py-2 text-sm font-semibold transition-colors ${
                selectedDay === null
                  ? "bg-primary text-white"
                  : "bg-surface-container-low text-on-surface-variant hover:bg-surface-container"
              }`}
            >
              All days
            </button>
            {availableDays.map((day) => (
              <button
                key={day}
                type="button"
                onClick={() => setSelectedDay(day)}
                className={`rounded-full px-4 py-2 text-sm font-semibold transition-colors ${
                  selectedDay === day
                    ? "bg-primary text-white"
                    : "bg-surface-container-low text-on-surface-variant hover:bg-surface-container"
                }`}
              >
                {day}
              </button>
            ))}
          </div>

          {visibleRoutes.length === 0 ? (
            <p className="text-on-surface-variant">
              No routes for this day yet.
            </p>
          ) : (
            <ul className="grid gap-3 sm:grid-cols-2">
              {visibleRoutes.map((route) => (
                <li
                  key={route.id}
                  className="flex items-center justify-between rounded-xl border border-outline-variant/30 px-4 py-3"
                >
                  <div>
                    <p className="font-semibold text-on-surface">
                      {route.departureCity} → {route.arrivalCity}
                    </p>
                    <p className="text-xs text-on-surface-variant">
                      {formatScheduleSummary(route)}
                    </p>
                  </div>
                  <p className="font-bold text-primary">{format(route.price)}</p>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </section>
  );
}

import { formatScheduleSummary } from "@/lib/schedule";
import type { RouteSummary } from "@/types/route";

const ICONS = ["local_shipping", "directions_bus", "airport_shuttle"];

export function ActiveRoutesSidebar({ routes }: { routes: RouteSummary[] }) {
  const preview = routes.slice(0, 3);

  return (
    <div className="flex w-full flex-col gap-4 md:w-80">
      <div className="rounded-3xl border border-outline-variant/30 bg-white p-6 shadow-sm">
        <h3 className="mb-4 font-headline text-lg font-bold text-primary">
          Active Routes
        </h3>
        <div className="space-y-3">
          {preview.map((route, index) => (
            <div
              key={route.id}
              className="flex items-center gap-3 rounded-xl bg-surface-light p-3"
            >
              <div className="flex h-10 w-10 items-center justify-center rounded-full bg-white shadow-sm">
                <span className="material-symbols-outlined text-primary">
                  {ICONS[index % ICONS.length]}
                </span>
              </div>
              <div>
                <p className="text-sm font-semibold text-on-surface">
                  {route.departureCity} - {route.arrivalCity}
                </p>
                <p className="text-xs text-text-muted">
                  {formatScheduleSummary(route)}
                </p>
              </div>
            </div>
          ))}
          {preview.length === 0 && (
            <p className="text-sm text-on-surface-variant">
              No active routes right now.
            </p>
          )}
        </div>
      </div>

      <div className="rounded-3xl bg-primary-container p-6 text-white">
        <h4 className="mb-2 text-sm font-bold">Need Custom Logistics?</h4>
        <p className="mb-4 text-xs opacity-80">
          We provide specialized transport services for complex cargo
          requirements across the region.
        </p>
        <a
          href="mailto:info@pometco.com"
          className="block w-full rounded-lg bg-white py-2 text-center text-sm font-bold text-primary"
        >
          Contact Sales
        </a>
      </div>
    </div>
  );
}

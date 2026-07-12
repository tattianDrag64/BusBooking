import { Link } from "@/i18n/navigation";
import { Suspense } from "react";
import { getRoutes } from "@/api/routes";
import { getRoutesVM } from "./routes.vm";
import { SearchWidget } from "@/components/home/SearchWidget";
import { ActiveRoutesSidebar } from "@/components/routes/ActiveRoutesSidebar";
import { RouteMap } from "@/components/routes/RouteMap";
import { PriceTag } from "@/components/ui/PriceTag";
import { formatSchedules, outboundSchedules, returnSchedules } from "@/lib/schedule";

export const metadata = {
  title: "Routes — Pometco",
};

export default function RoutesPage({
  searchParams,
}: {
  searchParams: Promise<{ from?: string; to?: string }>;
}) {
  return (
    <>
      <section className="bg-white py-12">
        <div className="mx-auto max-w-[1280px] px-4 sm:px-12">
          <div className="mb-8">
            <h1 className="font-headline text-3xl font-bold text-on-surface">
              Find Your Journey
            </h1>
            <p className="mt-2 text-on-surface-variant">
              Real-time transport routes across Moldova, Bulgaria, and
              Europe.
            </p>
          </div>
          <Suspense fallback={<SearchWidgetSkeleton />}>
            <SearchWidgetSection />
          </Suspense>
        </div>
      </section>

      <Suspense fallback={<RoutesResultsSkeleton />}>
        <RoutesResultsSection searchParams={searchParams} />
      </Suspense>
    </>
  );
}

async function SearchWidgetSection() {
  const allRoutes = await getRoutes();
  return <SearchWidget routes={allRoutes} />;
}

function SearchWidgetSkeleton() {
  return (
    <div className="h-16 w-full animate-pulse rounded-2xl bg-surface-light" />
  );
}

async function RoutesResultsSection({
  searchParams,
}: {
  searchParams: Promise<{ from?: string; to?: string }>;
}) {
  const { from, to } = await searchParams;
  const [allRoutes, { routes, filters }] = await Promise.all([
    getRoutes(),
    getRoutesVM({ from, to }),
  ]);
  const hasFilters = Boolean(filters.from || filters.to);

  return (
    <>
      <section className="mx-auto w-full max-w-[1280px] px-4 py-12 sm:px-12">
        <div className="grid grid-cols-1 gap-6 md:grid-cols-[20rem_1fr]">
          <ActiveRoutesSidebar routes={allRoutes} />
          <div className="min-w-0">
            <RouteMap routes={routes} />
          </div>
        </div>
      </section>

      <section className="mx-auto w-full max-w-[1280px] px-4 pb-24 sm:px-12">
        <h2 className="text-2xl font-bold text-primary">All Routes</h2>
        <p className="mt-2 text-on-surface-variant">
          Destinations we serve and their weekly schedule.
        </p>

        {hasFilters && (
          <div className="mt-4 flex flex-wrap items-center gap-2 text-sm">
            <span className="text-on-surface-variant">Filtered by:</span>
            {filters.from && (
              <span className="rounded-full bg-surface-light px-3 py-1 font-semibold text-primary">
                From: {filters.from}
              </span>
            )}
            {filters.to && (
              <span className="rounded-full bg-surface-light px-3 py-1 font-semibold text-primary">
                To: {filters.to}
              </span>
            )}
            <Link href="/routes" className="text-primary underline">
              Clear filters
            </Link>
          </div>
        )}

        {routes.length === 0 ? (
          <p className="mt-8 text-on-surface-variant">
            No routes match this search right now.
          </p>
        ) : (
          <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {routes.map((route) => (
              <article
                key={route.id}
                className="rounded-2xl border border-outline-variant/30 p-6"
              >
                <h3 className="text-lg font-semibold text-primary">
                  {route.departureCity} → {route.arrivalCity}
                </h3>
                <dl className="mt-3 space-y-1 text-sm text-on-surface-variant">
                  <div className="flex justify-between">
                    <dt>Departure</dt>
                    <dd>{formatSchedules(outboundSchedules(route))}</dd>
                  </div>
                  <div className="flex justify-between">
                    <dt>Return</dt>
                    <dd>{formatSchedules(returnSchedules(route))}</dd>
                  </div>
                </dl>
                <p className="mt-4 text-xl font-bold text-primary">
                  <PriceTag amountInEur={route.price} />
                </p>
              </article>
            ))}
          </div>
        )}
      </section>
    </>
  );
}

function RoutesResultsSkeleton() {
  return (
    <>
      <section className="mx-auto w-full max-w-[1280px] px-4 py-12 sm:px-12">
        <div className="flex flex-col gap-6 md:flex-row">
          <div className="h-64 w-full animate-pulse rounded-2xl bg-surface-light md:w-72" />
          <div className="h-64 flex-grow animate-pulse rounded-2xl bg-surface-light" />
        </div>
      </section>
      <section className="mx-auto w-full max-w-[1280px] px-4 pb-24 sm:px-12">
        <div className="h-7 w-32 animate-pulse rounded bg-surface-light" />
        <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <div
              key={i}
              className="h-40 animate-pulse rounded-2xl bg-surface-light"
            />
          ))}
        </div>
      </section>
    </>
  );
}

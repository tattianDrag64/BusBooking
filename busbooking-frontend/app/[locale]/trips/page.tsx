import { Link } from "@/i18n/navigation";
import { searchTrips } from "@/api/trips";
import { PriceTag } from "@/components/ui/PriceTag";

export const metadata = {
  title: "Search results — Pometco",
};

export default async function TripsPage({
  searchParams,
}: {
  searchParams: Promise<{
    from?: string;
    to?: string;
    departureDate?: string;
  }>;
}) {
  const { from, to, departureDate } = await searchParams;
  const hasQuery = Boolean(from && to && departureDate);

  const trips = hasQuery
    ? await searchTrips({
        from: from!,
        to: to!,
        departureDate: departureDate!,
        isReturnTrip: false,
      })
    : [];

  return (
    <section className="mx-auto w-full max-w-[1280px] px-4 py-24 sm:px-12">
      <h1 className="font-headline text-3xl font-bold text-on-surface">
        Search results
      </h1>
      {hasQuery && (
        <p className="mt-2 text-on-surface-variant">
          {from} → {to}, {departureDate}
        </p>
      )}

      {!hasQuery ? (
        <p className="mt-8 text-on-surface-variant">
          Start a search from the homepage to see available trips.
        </p>
      ) : trips.length === 0 ? (
        <p className="mt-8 text-on-surface-variant">
          No trips match this search right now.
        </p>
      ) : (
        <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {trips.map((trip) => (
            <article
              key={trip.tripId}
              className="rounded-2xl border border-outline-variant/30 p-6"
            >
              <h3 className="text-lg font-semibold text-primary">
                {trip.from} → {trip.to}
              </h3>
              <dl className="mt-3 space-y-1 text-sm text-on-surface-variant">
                <div className="flex justify-between">
                  <dt>Departure</dt>
                  <dd>{new Date(trip.departureDate).toLocaleString()}</dd>
                </div>
                <div className="flex justify-between">
                  <dt>Arrival</dt>
                  <dd>{new Date(trip.arrivalDate).toLocaleString()}</dd>
                </div>
              </dl>
              <p className="mt-4 text-xl font-bold text-primary">
                <PriceTag amountInEur={trip.price} />
              </p>
              <Link
                href={`/trips/${trip.tripId}/seats`}
                className="mt-4 inline-block rounded-full bg-primary px-5 py-2 text-sm font-semibold text-white transition-colors hover:bg-primary-container"
              >
                Select seats
              </Link>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}

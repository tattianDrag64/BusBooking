"use client";

import { useBookingsPageVM } from "./useBookingsPageVM";
import { useCurrency } from "@/currency/useCurrency";

const CANCELLABLE_STATUSES = new Set(["PendingPayment", "Confirmed"]);

export default function BookingsPage() {
  const { bookings, isLoading, error, cancellingId, cancel } =
    useBookingsPageVM();
  const { format } = useCurrency();

  function handleCancel(orderId: string) {
    if (!window.confirm("Cancel this booking? This cannot be undone.")) {
      return;
    }
    cancel(orderId);
  }

  return (
    <section className="mx-auto w-full max-w-[1280px] px-4 py-24 sm:px-12">
      <h1 className="font-headline text-3xl font-bold text-on-surface">
        My bookings
      </h1>

      {error && <p className="mt-4 text-sm text-error">{error}</p>}

      {isLoading ? (
        <p className="mt-8 text-on-surface-variant">Loading…</p>
      ) : bookings.length === 0 ? (
        <p className="mt-8 text-on-surface-variant">
          You don&rsquo;t have any bookings yet.
        </p>
      ) : (
        <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {bookings.map((booking) => (
            <article
              key={booking.id}
              className="rounded-2xl border border-outline-variant/30 p-6"
            >
              <div className="flex items-start justify-between gap-2">
                <h3 className="text-lg font-semibold text-primary">
                  {booking.from} → {booking.to}
                </h3>
                <span className="rounded-full bg-surface-light px-3 py-1 text-xs font-semibold text-on-surface-variant">
                  {booking.status}
                </span>
              </div>
              <p className="mt-1 text-xs text-on-surface-variant">
                {booking.orderCode}
              </p>
              <dl className="mt-3 space-y-1 text-sm text-on-surface-variant">
                <div className="flex justify-between">
                  <dt>Departure</dt>
                  <dd>{new Date(booking.departureDate).toLocaleString()}</dd>
                </div>
                <div className="flex justify-between">
                  <dt>Seats</dt>
                  <dd>
                    {booking.seatNumbers.filter(Boolean).join(", ") || "—"}
                  </dd>
                </div>
              </dl>
              <p className="mt-4 text-xl font-bold text-primary">
                {format(booking.totalPrice)}
              </p>
              {CANCELLABLE_STATUSES.has(booking.status) && (
                <button
                  type="button"
                  disabled={cancellingId === booking.id}
                  onClick={() => handleCancel(booking.id)}
                  className="mt-4 rounded-full border border-error px-4 py-2 text-sm font-semibold text-error transition-colors hover:bg-error hover:text-white disabled:opacity-50"
                >
                  {cancellingId === booking.id ? "Cancelling…" : "Cancel booking"}
                </button>
              )}
            </article>
          ))}
        </div>
      )}
    </section>
  );
}

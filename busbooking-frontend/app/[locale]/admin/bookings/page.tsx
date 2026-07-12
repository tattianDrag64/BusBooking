"use client";

import { useAdminBookingsPageVM } from "./useAdminBookingsPageVM";

export default function AdminBookingsPage() {
  const {
    bookings,
    isLoading,
    openReasonFor,
    reason,
    setReason,
    openReasonForm,
    closeReasonForm,
    submitCancel,
    isCancelling,
    isCancellable,
    error,
  } = useAdminBookingsPageVM();

  return (
    <div className="flex flex-col gap-8">
      <div>
        <h1 className="font-headline text-2xl font-bold text-on-surface">Bookings</h1>
        <p className="mt-1 text-sm text-on-surface-variant">
          Across all customers. Cancelling as an admin requires a reason and works past the
          customer-facing deadline.
        </p>
      </div>

      {error && <p className="text-sm text-error">{error}</p>}

      {isLoading ? (
        <div className="h-64 animate-pulse rounded-2xl bg-surface-light" />
      ) : bookings.length === 0 ? (
        <p className="text-on-surface-variant">No bookings yet.</p>
      ) : (
        <div className="flex flex-col gap-4">
          {bookings.map((order) => (
            <div key={order.id} className="rounded-2xl border border-outline-variant/30 p-6">
              <div className="flex flex-wrap items-start justify-between gap-4">
                <div>
                  <h3 className="text-lg font-semibold text-primary">
                    {order.from} → {order.to}
                  </h3>
                  <p className="text-sm text-on-surface-variant">
                    {order.orderCode} · {order.userEmail}
                  </p>
                  <p className="mt-1 text-sm text-on-surface-variant">
                    Seats: {order.seatNumbers.filter(Boolean).join(", ") || "—"}
                  </p>
                </div>
                <div className="text-right">
                  <span className="rounded-full bg-surface-light px-3 py-1 text-xs font-semibold text-primary">
                    {order.status}
                  </span>
                  <p className="mt-2 text-xl font-bold text-primary">{order.totalPrice} €</p>
                </div>
              </div>

              {isCancellable(order.status) && (
                <div className="mt-4 border-t border-outline-variant/30 pt-4">
                  {openReasonFor === order.id ? (
                    <div className="flex flex-col gap-2">
                      <textarea
                        value={reason}
                        onChange={(e) => setReason(e.target.value)}
                        placeholder="Reason for cancellation (required)"
                        rows={2}
                        className="rounded-lg border border-outline-variant px-3 py-2 text-sm text-on-surface outline-none focus:border-primary focus:ring-1 focus:ring-primary"
                      />
                      <div className="flex gap-2">
                        <button
                          onClick={() => submitCancel(order.id)}
                          disabled={isCancelling}
                          className="rounded-lg bg-error px-4 py-2 text-sm font-semibold text-white disabled:opacity-50"
                        >
                          {isCancelling ? "Cancelling…" : "Confirm cancellation"}
                        </button>
                        <button
                          onClick={closeReasonForm}
                          className="rounded-lg px-4 py-2 text-sm font-semibold text-on-surface-variant"
                        >
                          Dismiss
                        </button>
                      </div>
                    </div>
                  ) : (
                    <button
                      onClick={() => openReasonForm(order.id)}
                      className="text-sm font-semibold text-error hover:underline"
                    >
                      Cancel booking (admin)
                    </button>
                  )}
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

"use client";

import { useAdminTripsPageVM } from "./useAdminTripsPageVM";

export default function AdminTripsPage() {
  const {
    trips,
    page,
    totalPages,
    goToPage,
    isLoading,
    weeksAhead,
    setWeeksAhead,
    handleGenerate,
    isGenerating,
    generateResult,
    deletingId,
    isDeleting,
    deleteTrip,
    error,
  } = useAdminTripsPageVM();

  return (
    <div className="flex flex-col gap-8">
      <div>
        <h1 className="font-headline text-2xl font-bold text-on-surface">Trips</h1>
        <p className="mt-1 text-sm text-on-surface-variant">
          Trips are generated from schedules — this doesn&rsquo;t create new schedules, only trip
          instances for the coming weeks.
        </p>
      </div>

      <div className="flex flex-wrap items-end gap-4 rounded-2xl border border-outline-variant/30 p-6">
        <label className="flex flex-col gap-2 text-sm font-semibold text-on-surface-variant">
          Weeks ahead
          <input
            type="number"
            min="1"
            value={weeksAhead}
            onChange={(e) => setWeeksAhead(e.target.value)}
            className="w-24 rounded-lg border border-outline-variant px-3 py-2 text-on-surface outline-none focus:border-primary focus:ring-1 focus:ring-primary"
          />
        </label>
        <button
          onClick={handleGenerate}
          disabled={isGenerating}
          className="rounded-lg bg-primary px-4 py-2 font-semibold text-white disabled:opacity-50"
        >
          {isGenerating ? "Generating…" : "Generate trips"}
        </button>
        {generateResult && <p className="text-sm text-on-surface-variant">{generateResult}</p>}
      </div>

      {error && <p className="text-sm text-error">{error}</p>}

      {isLoading ? (
        <div className="h-64 animate-pulse rounded-2xl bg-surface-light" />
      ) : trips.length === 0 ? (
        <p className="text-on-surface-variant">No trips yet — generate some above.</p>
      ) : (
        <>
          <div className="overflow-x-auto rounded-2xl border border-outline-variant/30">
            <table className="w-full text-left text-sm">
              <thead className="bg-surface-light text-on-surface-variant">
                <tr>
                  <th className="px-4 py-3">From</th>
                  <th className="px-4 py-3">To</th>
                  <th className="px-4 py-3">Departure</th>
                  <th className="px-4 py-3">Price</th>
                  <th className="px-4 py-3">Return?</th>
                  <th className="px-4 py-3" />
                </tr>
              </thead>
              <tbody>
                {trips.map((trip) => (
                  <tr key={trip.tripId} className="border-t border-outline-variant/30">
                    <td className="px-4 py-3">{trip.from}</td>
                    <td className="px-4 py-3">{trip.to}</td>
                    <td className="px-4 py-3">
                      {new Date(trip.departureDate).toLocaleString()}
                    </td>
                    <td className="px-4 py-3">{trip.price} €</td>
                    <td className="px-4 py-3">{trip.isReturnTrip ? "Yes" : "No"}</td>
                    <td className="px-4 py-3 text-right">
                      <button
                        onClick={() => deleteTrip(trip.tripId)}
                        disabled={isDeleting && deletingId === trip.tripId}
                        className="font-semibold text-error hover:underline disabled:opacity-50"
                      >
                        {isDeleting && deletingId === trip.tripId ? "Deleting…" : "Delete"}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="flex items-center justify-between text-sm">
            <button
              onClick={() => goToPage(page - 1)}
              disabled={page <= 1}
              className="font-semibold text-primary disabled:opacity-40"
            >
              ← Previous
            </button>
            <span className="text-on-surface-variant">
              Page {page} of {totalPages}
            </span>
            <button
              onClick={() => goToPage(page + 1)}
              disabled={page >= totalPages}
              className="font-semibold text-primary disabled:opacity-40"
            >
              Next →
            </button>
          </div>
        </>
      )}
    </div>
  );
}

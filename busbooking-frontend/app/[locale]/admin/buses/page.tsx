"use client";

import { useAdminBusesPageVM } from "./useAdminBusesPageVM";

export default function AdminBusesPage() {
  const {
    buses,
    routes,
    isLoading,
    form,
    setField,
    handleSubmit,
    isSubmitting,
    deletingId,
    isDeleting,
    deleteBus,
    routeLabel,
    error,
  } = useAdminBusesPageVM();

  return (
    <div className="flex flex-col gap-8">
      <div>
        <h1 className="font-headline text-2xl font-bold text-on-surface">Buses</h1>
        <p className="mt-1 text-sm text-on-surface-variant">
          A route needs at least one bus before trips can be generated for it.
        </p>
      </div>

      <form
        onSubmit={handleSubmit}
        className="grid grid-cols-1 gap-4 rounded-2xl border border-outline-variant/30 p-6 sm:grid-cols-4"
      >
        <label className="flex flex-col gap-2 text-sm font-semibold text-on-surface-variant">
          Bus number
          <input
            value={form.busNumber}
            onChange={(e) => setField("busNumber", e.target.value)}
            className="rounded-lg border border-outline-variant px-3 py-2 text-on-surface outline-none focus:border-primary focus:ring-1 focus:ring-primary"
          />
        </label>
        <label className="flex flex-col gap-2 text-sm font-semibold text-on-surface-variant">
          Seats
          <input
            type="number"
            min="1"
            max="80"
            value={form.seatsCount}
            onChange={(e) => setField("seatsCount", e.target.value)}
            className="rounded-lg border border-outline-variant px-3 py-2 text-on-surface outline-none focus:border-primary focus:ring-1 focus:ring-primary"
          />
        </label>
        <label className="flex flex-col gap-2 text-sm font-semibold text-on-surface-variant">
          Route
          <select
            value={form.routeId}
            onChange={(e) => setField("routeId", e.target.value)}
            className="rounded-lg border border-outline-variant px-3 py-2 text-on-surface outline-none focus:border-primary focus:ring-1 focus:ring-primary"
          >
            <option value="">Select a route…</option>
            {routes.map((route) => (
              <option key={route.id} value={route.id}>
                {route.departureCity} → {route.arrivalCity}
              </option>
            ))}
          </select>
        </label>
        <div className="flex items-end">
          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full rounded-lg bg-primary px-4 py-2 font-semibold text-white disabled:opacity-50"
          >
            {isSubmitting ? "Creating…" : "Create bus"}
          </button>
        </div>
      </form>

      {error && <p className="text-sm text-error">{error}</p>}

      {isLoading ? (
        <div className="h-40 animate-pulse rounded-2xl bg-surface-light" />
      ) : buses.length === 0 ? (
        <p className="text-on-surface-variant">No buses yet.</p>
      ) : (
        <div className="overflow-x-auto rounded-2xl border border-outline-variant/30">
          <table className="w-full text-left text-sm">
            <thead className="bg-surface-light text-on-surface-variant">
              <tr>
                <th className="px-4 py-3">Bus number</th>
                <th className="px-4 py-3">Seats</th>
                <th className="px-4 py-3">Route</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody>
              {buses.map((bus) => (
                <tr key={bus.id} className="border-t border-outline-variant/30">
                  <td className="px-4 py-3">{bus.busNumber}</td>
                  <td className="px-4 py-3">{bus.seatsCount}</td>
                  <td className="px-4 py-3">{routeLabel(bus.routeId)}</td>
                  <td className="px-4 py-3 text-right">
                    <button
                      onClick={() => deleteBus(bus.id)}
                      disabled={isDeleting && deletingId === bus.id}
                      className="font-semibold text-error hover:underline disabled:opacity-50"
                    >
                      {isDeleting && deletingId === bus.id ? "Deleting…" : "Delete"}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

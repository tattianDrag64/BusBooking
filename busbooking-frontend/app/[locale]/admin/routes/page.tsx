"use client";

import { useAdminRoutesPageVM } from "./useAdminRoutesPageVM";

export default function AdminRoutesPage() {
  const {
    routes,
    isLoading,
    form,
    setField,
    handleSubmit,
    isSubmitting,
    deletingId,
    isDeleting,
    deleteRoute,
    error,
  } = useAdminRoutesPageVM();

  return (
    <div className="flex flex-col gap-8">
      <div>
        <h1 className="font-headline text-2xl font-bold text-on-surface">Routes</h1>
        <p className="mt-1 text-sm text-on-surface-variant">
          Create the departure/arrival pairs that schedules and trips are generated from.
        </p>
      </div>

      <form
        onSubmit={handleSubmit}
        className="grid grid-cols-1 gap-4 rounded-2xl border border-outline-variant/30 p-6 sm:grid-cols-4"
      >
        <label className="flex flex-col gap-2 text-sm font-semibold text-on-surface-variant">
          Departure city
          <input
            value={form.departureCity}
            onChange={(e) => setField("departureCity", e.target.value)}
            className="rounded-lg border border-outline-variant px-3 py-2 text-on-surface outline-none focus:border-primary focus:ring-1 focus:ring-primary"
          />
        </label>
        <label className="flex flex-col gap-2 text-sm font-semibold text-on-surface-variant">
          Arrival city
          <input
            value={form.arrivalCity}
            onChange={(e) => setField("arrivalCity", e.target.value)}
            className="rounded-lg border border-outline-variant px-3 py-2 text-on-surface outline-none focus:border-primary focus:ring-1 focus:ring-primary"
          />
        </label>
        <label className="flex flex-col gap-2 text-sm font-semibold text-on-surface-variant">
          Price (€)
          <input
            type="number"
            min="0"
            step="0.01"
            value={form.price}
            onChange={(e) => setField("price", e.target.value)}
            className="rounded-lg border border-outline-variant px-3 py-2 text-on-surface outline-none focus:border-primary focus:ring-1 focus:ring-primary"
          />
        </label>
        <div className="flex items-end">
          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full rounded-lg bg-primary px-4 py-2 font-semibold text-white disabled:opacity-50"
          >
            {isSubmitting ? "Creating…" : "Create route"}
          </button>
        </div>
      </form>

      {error && <p className="text-sm text-error">{error}</p>}

      {isLoading ? (
        <div className="h-40 animate-pulse rounded-2xl bg-surface-light" />
      ) : routes.length === 0 ? (
        <p className="text-on-surface-variant">No routes yet.</p>
      ) : (
        <div className="overflow-x-auto rounded-2xl border border-outline-variant/30">
          <table className="w-full text-left text-sm">
            <thead className="bg-surface-light text-on-surface-variant">
              <tr>
                <th className="px-4 py-3">Departure</th>
                <th className="px-4 py-3">Arrival</th>
                <th className="px-4 py-3">Price</th>
                <th className="px-4 py-3">Schedules</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody>
              {routes.map((route) => (
                <tr key={route.id} className="border-t border-outline-variant/30">
                  <td className="px-4 py-3">{route.departureCity}</td>
                  <td className="px-4 py-3">{route.arrivalCity}</td>
                  <td className="px-4 py-3">{route.price} €</td>
                  <td className="px-4 py-3">{route.schedules.length}</td>
                  <td className="px-4 py-3 text-right">
                    <button
                      onClick={() => deleteRoute(route.id)}
                      disabled={isDeleting && deletingId === route.id}
                      className="font-semibold text-error hover:underline disabled:opacity-50"
                    >
                      {isDeleting && deletingId === route.id ? "Deleting…" : "Delete"}
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

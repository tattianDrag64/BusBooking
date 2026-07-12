"use client";

import { useAdminSchedulesPageVM } from "./useAdminSchedulesPageVM";

export default function AdminSchedulesPage() {
  const {
    routes,
    weekdays,
    schedules,
    isLoadingSchedules,
    form,
    setField,
    handleSubmit,
    isSubmitting,
    deletingId,
    isDeleting,
    deleteSchedule,
    error,
  } = useAdminSchedulesPageVM();

  return (
    <div className="flex flex-col gap-8">
      <div>
        <h1 className="font-headline text-2xl font-bold text-on-surface">Schedules</h1>
        <p className="mt-1 text-sm text-on-surface-variant">
          Trips are generated from schedules — pick a route to see and manage its slots.
        </p>
      </div>

      <label className="flex max-w-sm flex-col gap-2 text-sm font-semibold text-on-surface-variant">
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

      {form.routeId && (
        <>
          <form
            onSubmit={handleSubmit}
            className="grid grid-cols-1 gap-4 rounded-2xl border border-outline-variant/30 p-6 sm:grid-cols-5"
          >
            <label className="flex flex-col gap-2 text-sm font-semibold text-on-surface-variant">
              Departure time
              <input
                type="time"
                value={form.departureTime}
                onChange={(e) => setField("departureTime", e.target.value)}
                className="rounded-lg border border-outline-variant px-3 py-2 text-on-surface outline-none focus:border-primary focus:ring-1 focus:ring-primary"
              />
            </label>
            <label className="flex flex-col gap-2 text-sm font-semibold text-on-surface-variant">
              Duration (hours)
              <input
                type="number"
                min="0.5"
                step="0.5"
                value={form.durationHours}
                onChange={(e) => setField("durationHours", e.target.value)}
                className="rounded-lg border border-outline-variant px-3 py-2 text-on-surface outline-none focus:border-primary focus:ring-1 focus:ring-primary"
              />
            </label>
            <label className="flex flex-col gap-2 text-sm font-semibold text-on-surface-variant">
              Day of week
              <select
                value={form.dayOfWeek}
                onChange={(e) => setField("dayOfWeek", e.target.value as typeof form.dayOfWeek)}
                className="rounded-lg border border-outline-variant px-3 py-2 text-on-surface outline-none focus:border-primary focus:ring-1 focus:ring-primary"
              >
                <option value="">Every day</option>
                {weekdays.map((day) => (
                  <option key={day} value={day}>
                    {day}
                  </option>
                ))}
              </select>
            </label>
            <label className="flex items-center gap-2 self-end pb-2 text-sm font-semibold text-on-surface-variant">
              <input
                type="checkbox"
                checked={form.isReturnTrip}
                onChange={(e) => setField("isReturnTrip", e.target.checked)}
              />
              Return trip
            </label>
            <div className="flex items-end">
              <button
                type="submit"
                disabled={isSubmitting}
                className="w-full rounded-lg bg-primary px-4 py-2 font-semibold text-white disabled:opacity-50"
              >
                {isSubmitting ? "Creating…" : "Create schedule"}
              </button>
            </div>
          </form>

          {error && <p className="text-sm text-error">{error}</p>}

          {isLoadingSchedules ? (
            <div className="h-32 animate-pulse rounded-2xl bg-surface-light" />
          ) : schedules.length === 0 ? (
            <p className="text-on-surface-variant">No schedules for this route yet.</p>
          ) : (
            <div className="overflow-x-auto rounded-2xl border border-outline-variant/30">
              <table className="w-full text-left text-sm">
                <thead className="bg-surface-light text-on-surface-variant">
                  <tr>
                    <th className="px-4 py-3">Departure</th>
                    <th className="px-4 py-3">Duration</th>
                    <th className="px-4 py-3">Day</th>
                    <th className="px-4 py-3">Return?</th>
                    <th className="px-4 py-3" />
                  </tr>
                </thead>
                <tbody>
                  {schedules.map((schedule) => (
                    <tr key={schedule.id} className="border-t border-outline-variant/30">
                      <td className="px-4 py-3">{schedule.departureTime.slice(0, 5)}</td>
                      <td className="px-4 py-3">{schedule.duration.slice(0, 5)}</td>
                      <td className="px-4 py-3">{schedule.dayOfWeek ?? "Every day"}</td>
                      <td className="px-4 py-3">{schedule.isReturnTrip ? "Yes" : "No"}</td>
                      <td className="px-4 py-3 text-right">
                        <button
                          onClick={() => deleteSchedule(schedule.id)}
                          disabled={isDeleting && deletingId === schedule.id}
                          className="font-semibold text-error hover:underline disabled:opacity-50"
                        >
                          {isDeleting && deletingId === schedule.id ? "Deleting…" : "Delete"}
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}
    </div>
  );
}

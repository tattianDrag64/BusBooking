"use client";

import { useTranslations } from "next-intl";
import { useRouter } from "@/i18n/navigation";
import { useMemo, useState } from "react";
import type { RouteSummary } from "@/types/route";

export function SearchWidget({ routes }: { routes: RouteSummary[] }) {
  const t = useTranslations("Home");
  const router = useRouter();
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [departure, setDeparture] = useState("");
  const [returnDate, setReturnDate] = useState("");

  const departureCities = useMemo(
    () => Array.from(new Set(routes.map((route) => route.departureCity))),
    [routes],
  );
  const arrivalCities = useMemo(
    () => Array.from(new Set(routes.map((route) => route.arrivalCity))),
    [routes],
  );

  function handleSubmit(event: React.SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!from.trim() || !to.trim() || !departure) {
      return;
    }
    // Round-trip search isn't supported by the backend yet (see ROADMAP.md,
    // "Билет туда-обратно со скидкой") — `returnDate` is collected but not sent.
    const params = new URLSearchParams({
      from: from.trim(),
      to: to.trim(),
      departureDate: departure,
      isReturnTrip: "false",
    });
    router.push(`/trips?${params}`);
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="grid grid-cols-1 gap-4 rounded-card bg-white p-4 shadow-lg sm:p-6 md:grid-cols-2 lg:grid-cols-5"
    >
      <label className="flex flex-col gap-2 text-sm font-semibold text-on-surface-variant">
        <span className="flex items-center gap-1">
          <span className="material-symbols-outlined text-base">
            location_on
          </span>
          {t("searchFrom")}
        </span>
        <input
          list="departure-cities"
          value={from}
          onChange={(event) => setFrom(event.target.value)}
          placeholder={t("searchFromPlaceholder")}
          className="rounded-lg border border-outline-variant px-4 py-3 text-on-surface outline-none focus:border-primary focus:ring-1 focus:ring-primary"
        />
        <datalist id="departure-cities">
          {departureCities.map((city) => (
            <option key={city} value={city} />
          ))}
        </datalist>
      </label>

      <label className="flex flex-col gap-2 text-sm font-semibold text-on-surface-variant">
        <span className="flex items-center gap-1">
          <span className="material-symbols-outlined text-base">
            near_me
          </span>
          {t("searchTo")}
        </span>
        <input
          list="arrival-cities"
          value={to}
          onChange={(event) => setTo(event.target.value)}
          placeholder={t("searchToPlaceholder")}
          className="rounded-lg border border-outline-variant px-4 py-3 text-on-surface outline-none focus:border-primary focus:ring-1 focus:ring-primary"
        />
        <datalist id="arrival-cities">
          {arrivalCities.map((city) => (
            <option key={city} value={city} />
          ))}
        </datalist>
      </label>

      <label className="flex flex-col gap-2 text-sm font-semibold text-on-surface-variant">
        <span className="flex items-center gap-1">
          <span className="material-symbols-outlined text-base">
            calendar_month
          </span>
          {t("searchDeparture")}
        </span>
        <input
          type="date"
          value={departure}
          onChange={(event) => setDeparture(event.target.value)}
          className="rounded-lg border border-outline-variant px-4 py-3 text-on-surface outline-none focus:border-primary focus:ring-1 focus:ring-primary"
        />
      </label>

      <label className="flex flex-col gap-2 text-sm font-semibold text-on-surface-variant">
        <span className="flex items-center gap-1">
          <span className="material-symbols-outlined text-base">
            history_toggle_off
          </span>
          {t("searchReturn")}
        </span>
        <input
          type="date"
          value={returnDate}
          onChange={(event) => setReturnDate(event.target.value)}
          className="rounded-lg border border-outline-variant px-4 py-3 text-on-surface outline-none focus:border-primary focus:ring-1 focus:ring-primary"
        />
      </label>

      <div className="flex items-end">
        <button
          type="submit"
          className="flex h-[52px] w-full items-center justify-center gap-2 rounded-lg bg-primary font-semibold text-white shadow-md transition-soft hover:bg-primary-container active:scale-95"
        >
          <span className="material-symbols-outlined">search</span>
          {t("searchButton")}
        </button>
      </div>
    </form>
  );
}

import { apiFetch } from "@/api/client";
import type { TripSearchParams, TripSearchResult } from "@/types/trip";

export function searchTrips(
  params: TripSearchParams,
): Promise<TripSearchResult[]> {
  const query = new URLSearchParams({
    from: params.from,
    to: params.to,
    departureDate: params.departureDate,
    isReturnTrip: String(params.isReturnTrip),
  });
  if (params.arrivalDate) {
    query.set("arrivalDate", params.arrivalDate);
  }
  return apiFetch<TripSearchResult[]>(`/api/trips/search?${query}`);
}

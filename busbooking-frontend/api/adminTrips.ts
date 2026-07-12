import { authFetch } from "@/api/authFetch";
import type { AdminTripPage, GenerateTripsResult } from "@/types/adminTrip";

export function getTripsPage(page: number, pageSize = 20): Promise<AdminTripPage> {
  return authFetch<AdminTripPage>(`/api/trips?page=${page}&pageSize=${pageSize}`);
}

export function generateTrips(weeksAhead: number): Promise<GenerateTripsResult> {
  return authFetch<GenerateTripsResult>("/api/trips/generate", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ weeksAhead }),
  });
}

export function deleteTrip(tripId: string): Promise<void> {
  return authFetch<void>(`/api/trips/${tripId}`, { method: "DELETE" });
}

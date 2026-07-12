import { authFetch } from "@/api/authFetch";
import type { BookSeatResponse } from "@/types/order";
import type { Seat } from "@/types/seat";

export function getSeatsForTrip(tripId: string): Promise<Seat[]> {
  return authFetch<Seat[]>(`/api/seats/trip/${tripId}`);
}

export function bookSeats(payload: {
  tripId: string;
  seatIds: string[];
}): Promise<BookSeatResponse> {
  return authFetch<BookSeatResponse>("/api/seats/book", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
}

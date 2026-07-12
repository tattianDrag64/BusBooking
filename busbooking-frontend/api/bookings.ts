import { authFetch } from "@/api/authFetch";
import type { OrderSummary } from "@/types/order";

export function getMyBookings(): Promise<OrderSummary[]> {
  return authFetch<OrderSummary[]>("/api/bookings/my");
}

export function cancelBooking(orderId: string): Promise<void> {
  return authFetch<void>(`/api/bookings/${orderId}/cancel`, {
    method: "POST",
  });
}

import { authFetch } from "@/api/authFetch";
import type { AdminOrderSummary } from "@/types/adminOrder";

export function getAllBookings(): Promise<AdminOrderSummary[]> {
  return authFetch<AdminOrderSummary[]>("/api/bookings");
}

export function adminCancelBooking(orderId: string, reason: string): Promise<void> {
  return authFetch<void>(`/api/bookings/${orderId}/admin-cancel`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ reason }),
  });
}

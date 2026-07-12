import { authFetch } from "@/api/authFetch";
import type { CheckoutSession } from "@/types/order";

export function createCheckoutSession(orderId: string): Promise<CheckoutSession> {
  return authFetch<CheckoutSession>(`/api/payments/checkout/${orderId}`, {
    method: "POST",
  });
}

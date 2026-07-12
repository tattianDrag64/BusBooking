import type { OrderStatus } from "@/types/order";

export interface AdminOrderSummary {
  id: string;
  orderCode: string;
  userEmail: string;
  from: string;
  to: string;
  departureDate: string;
  totalPrice: number;
  createdAt: string;
  seatNumbers: (string | null)[];
  status: OrderStatus;
}

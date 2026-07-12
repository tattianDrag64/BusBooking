export interface BookSeatResponse {
  orderId: string;
  orderCode: string;
  seatIds: string[];
  holdExpiresAt: string;
}

export type OrderStatus =
  | "PendingPayment"
  | "Confirmed"
  | "Cancelled"
  | "Expired"
  | "Refunded";

export interface OrderSummary {
  id: string;
  orderCode: string;
  from: string;
  to: string;
  departureDate: string;
  totalPrice: number;
  createdAt: string;
  seatNumbers: (string | null)[];
  status: OrderStatus;
}

export interface CheckoutSession {
  checkoutUrl: string;
}

"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { adminCancelBooking, getAllBookings } from "@/api/adminBookings";

const CANCELLABLE_STATUSES = new Set(["PendingPayment", "Confirmed"]);

export function useAdminBookingsPageVM() {
  const queryClient = useQueryClient();
  const [openReasonFor, setOpenReasonFor] = useState<string | null>(null);
  const [reason, setReason] = useState("");
  const [error, setError] = useState<string | null>(null);

  const bookingsQuery = useQuery({ queryKey: ["admin", "bookings"], queryFn: getAllBookings });

  const cancelMutation = useMutation({
    mutationFn: ({ orderId, reason }: { orderId: string; reason: string }) =>
      adminCancelBooking(orderId, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin", "bookings"] });
      setOpenReasonFor(null);
      setReason("");
      setError(null);
    },
    onError: () => setError("Could not cancel this booking."),
  });

  function openReasonForm(orderId: string) {
    setOpenReasonFor(orderId);
    setReason("");
    setError(null);
  }

  function closeReasonForm() {
    setOpenReasonFor(null);
    setReason("");
  }

  function submitCancel(orderId: string) {
    if (!reason.trim()) {
      setError("A reason is required to cancel a booking as an admin.");
      return;
    }
    cancelMutation.mutate({ orderId, reason: reason.trim() });
  }

  return {
    bookings: bookingsQuery.data ?? [],
    isLoading: bookingsQuery.isPending,
    openReasonFor,
    reason,
    setReason,
    openReasonForm,
    closeReasonForm,
    submitCancel,
    isCancelling: cancelMutation.isPending,
    isCancellable: (status: string) => CANCELLABLE_STATUSES.has(status),
    error: error ?? (bookingsQuery.isError ? "Could not load bookings." : null),
  };
}

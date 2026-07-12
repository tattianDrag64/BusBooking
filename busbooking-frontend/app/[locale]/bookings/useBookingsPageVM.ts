"use client";

import { useEffect, useState } from "react";
import { useRouter } from "@/i18n/navigation";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { cancelBooking, getMyBookings } from "@/api/bookings";
import { useAuth } from "@/auth/useAuth";

export function useBookingsPageVM() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const { isAuthenticated, isLoading: authLoading } = useAuth();

  const [cancellingId, setCancellingId] = useState<string | null>(null);
  const [cancelError, setCancelError] = useState<string | null>(null);

  useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      router.replace("/login?redirectTo=/bookings");
    }
  }, [authLoading, isAuthenticated, router]);

  const bookingsQuery = useQuery({
    queryKey: ["bookings", "my"],
    queryFn: getMyBookings,
    enabled: !authLoading && isAuthenticated,
  });

  const cancelMutation = useMutation({
    mutationFn: cancelBooking,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["bookings", "my"] });
    },
    onError: () => {
      setCancelError("Could not cancel this booking. Please try again.");
    },
    onSettled: () => {
      setCancellingId(null);
    },
  });

  function cancel(orderId: string) {
    setCancelError(null);
    setCancellingId(orderId);
    cancelMutation.mutate(orderId);
  }

  return {
    bookings: bookingsQuery.data ?? [],
    isLoading: authLoading || bookingsQuery.isPending,
    error:
      cancelError ??
      (bookingsQuery.isError ? "Could not load your bookings." : null),
    cancellingId,
    cancel,
  };
}

"use client";

import { useEffect, useState } from "react";
import { useRouter } from "@/i18n/navigation";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ApiError } from "@/api/client";
import { bookSeats, getSeatsForTrip } from "@/api/seats";
import { useAuth } from "@/auth/useAuth";

export function useSeatsPageVM(tripId: string) {
  const router = useRouter();
  const queryClient = useQueryClient();
  const { isAuthenticated, isLoading: authLoading } = useAuth();

  const [selectedSeatIds, setSelectedSeatIds] = useState<string[]>([]);
  const [bookingError, setBookingError] = useState<string | null>(null);

  useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      router.replace(`/login?redirectTo=/trips/${tripId}/seats`);
    }
  }, [authLoading, isAuthenticated, tripId, router]);

  // No staleTime override — seat availability changes as other users book, so the
  // default (always refetch on mount/refocus) is the right behavior here, not an
  // oversight (see FRONTEND_PLAN.md, "seats — обязательно staleTime небольшой").
  const seatsQuery = useQuery({
    queryKey: ["seats", tripId],
    queryFn: () => getSeatsForTrip(tripId),
    enabled: !authLoading && isAuthenticated,
  });

  const bookMutation = useMutation({
    mutationFn: (seatIds: string[]) => bookSeats({ tripId, seatIds }),
    onSuccess: ({ orderId }) => {
      router.push(`/checkout/${orderId}`);
    },
    onError: (err: unknown) => {
      if (err instanceof ApiError && err.status === 409) {
        setBookingError(
          "One or more selected seats were just taken by someone else — pick different ones.",
        );
        setSelectedSeatIds([]);
        queryClient.invalidateQueries({ queryKey: ["seats", tripId] });
      } else {
        setBookingError("Could not book these seats. Please try again.");
      }
    },
  });

  function toggleSeat(seatId: string) {
    setSelectedSeatIds((current) =>
      current.includes(seatId)
        ? current.filter((id) => id !== seatId)
        : [...current, seatId],
    );
  }

  function confirmBooking() {
    if (selectedSeatIds.length === 0) {
      return;
    }
    setBookingError(null);
    bookMutation.mutate(selectedSeatIds);
  }

  return {
    seats: seatsQuery.data ?? [],
    isLoading: authLoading || seatsQuery.isPending,
    error:
      bookingError ??
      (seatsQuery.isError ? "Could not load seats for this trip." : null),
    selectedSeatIds,
    toggleSeat,
    isBooking: bookMutation.isPending,
    confirmBooking,
  };
}

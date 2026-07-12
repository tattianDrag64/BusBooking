"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { deleteTrip, generateTrips, getTripsPage } from "@/api/adminTrips";

const PAGE_SIZE = 20;

export function useAdminTripsPageVM() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [weeksAhead, setWeeksAhead] = useState("4");
  const [error, setError] = useState<string | null>(null);
  const [generateResult, setGenerateResult] = useState<string | null>(null);

  const tripsQuery = useQuery({
    queryKey: ["admin", "trips", page],
    queryFn: () => getTripsPage(page, PAGE_SIZE),
  });

  const generateMutation = useMutation({
    mutationFn: generateTrips,
    onSuccess: (result) => {
      setGenerateResult(
        `Created ${result.created}, skipped ${result.skipped} (already existed), ${result.skippedNoBus} route(s) had no bus assigned.`,
      );
      setError(null);
      queryClient.invalidateQueries({ queryKey: ["admin", "trips"] });
    },
    onError: () => setError("Could not generate trips."),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteTrip,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["admin", "trips"] }),
    onError: () => setError("Could not delete the trip."),
  });

  function handleGenerate() {
    const weeks = Number(weeksAhead);
    if (!Number.isFinite(weeks) || weeks <= 0) {
      setError("Weeks ahead must be a positive number.");
      return;
    }
    setGenerateResult(null);
    generateMutation.mutate(weeks);
  }

  const data = tripsQuery.data;
  const totalPages = data ? Math.max(1, Math.ceil(data.totalCount / data.pageSize)) : 1;

  return {
    trips: data?.items ?? [],
    page,
    totalPages,
    goToPage: (p: number) => setPage(Math.min(Math.max(p, 1), totalPages)),
    isLoading: tripsQuery.isPending,
    weeksAhead,
    setWeeksAhead,
    handleGenerate,
    isGenerating: generateMutation.isPending,
    generateResult,
    deletingId: deleteMutation.variables,
    isDeleting: deleteMutation.isPending,
    deleteTrip: (id: string) => deleteMutation.mutate(id),
    error: error ?? (tripsQuery.isError ? "Could not load trips." : null),
  };
}

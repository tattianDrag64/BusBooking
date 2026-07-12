"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getRoutes } from "@/api/routes";
import { createBus, deleteBus, getBuses } from "@/api/adminBuses";

const EMPTY_FORM = { busNumber: "", seatsCount: "40", routeId: "" };

export function useAdminBusesPageVM() {
  const queryClient = useQueryClient();
  const [form, setForm] = useState(EMPTY_FORM);
  const [error, setError] = useState<string | null>(null);

  const busesQuery = useQuery({ queryKey: ["admin", "buses"], queryFn: getBuses });
  const routesQuery = useQuery({ queryKey: ["admin", "routes"], queryFn: getRoutes });

  const createMutation = useMutation({
    mutationFn: createBus,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin", "buses"] });
      setForm(EMPTY_FORM);
      setError(null);
    },
    onError: () => setError("Could not create the bus. Please check the fields and try again."),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteBus,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["admin", "buses"] }),
    onError: () => setError("Could not delete the bus."),
  });

  function setField(field: keyof typeof EMPTY_FORM, value: string) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  function handleSubmit(event: React.SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    const seatsCount = Number(form.seatsCount);
    if (!form.busNumber.trim() || !form.routeId || !Number.isInteger(seatsCount) || seatsCount < 1) {
      setError("Please fill in every field — seats must be a whole number of at least 1.");
      return;
    }
    createMutation.mutate({
      busNumber: form.busNumber.trim(),
      seatsCount,
      routeId: form.routeId,
    });
  }

  const routes = routesQuery.data ?? [];
  const routeLabel = (routeId: string) => {
    const route = routes.find((r) => r.id === routeId);
    return route ? `${route.departureCity} → ${route.arrivalCity}` : routeId;
  };

  return {
    buses: busesQuery.data ?? [],
    routes,
    isLoading: busesQuery.isPending || routesQuery.isPending,
    form,
    setField,
    handleSubmit,
    isSubmitting: createMutation.isPending,
    deletingId: deleteMutation.variables,
    isDeleting: deleteMutation.isPending,
    deleteBus: (id: string) => deleteMutation.mutate(id),
    routeLabel,
    error: error ?? (busesQuery.isError ? "Could not load buses." : null),
  };
}

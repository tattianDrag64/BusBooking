"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getRoutes } from "@/api/routes";
import { createRoute, deleteRoute } from "@/api/adminRoutes";

const EMPTY_FORM = { departureCity: "", arrivalCity: "", price: "" };

export function useAdminRoutesPageVM() {
  const queryClient = useQueryClient();
  const [form, setForm] = useState(EMPTY_FORM);
  const [error, setError] = useState<string | null>(null);

  const routesQuery = useQuery({
    queryKey: ["admin", "routes"],
    queryFn: getRoutes,
  });

  const createMutation = useMutation({
    mutationFn: createRoute,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin", "routes"] });
      queryClient.invalidateQueries({ queryKey: ["routes"] });
      setForm(EMPTY_FORM);
      setError(null);
    },
    onError: () => setError("Could not create the route. Please check the fields and try again."),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteRoute,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin", "routes"] });
      queryClient.invalidateQueries({ queryKey: ["routes"] });
    },
    onError: () => setError("Could not delete the route."),
  });

  function setField(field: keyof typeof EMPTY_FORM, value: string) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  function handleSubmit(event: React.SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    const price = Number(form.price);
    if (!form.departureCity.trim() || !form.arrivalCity.trim() || Number.isNaN(price) || price < 0) {
      setError("Please fill in every field with a valid, non-negative price.");
      return;
    }
    createMutation.mutate({
      departureCity: form.departureCity.trim(),
      arrivalCity: form.arrivalCity.trim(),
      price,
    });
  }

  return {
    routes: routesQuery.data ?? [],
    isLoading: routesQuery.isPending,
    form,
    setField,
    handleSubmit,
    isSubmitting: createMutation.isPending,
    deletingId: deleteMutation.variables,
    isDeleting: deleteMutation.isPending,
    deleteRoute: (id: string) => deleteMutation.mutate(id),
    error: error ?? (routesQuery.isError ? "Could not load routes." : null),
  };
}

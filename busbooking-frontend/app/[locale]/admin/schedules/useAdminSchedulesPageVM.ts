"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getRoutes } from "@/api/routes";
import { createSchedule, deleteSchedule, getSchedulesForRoute } from "@/api/adminSchedules";
import type { Weekday } from "@/types/route";

const WEEKDAYS: Weekday[] = [
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
  "Sunday",
];

const EMPTY_FORM = {
  routeId: "",
  departureTime: "08:00",
  durationHours: "3",
  dayOfWeek: "" as Weekday | "",
  isReturnTrip: false,
};

export function useAdminSchedulesPageVM() {
  const queryClient = useQueryClient();
  const [form, setForm] = useState(EMPTY_FORM);
  const [error, setError] = useState<string | null>(null);

  const routesQuery = useQuery({ queryKey: ["admin", "routes"], queryFn: getRoutes });

  const schedulesQuery = useQuery({
    queryKey: ["admin", "schedules", form.routeId],
    queryFn: () => getSchedulesForRoute(form.routeId),
    enabled: Boolean(form.routeId),
  });

  const createMutation = useMutation({
    mutationFn: createSchedule,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin", "schedules", form.routeId] });
      setError(null);
    },
    onError: () => setError("Could not create the schedule — it may already exist for this route/day/time."),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteSchedule,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["admin", "schedules", form.routeId] }),
    onError: () => setError("Could not delete the schedule."),
  });

  function setField<K extends keyof typeof EMPTY_FORM>(field: K, value: (typeof EMPTY_FORM)[K]) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  function handleSubmit(event: React.SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    const hours = Number(form.durationHours);
    if (!form.routeId || !form.departureTime || !Number.isFinite(hours) || hours <= 0) {
      setError("Please select a route, a departure time, and a positive duration.");
      return;
    }
    const durationWholeHours = Math.floor(hours);
    const durationMinutes = Math.round((hours - durationWholeHours) * 60);
    createMutation.mutate({
      routeId: form.routeId,
      departureTime: `${form.departureTime}:00`,
      duration: `${String(durationWholeHours).padStart(2, "0")}:${String(durationMinutes).padStart(2, "0")}:00`,
      dayOfWeek: form.dayOfWeek || null,
      isReturnTrip: form.isReturnTrip,
    });
  }

  return {
    routes: routesQuery.data ?? [],
    weekdays: WEEKDAYS,
    schedules: schedulesQuery.data ?? [],
    isLoadingSchedules: form.routeId ? schedulesQuery.isPending : false,
    form,
    setField,
    handleSubmit,
    isSubmitting: createMutation.isPending,
    deletingId: deleteMutation.variables,
    isDeleting: deleteMutation.isPending,
    deleteSchedule: (id: string) => deleteMutation.mutate(id),
    error: error ?? (schedulesQuery.isError ? "Could not load schedules for this route." : null),
  };
}

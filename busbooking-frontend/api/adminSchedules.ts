import { authFetch } from "@/api/authFetch";
import type { AdminSchedule, ScheduleCreatePayload } from "@/types/adminSchedule";

export function getSchedulesForRoute(routeId: string): Promise<AdminSchedule[]> {
  return authFetch<AdminSchedule[]>(`/api/schedules/route/${routeId}`);
}


export function createSchedule(payload: ScheduleCreatePayload): Promise<AdminSchedule> {
  return authFetch<AdminSchedule>("/api/schedules", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
}

export function deleteSchedule(id: string): Promise<void> {
  return authFetch<void>(`/api/schedules/${id}`, { method: "DELETE" });
}

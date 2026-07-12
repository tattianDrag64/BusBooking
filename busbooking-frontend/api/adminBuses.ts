import { authFetch } from "@/api/authFetch";
import type { Bus, BusCreatePayload } from "@/types/bus";

export function getBuses(): Promise<Bus[]> {
  return authFetch<Bus[]>("/api/buses");
}

export function createBus(payload: BusCreatePayload): Promise<Bus> {
  return authFetch<Bus>("/api/buses", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
}

export function deleteBus(id: string): Promise<void> {
  return authFetch<void>(`/api/buses/${id}`, { method: "DELETE" });
}

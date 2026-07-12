import type { Weekday } from "@/types/route";

export interface AdminSchedule {
  id: string;
  routeId: string;
  // TimeSpan serialized by System.Text.Json as "HH:mm:ss".
  departureTime: string;
  duration: string;
  dayOfWeek: Weekday | null;
  isReturnTrip: boolean;
}

export interface ScheduleCreatePayload {
  routeId: string;
  departureTime: string;
  duration: string;
  dayOfWeek: Weekday | null;
  isReturnTrip: boolean;
}

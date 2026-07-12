export type Weekday =
  | "Monday"
  | "Tuesday"
  | "Wednesday"
  | "Thursday"
  | "Friday"
  | "Saturday"
  | "Sunday";

export interface ScheduleSummary {
  // null = runs every day.
  dayOfWeek: Weekday | null;
  departureTime: string;
  isReturnTrip: boolean;
}

export interface RouteSummary {
  id: string;
  departureCity: string;
  arrivalCity: string;
  price: number;
  schedules: ScheduleSummary[];
}

export interface RouteCreatePayload {
  departureCity: string;
  arrivalCity: string;
  price: number;
}

export interface Bus {
  id: string;
  busNumber: string;
  seatsCount: number;
  routeId: string;
}

export interface BusCreatePayload {
  busNumber: string;
  seatsCount: number;
  routeId: string;
}

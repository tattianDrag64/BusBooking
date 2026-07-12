export interface TripSearchResult {
  tripId: string;
  from: string;
  to: string;
  departureDate: string;
  arrivalDate: string;
  isReturnTrip: boolean;
  price: number;
}

export interface TripSearchParams {
  from: string;
  to: string;
  departureDate: string;
  arrivalDate?: string;
  isReturnTrip: boolean;
}

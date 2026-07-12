export interface AdminTrip {
  tripId: string;
  from: string;
  to: string;
  departureDate: string;
  arrivalDate: string;
  isReturnTrip: boolean;
  price: number;
  busId: string;
}

export interface AdminTripPage {
  items: AdminTrip[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface GenerateTripsResult {
  created: number;
  skipped: number;
  skippedNoBus: number;
}

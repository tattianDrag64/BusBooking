// EUR is the storage currency (RouteInfo.Price/Trip.Price on the backend) — it's
// always available even if the external rates API is unreachable (see
// CurrencyRateService.cs, fail-open fallback).
export const SUPPORTED_CURRENCIES = ["EUR", "MDL", "USD", "RON"] as const;
export type SupportedCurrency = (typeof SUPPORTED_CURRENCIES)[number];

export interface CurrencyRates {
  base: string;
  rates: Record<string, number>;
  asOf: string;
}

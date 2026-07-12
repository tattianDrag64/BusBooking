import { apiFetch } from "@/api/client";
import type { CurrencyRates } from "@/types/currency";

export function getCurrencyRates(): Promise<CurrencyRates> {
  return apiFetch<CurrencyRates>("/api/currency/rates");
}

import type { SupportedCurrency } from "@/types/currency";
import { SUPPORTED_CURRENCIES } from "@/types/currency";

const STORAGE_KEY = "pometco_currency";

export function saveCurrency(currency: SupportedCurrency): void {
  localStorage.setItem(STORAGE_KEY, currency);
}

export function getCurrency(): SupportedCurrency | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  return SUPPORTED_CURRENCIES.includes(raw as SupportedCurrency)
    ? (raw as SupportedCurrency)
    : null;
}

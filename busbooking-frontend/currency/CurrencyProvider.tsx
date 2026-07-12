"use client";

import {
  createContext,
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { useQuery } from "@tanstack/react-query";
import { getCurrencyRates } from "@/api/currency";
import { getCurrency, saveCurrency } from "./currencyStorage";
import type { SupportedCurrency } from "@/types/currency";

interface CurrencyContextValue {
  currency: SupportedCurrency;
  setCurrency: (currency: SupportedCurrency) => void;
  // `amountInEur` — RouteInfo.Price/Trip.Price are stored in EUR on the backend
  // (see CurrencyRateService.cs) — every price in the app is EUR at the source.
  format: (amountInEur: number) => string;
}

export const CurrencyContext = createContext<CurrencyContextValue | null>(null);

export function CurrencyProvider({ children }: { children: ReactNode }) {
  const [currency, setCurrencyState] = useState<SupportedCurrency>("EUR");

  useEffect(() => {
    const stored = getCurrency();
    if (stored) setCurrencyState(stored);
  }, []);

  const ratesQuery = useQuery({
    queryKey: ["currency", "rates"],
    queryFn: getCurrencyRates,
    // Rates are cached server-side for hours (CurrencyRateService.cs) — no
    // point refetching more often than that on the client.
    staleTime: 60 * 60 * 1000,
  });

  const setCurrency = useCallback((next: SupportedCurrency) => {
    setCurrencyState(next);
    saveCurrency(next);
  }, []);

  const format = useCallback(
    (amountInEur: number) => {
      const rate = ratesQuery.data?.rates[currency];
      // No rate yet (still loading) or currency missing from the API response
      // (fail-open fallback only returns EUR, see CurrencyRateService.cs) —
      // fall back to displaying the stored EUR amount rather than blocking on it.
      const amount = rate ? amountInEur * rate : amountInEur;
      const displayCurrency = rate ? currency : "EUR";

      try {
        return new Intl.NumberFormat(undefined, {
          style: "currency",
          currency: displayCurrency,
          maximumFractionDigits: 2,
        }).format(amount);
      } catch {
        return `${amount.toFixed(2)} ${displayCurrency}`;
      }
    },
    [currency, ratesQuery.data],
  );

  const value = useMemo(
    () => ({ currency, setCurrency, format }),
    [currency, setCurrency, format],
  );

  return (
    <CurrencyContext.Provider value={value}>
      {children}
    </CurrencyContext.Provider>
  );
}

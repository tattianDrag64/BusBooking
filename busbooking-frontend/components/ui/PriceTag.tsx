"use client";

import { useCurrency } from "@/currency/useCurrency";

// Thin client-component wrapper so server-rendered pages (routes/trips search
// results) can show a currency-converted price without becoming client
// components themselves — only this leaf needs useCurrency().
export function PriceTag({ amountInEur }: { amountInEur: number }) {
  const { format } = useCurrency();
  return <>{format(amountInEur)}</>;
}

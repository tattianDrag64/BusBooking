"use client";

import { useCurrency } from "@/currency/useCurrency";
import { SUPPORTED_CURRENCIES } from "@/types/currency";

export function CurrencySwitcher({ className = "" }: { className?: string }) {
  const { currency, setCurrency } = useCurrency();

  return (
    <div className={`flex items-center gap-1 text-sm font-semibold text-on-surface-variant ${className}`}>
      {SUPPORTED_CURRENCIES.map((code, index) => (
        <span key={code} className="flex items-center gap-1">
          {index > 0 && <span className="text-outline-variant">/</span>}
          <button
            type="button"
            onClick={() => setCurrency(code)}
            aria-current={currency === code}
            className={currency === code ? "text-primary" : "hover:text-primary"}
          >
            {code}
          </button>
        </span>
      ))}
    </div>
  );
}

"use client";

import { useLocale } from "next-intl";
import { usePathname, useRouter } from "@/i18n/navigation";
import { useSearchParams } from "next/navigation";
import { routing } from "@/i18n/routing";

const LOCALE_LABELS: Record<string, string> = {
  en: "EN",
  ru: "RU",
  ro: "RO",
};

export function LocaleSwitcher({ className = "" }: { className?: string }) {
  const locale = useLocale();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const router = useRouter();

  function switchTo(nextLocale: string) {
    const query = searchParams.toString();
    router.replace(`${pathname}${query ? `?${query}` : ""}`, {
      locale: nextLocale,
    });
  }

  return (
    <div className={`flex items-center gap-1 text-sm font-semibold text-on-surface-variant ${className}`}>
      {routing.locales.map((loc, index) => (
        <span key={loc} className="flex items-center gap-1">
          {index > 0 && <span className="text-outline-variant">/</span>}
          <button
            type="button"
            onClick={() => switchTo(loc)}
            aria-current={locale === loc}
            className={locale === loc ? "text-primary" : "hover:text-primary"}
          >
            {LOCALE_LABELS[loc] ?? loc.toUpperCase()}
          </button>
        </span>
      ))}
    </div>
  );
}

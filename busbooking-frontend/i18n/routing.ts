import { defineRouting } from "next-intl/routing";

export const routing = defineRouting({
  locales: ["en", "ru", "ro"],
  defaultLocale: "en",
  // Default locale (en) keeps unprefixed URLs (/routes) — existing links/SEO
  // stay valid; ru/ro get a prefix (/ru/routes, /ro/routes).
  localePrefix: "as-needed",
});

export type AppLocale = (typeof routing.locales)[number];

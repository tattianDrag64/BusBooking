import createMiddleware from "next-intl/middleware";
import { routing } from "./i18n/routing";

// Named proxy.ts, not middleware.ts — this Next.js version renamed the
// convention (see node_modules/next/dist/docs/.../internationalization.md).
export default createMiddleware(routing);

export const config = {
  matcher: [
    // Skip Next.js internals, static files, and API routes (this app has none
    // of its own — /api/* is the separate ASP.NET Core backend, not proxied
    // through here) from locale negotiation.
    "/((?!_next|.*\\..*).*)",
  ],
};

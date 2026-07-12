"use client";

import dynamic from "next/dynamic";
import type { RouteSummary } from "@/types/route";

// Leaflet touches `window`/`document` at import time, so this can only ever
// render on the client — ssr: false is only allowed here because this whole
// file is already a Client Component (see AGENTS.md re: Next.js constraints).
const LeafletRouteMap = dynamic(
  () => import("./LeafletRouteMap").then((mod) => mod.LeafletRouteMap),
  {
    ssr: false,
    loading: () => (
      <div className="h-full w-full animate-pulse rounded-3xl bg-surface-light" />
    ),
  },
);

export function RouteMap({ routes }: { routes: RouteSummary[] }) {
  return (
    <div className="h-[420px] w-full overflow-hidden rounded-3xl shadow-sm sm:h-[500px]">
      <LeafletRouteMap routes={routes} />
    </div>
  );
}

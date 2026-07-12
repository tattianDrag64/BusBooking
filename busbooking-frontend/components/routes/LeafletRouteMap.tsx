"use client";

import "leaflet/dist/leaflet.css";
import { MapContainer, TileLayer, Marker, Popup, Polyline } from "react-leaflet";
import { divIcon, type LatLngBoundsExpression, type LatLngTuple } from "leaflet";
import { coordinatesFor } from "@/lib/cityCoordinates";
import { waypointsFor } from "@/lib/routeWaypoints";
import type { RouteSummary } from "@/types/route";

const MARKER_ICON = divIcon({
  className: "",
  html: `<span class="relative flex h-3 w-3">
    <span class="absolute inline-flex h-full w-full animate-ping rounded-full bg-primary-container opacity-75"></span>
    <span class="relative inline-flex h-3 w-3 rounded-full bg-primary-container"></span>
  </span>`,
  iconSize: [12, 12],
  iconAnchor: [6, 6],
});

// Smaller, non-pulsing dot for decorative intermediate stops — visually
// distinct from the real departure/arrival markers above.
const WAYPOINT_ICON = divIcon({
  className: "",
  html: `<span class="block h-2 w-2 rounded-full border border-white bg-on-surface-variant/70 shadow-sm"></span>`,
  iconSize: [8, 8],
  iconAnchor: [4, 4],
});

const FALLBACK_CENTER: LatLngTuple = [46.5, 21]; // roughly the middle of the Balkans/Central Europe corridor

type Coord = [number, number];

export function LeafletRouteMap({ routes }: { routes: RouteSummary[] }) {
  const cities = new Map<string, Coord>();
  const waypointCities = new Map<string, Coord>();
  const lines: { path: Coord[]; key: string }[] = [];

  for (const route of routes) {
    const from = coordinatesFor(route.departureCity);
    const to = coordinatesFor(route.arrivalCity);

    if (from) cities.set(route.departureCity, from);
    if (to) cities.set(route.arrivalCity, to);

    if (from && to) {
      const waypoints = waypointsFor(route.departureCity, route.arrivalCity)
        .map((city) => {
          const position = coordinatesFor(city);
          if (position) waypointCities.set(city, position);
          return position;
        })
        .filter((position): position is Coord => position !== null);

      lines.push({ path: [from, ...waypoints, to], key: route.id });
    } else {
      // A city missing from CITY_COORDINATES shouldn't silently drop the whole
      // map — just skip drawing the line for this one route.
      console.warn(
        `RouteMap: missing coordinates for "${route.departureCity}" or "${route.arrivalCity}" — skipping line for route ${route.id}`,
      );
    }
  }

  const points = [...cities.values(), ...waypointCities.values()];
  const bounds: LatLngBoundsExpression | undefined =
    points.length > 0 ? (points as LatLngBoundsExpression) : undefined;

  return (
    <MapContainer
      center={FALLBACK_CENTER}
      zoom={5}
      bounds={bounds}
      boundsOptions={{ padding: [32, 32] }}
      scrollWheelZoom={false}
      className="h-full w-full"
    >
      <TileLayer
        attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />

      {lines.map((line) => (
        <Polyline
          key={line.key}
          positions={line.path}
          pathOptions={{ color: "#5e2ca2", weight: 2, dashArray: "6 6" }}
        />
      ))}

      {[...cities.entries()].map(([city, position]) => (
        <Marker key={city} position={position} icon={MARKER_ICON}>
          <Popup>{city}</Popup>
        </Marker>
      ))}

      {/* Decorative only — see lib/routeWaypoints.ts for why these aren't real stops. */}
      {[...waypointCities.entries()].map(([city, position]) => (
        <Marker key={city} position={position} icon={WAYPOINT_ICON}>
          <Popup>{city} (stop)</Popup>
        </Marker>
      ))}
    </MapContainer>
  );
}

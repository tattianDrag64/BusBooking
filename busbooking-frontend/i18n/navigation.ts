import { createNavigation } from "next-intl/navigation";
import { routing } from "./routing";

// Locale-aware drop-in replacements for next/link and next/navigation — every
// component that links or navigates within the app must import from here, not
// from "next/link"/"next/navigation" directly, or the locale prefix gets lost.
export const { Link, redirect, usePathname, useRouter, getPathname } =
  createNavigation(routing);

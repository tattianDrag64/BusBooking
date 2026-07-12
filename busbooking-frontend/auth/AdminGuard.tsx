"use client";

import { useEffect, type ReactNode } from "react";
import { useRouter } from "@/i18n/navigation";
import { useAuth } from "./useAuth";

// Gates the whole `/admin` section: not authenticated -> /login, authenticated
// but not an Admin -> / (showing the login form to an already-logged-in
// non-admin user would be pointless — they can't fix their role there).
export function AdminGuard({ children }: { children: ReactNode }) {
  const router = useRouter();
  const { user, isAuthenticated, isLoading } = useAuth();
  const isAdmin = isAuthenticated && user?.role === "Admin";

  useEffect(() => {
    if (isLoading) return;
    if (!isAuthenticated) {
      router.replace("/login?redirectTo=/admin");
    } else if (!isAdmin) {
      router.replace("/");
    }
  }, [isLoading, isAuthenticated, isAdmin, router]);

  if (isLoading || !isAdmin) {
    return (
      <div className="mx-auto max-w-[1280px] px-4 py-12 sm:px-12">
        <div className="h-32 w-full animate-pulse rounded-2xl bg-surface-light" />
      </div>
    );
  }

  return <>{children}</>;
}

"use client";

import { useEffect, useState } from "react";
import { useRouter } from "@/i18n/navigation";
import { ApiError } from "@/api/client";
import { createCheckoutSession } from "@/api/payments";
import { useAuth } from "@/auth/useAuth";

export function useCheckoutPageVM(orderId: string) {
  const router = useRouter();
  const { isAuthenticated, isLoading: authLoading } = useAuth();

  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (authLoading) {
      return;
    }
    if (!isAuthenticated) {
      router.replace(`/login?redirectTo=/checkout/${orderId}`);
      return;
    }

    let cancelled = false;
    createCheckoutSession(orderId)
      .then(({ checkoutUrl }) => {
        if (!cancelled) {
          window.location.href = checkoutUrl;
        }
      })
      .catch((err: unknown) => {
        if (cancelled) {
          return;
        }
        setError(
          err instanceof ApiError
            ? err.message
            : "Could not start checkout. Please try again.",
        );
      });

    return () => {
      cancelled = true;
    };
  }, [authLoading, isAuthenticated, orderId, router]);

  return { error };
}

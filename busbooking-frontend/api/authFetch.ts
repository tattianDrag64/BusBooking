import { apiFetch, ApiError } from "@/api/client";
import { getTokens } from "@/auth/tokenStorage";

// For JWT-protected endpoints, called only from client components. Attaches the
// stored access token and redirects to /login on 401 (no refresh (i.e. it doesn't
// retry with a refreshed token) is intentionally deferred — see FRONTEND_AUTH_PLAN.md).
export async function authFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const tokens = getTokens();
  if (!tokens) {
    redirectToLogin();
    throw new ApiError(401, "Not authenticated.");
  }

  try {
    return await apiFetch<T>(path, {
      ...init,
      headers: {
        ...init?.headers,
        Authorization: `Bearer ${tokens.accessToken}`,
      },
    });
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) {
      redirectToLogin();
    }
    throw error;
  }
}

function redirectToLogin(): void {
  if (typeof window === "undefined") {
    return;
  }
  const redirectTo = encodeURIComponent(
    window.location.pathname + window.location.search,
  );
  window.location.href = `/login?redirectTo=${redirectTo}`;
}

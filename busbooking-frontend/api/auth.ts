import { apiFetch } from "@/api/client";
import type { SignInPayload, SignUpPayload, TokenResponse } from "@/types/auth";

export function signIn(payload: SignInPayload): Promise<TokenResponse> {
  return apiFetch<TokenResponse>("/api/auth/signin", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
}

export function signUp(payload: SignUpPayload): Promise<TokenResponse> {
  return apiFetch<TokenResponse>("/api/auth/signup", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
}

export function refreshToken(refreshToken: string): Promise<TokenResponse> {
  return apiFetch<TokenResponse>("/api/auth/refresh", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ refreshToken }),
  });
}

export function revoke(
  accessToken: string,
  refreshTokenValue: string,
): Promise<void> {
  return apiFetch<void>("/api/auth/revoke", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${accessToken}`,
    },
    body: JSON.stringify({ refreshToken: refreshTokenValue }),
  });
}
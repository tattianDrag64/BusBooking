import type { AuthUser } from "@/types/auth";

// ASP.NET Core serializes System.Security.Claims.ClaimTypes constants as
// their full URI form in the JWT payload, e.g. ClaimTypes.NameIdentifier ->
// "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier".
const CLAIM_URI = {
  id: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
  username: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
  email: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
  role: "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
} as const;

function base64UrlDecode(segment: string): string {
  const padded = segment.replace(/-/g, "+").replace(/_/g, "/");
  const withPadding = padded.padEnd(
    padded.length + ((4 - (padded.length % 4)) % 4),
    "=",
  );
  return decodeURIComponent(
    atob(withPadding)
      .split("")
      .map((char) => "%" + char.charCodeAt(0).toString(16).padStart(2, "0"))
      .join(""),
  );
}

export function decodeAccessToken(accessToken: string): AuthUser | null {
  const parts = accessToken.split(".");
  if (parts.length !== 3) return null;

  try {
    const payload = JSON.parse(base64UrlDecode(parts[1])) as Record<
      string,
      unknown
    >;
    const claim = (uri: string, shortKey: string) =>
      (payload[uri] ?? payload[shortKey]) as string | undefined;

    const id = claim(CLAIM_URI.id, "nameid");
    const email = claim(CLAIM_URI.email, "email");
    if (!id || !email) return null;

    return {
      id,
      email,
      username: claim(CLAIM_URI.username, "unique_name") ?? "",
      role: claim(CLAIM_URI.role, "role") ?? "",
    };
  } catch {
    return null;
  }
}

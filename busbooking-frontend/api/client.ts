const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

export async function apiFetch<T>(
  path: string,
  init?: RequestInit,
): Promise<T> {
  const res = await fetch(`${API_BASE_URL}${path}`, init);

  if (!res.ok) {
    // Backend error responses are `{ message: string }` (see e.g. PaymentsController's
    // 502) — surface that when present instead of just the status code.
    const serverMessage = await res
      .clone()
      .json()
      .then((body: unknown) =>
        typeof body === "object" && body !== null && "message" in body
          ? String((body as { message: unknown }).message)
          : null,
      )
      .catch(() => null);
    throw new ApiError(res.status, serverMessage ?? `API ${path} failed: ${res.status}`);
  }

  if (res.status === 204 || res.headers.get("content-length") === "0") {
    return undefined as T;
  }

  return res.json() as Promise<T>;
}

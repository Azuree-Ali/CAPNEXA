import type { AuthResponseDto } from "@/features/auth/types/identity.types";
export const connected =
  import.meta.env.VITE_AUTH_MODE === "live" ||
  (import.meta.env.PROD && import.meta.env.VITE_AUTH_MODE !== "preview");
let expired = false;
export const sessionExpired = () => expired;
function expireSession() {
  setSession(null, true);
}
let session: AuthResponseDto | null = null;
let expiry = 0;
const listeners = new Set<() => void>();
let timer: ReturnType<typeof setTimeout> | undefined;
export function setSession(next: AuthResponseDto | null, isExpired = false) {
  expired = isExpired;
  clearTimeout(timer);
  session = next;
  expiry = next ? Date.now() + next.expiresInSeconds * 1000 : 0;
  if (next) timer = setTimeout(expireSession, Math.min(next.expiresInSeconds * 1000, 2147483647));
  listeners.forEach((notify) => notify());
}
export const currentUser = () => session?.user ?? null;
export function subscribeSession(notify: () => void) {
  listeners.add(notify);
  return () => {
    listeners.delete(notify);
  };
}
export function isObject(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}
export class ApiHttpError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.status = status;
    this.name = "ApiHttpError";
  }
}

function responseMessage(data: unknown, status: number): string {
  if (isObject(data)) {
    if (typeof data.message === "string" && data.message.trim()) return data.message;
    if (isObject(data.errors)) {
      for (const messages of Object.values(data.errors)) {
        if (!Array.isArray(messages)) continue;
        const first = messages.find(
          (message: unknown): message is string => typeof message === "string" && !!message.trim(),
        );
        if (first) return first;
      }
    }
  }
  return "Request failed (HTTP " + status + ").";
}

export async function apiRequest<T>(
  path: string,
  method: "GET" | "PUT" | "POST",
  validate: (value: unknown) => value is T,
  body?: unknown,
  signal?: AbortSignal,
): Promise<T> {
  const origin = import.meta.env.VITE_API_BASE_URL?.trim();
  if (!connected || !origin) throw new Error("Live API access is not configured.");
  const url = new URL(path, origin);
  if (
    url.protocol !== "https:" &&
    !(url.protocol === "http:" && ["localhost", "127.0.0.1", "[::1]"].includes(url.hostname))
  )
    throw new Error("Use HTTPS for the API origin.");
  const headers = new Headers({ Accept: "application/json" });
  if (body !== undefined) headers.set("Content-Type", "application/json");
  const requiresSession = path.startsWith("/api/Admin/");
  const active = requiresSession ? session : null;
  if (requiresSession) {
    if (!active || Date.now() >= expiry) {
      expireSession();
      throw new Error("Sign in again to continue.");
    }
    headers.set("Authorization", "Bearer " + active.accessToken);
  }
  const response = await fetch(url, {
    method,
    headers,
    signal,
    redirect: "error",
    credentials: "omit",
    cache: "no-store",
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  const data: unknown =
    response.status === 204 ? undefined : await response.json().catch(() => undefined);
  if (!response.ok) {
    if (response.status === 401 && active && active === session) expireSession();
    throw new ApiHttpError(response.status, responseMessage(data, response.status));
  }
  if (!validate(data)) throw new Error("Unexpected API response. Verify the backend revision.");
  return data;
}

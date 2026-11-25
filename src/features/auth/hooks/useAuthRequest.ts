import { useLayoutEffect, useRef, useState } from "react";
import type { PreviewResult } from "../types/auth.types";
export function useAuthRequest<T>(
  request: (values: T, signal: AbortSignal) => Promise<PreviewResult>,
) {
  const [pending, setPending] = useState(false);
  const [error, setError] = useState<string>();
  const [result, setResult] = useState<PreviewResult>();
  const active = useRef<AbortController | null>(null);
  useLayoutEffect(() => {
    return () => {
      active.current?.abort();
      active.current = null;
    };
  }, []);
  async function submit(values: T) {
    if (active.current) return;
    const attempt = new AbortController();
    active.current = attempt;
    setPending(true);
    setError(undefined);
    setResult(undefined);
    try {
      const response = await request(values, attempt.signal);
      if (active.current === attempt) setResult(response);
    } catch (cause) {
      if (active.current === attempt)
        setError(cause instanceof Error ? cause.message : "An unexpected error occurred.");
    } finally {
      if (active.current === attempt) {
        active.current = null;
        setPending(false);
      }
    }
  }
  function clearFeedback() {
    setError(undefined);
    setResult(undefined);
  }
  return { submit, pending, error, result, clearFeedback };
}

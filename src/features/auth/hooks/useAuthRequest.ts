import { useEffect, useRef, useState } from "react";
import type { PreviewResult } from "../types/auth.types";
export function useAuthRequest<T>(request: (values: T) => Promise<PreviewResult>) {
  const [pending, setPending] = useState(false);
  const [error, setError] = useState<string>();
  const [result, setResult] = useState<PreviewResult>();
  const active = useRef(false);
  const mounted = useRef(false);
  useEffect(() => { mounted.current = true; return () => { mounted.current = false; }; }, []);
  async function submit(values: T) {
    if (active.current) return;
    active.current = true; setPending(true); setError(undefined); setResult(undefined);
    try {
      const response = await request(values);
      if (mounted.current) setResult(response);
    } catch (cause) {
      if (mounted.current) setError(cause instanceof Error ? cause.message : "An unexpected error occurred.");
    } finally {
      active.current = false;
      if (mounted.current) setPending(false);
    }
  }
  function clearFeedback() { setError(undefined); setResult(undefined); }
  return { submit, pending, error, result, clearFeedback };
}
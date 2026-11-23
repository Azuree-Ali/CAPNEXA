import { useEffect, useRef, useState } from "react";
import type { PortalRole } from "../../../config/navigation";
import { getProfile, saveProfile } from "../api/profile.api";
import type { Profile, ProfileEditable } from "../types/profile.types";
export function useProfile(role: PortalRole) {
  const [profile, setProfile] = useState<Profile>();
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);
  const [attempt, setAttempt] = useState(0);
  const request = useRef<AbortController | null>(null);
  const alive = useRef(false);
  const busy = useRef(false);
  useEffect(() => {
    alive.current = true;
    const controller = new AbortController();
    getProfile(role, controller.signal)
      .then((value) => {
        if (!controller.signal.aborted) {
          setProfile(value);
          setError("");
        }
      })
      .catch((cause) => {
        if (!controller.signal.aborted)
          setError(cause instanceof Error ? cause.message : "Unable to load profile.");
      });
    return () => {
      alive.current = false;
      controller.abort();
      request.current?.abort();
    };
  }, [role, attempt]);
  async function save(changes: ProfileEditable) {
    if (busy.current) return;
    busy.current = true;
    setSaving(true);
    setError("");
    const controller = new AbortController();
    request.current = controller;
    try {
      const value = await saveProfile(role, changes, controller.signal);
      if (alive.current && !controller.signal.aborted) {
        setProfile(value);
        return value;
      }
    } catch (cause) {
      if (alive.current && !controller.signal.aborted)
        setError(cause instanceof Error ? cause.message : "Unable to save changes.");
    } finally {
      busy.current = false;
      if (alive.current) setSaving(false);
    }
  }
  return {
    profile: profile?.role === role ? profile : undefined,
    error,
    saving,
    save,
    retry: () => setAttempt((n) => n + 1),
  };
}

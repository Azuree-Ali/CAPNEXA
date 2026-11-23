import type { PortalRole } from "../../../config/navigation";
import { profileFixtures } from "../mocks/profile.mock";
import type { Profile, ProfileEditable } from "../types/profile.types";
const storageKey = (role: PortalRole) => `capnexa:profile-preview:v1:${role}`;
function checkAbort(signal?: AbortSignal) {
  signal?.throwIfAborted();
}
function isEditable(value: unknown): value is ProfileEditable {
  if (!value || typeof value !== "object") return false;
  const v = value as Partial<ProfileEditable>;
  return (
    typeof v.officeLocation === "string" &&
    v.officeLocation.length <= 160 &&
    typeof v.officeHours === "string" &&
    v.officeHours.length <= 160 &&
    Array.isArray(v.interests) &&
    v.interests.length <= 20 &&
    v.interests.every((t) => typeof t === "string" && t.length > 0 && t.length <= 80) &&
    !!v.preferences &&
    ["daily", "weekly", "off"].includes(v.preferences.digest) &&
    ["immediate", "digest", "off"].includes(v.preferences.alerts) &&
    v.preferences.locale === "English (US)" &&
    v.preferences.theme === "Light Mode"
  );
}
function pickEditable(value: ProfileEditable): ProfileEditable {
  return {
    officeLocation: value.officeLocation.trim(),
    officeHours: value.officeHours.trim(),
    interests: [...value.interests],
    preferences: { ...value.preferences },
  };
}
// BACKEND: Replace this adapter with the agreed authenticated GET request. Resolve
// current user and role on the server; the route's role is only a preview fixture.
export async function getProfile(role: PortalRole, signal?: AbortSignal): Promise<Profile> {
  checkAbort(signal);
  const base = structuredClone(profileFixtures[role]);
  try {
    const raw = sessionStorage.getItem(storageKey(role));
    if (raw) {
      const saved: unknown = JSON.parse(raw);
      if (isEditable(saved)) return { ...base, ...pickEditable(saved) };
    }
  } catch {
    /* Preview storage is optional; load fixtures when unavailable or corrupt. */
  }
  return base;
}
// BACKEND: Replace preview storage with the agreed update API. Only send editable
// fields; enforce permissions and validation on the server. Add concurrency/version
// handling and map field errors. Never submit institutional identity or role updates.
export async function saveProfile(
  role: PortalRole,
  changes: ProfileEditable,
  signal?: AbortSignal,
): Promise<Profile> {
  checkAbort(signal);
  if (!isEditable(changes)) throw new Error("Check your profile details and try again.");
  const editable = pickEditable(changes);
  try {
    sessionStorage.setItem(storageKey(role), JSON.stringify(editable));
  } catch {
    throw new Error(
      "Changes could not be saved in this browser. Please allow site storage and try again.",
    );
  }
  checkAbort(signal);
  return { ...structuredClone(profileFixtures[role]), ...editable };
}

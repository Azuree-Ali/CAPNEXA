import type { PortalRole } from "../../../config/navigation";
export type ProfileTab = "personal" | "security" | "preferences";
export type DigestSchedule = "daily" | "weekly" | "off";
export type SubmissionAlerts = "immediate" | "digest" | "off";
export interface ProfilePreferences {
  digest: DigestSchedule;
  alerts: SubmissionAlerts;
  locale: "English (US)";
  theme: "Light Mode";
}
export interface ProfileEditable {
  officeLocation: string;
  officeHours: string;
  interests: string[];
  preferences: ProfilePreferences;
}
export interface Profile extends ProfileEditable {
  role: PortalRole;
  name: string;
  initials: string;
  title: string;
  firstName: string;
  lastName: string;
  email: string;
  department: string;
  institutionId: string;
  accessScope?: string;
}
export const digestLabels: Record<DigestSchedule, string> = {
  daily: "Daily 08:00 EST",
  weekly: "Weekly Monday 08:00 EST",
  off: "Off",
};
export const alertLabels: Record<SubmissionAlerts, string> = {
  immediate: "Immediate",
  digest: "Digest only",
  off: "Off",
};

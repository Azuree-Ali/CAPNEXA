import {
  LayoutDashboard,
  GraduationCap,
  UsersRound,
  Folder,
  FileChartColumn,
  ClipboardList,
  Flag,
  FileText,
  CircleCheck,
  ChartNoAxesCombined,
  History,
  Sun,
  UserRound,
  ScanSearch,
  Bell,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";
export type PortalRole = "Admin" | "Supervisor";
export interface NavigationItem {
  label: string;
  path: string;
  icon: LucideIcon;
  available?: boolean;
  badge?: number;
}
export interface NavigationGroup {
  label?: string;
  items: NavigationItem[];
}
export interface PortalIdentity {
  name: string;
  initials: string;
  title: string;
}
// TODO(BACKEND): Academic year, current user and badge counts must come from the server.
// These identities and counts are visual fixtures only, never authentication data.
export const previewIdentity: Record<PortalRole, PortalIdentity> = {
  Admin: { name: "Dr. Omar Hassan", initials: "OH", title: "System Administrator" },
  Supervisor: { name: "Dr. Olivia Bennett", initials: "OB", title: "Faculty Supervisor" },
};
export const navigation: Record<PortalRole, NavigationGroup[]> = {
  Admin: [
    { items: [{ label: "Overview", path: "dashboard", icon: LayoutDashboard }] },
    {
      label: "ACADEMIC MANAGEMENT",
      items: [
        { label: "Students", path: "students", icon: GraduationCap },
        { label: "Supervisors", path: "supervisors", icon: UsersRound },
        { label: "Projects", path: "projects", icon: Folder },
        { label: "Proposals", path: "proposals", icon: FileChartColumn },
        { label: "Assignments", path: "assignments", icon: ClipboardList },
        { label: "Milestones", path: "milestones", icon: Flag },
      ],
    },
    {
      label: "OPERATIONS",
      items: [
        { label: "Submissions", path: "submissions", icon: FileText },
        { label: "Evaluations", path: "evaluations", icon: CircleCheck },
        { label: "Reports", path: "reports", icon: ChartNoAxesCombined },
      ],
    },
    {
      label: "ADMINISTRATION",
      items: [
        { label: "Audit Logs", path: "audit-logs", icon: History },
        { label: "Settings", path: "settings", icon: Sun },
        { label: "Profile", path: "profile", icon: UserRound, available: true },
      ],
    },
  ],
  Supervisor: [
    {
      items: [
        { label: "Dashboard", path: "dashboard", icon: LayoutDashboard },
        { label: "My Projects", path: "projects", icon: Folder },
        { label: "Submissions", path: "submissions", icon: FileText },
        { label: "Reviews", path: "reviews", icon: ClipboardList, badge: 2 },
        { label: "Evaluations", path: "evaluations", icon: CircleCheck },
        { label: "Similarity Reports", path: "similarity", icon: ScanSearch },
        { label: "Notifications", path: "notifications", icon: Bell },
        { label: "Profile", path: "profile", icon: UserRound, available: true },
      ],
    },
  ],
};

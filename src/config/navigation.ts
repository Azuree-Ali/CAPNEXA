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
  Settings,
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
// These identities and counts are visual fixtures only, never authentication data.
export const previewIdentity: Record<PortalRole, PortalIdentity> = {
  Admin: { name: "Dr. Omar Hassan", initials: "OH", title: "System Administrator" },
  Supervisor: { name: "Dr. Olivia Bennett", initials: "OB", title: "Faculty Supervisor" },
};
export const navigation: Record<PortalRole, NavigationGroup[]> = {
  Admin: [
    { items: [{ label: "Overview", path: "dashboard", available: true, icon: LayoutDashboard }] },
    {
      label: "ACADEMIC MANAGEMENT",
      items: [
        { label: "Students", path: "students", available: true, icon: GraduationCap },
        { label: "Supervisors", path: "supervisors", available: true, icon: UsersRound },
        { label: "Projects", path: "projects", available: true, icon: Folder },
        { label: "Proposals", path: "proposals", available: true, icon: FileChartColumn },
        { label: "Assignments", path: "assignments", available: true, icon: ClipboardList },
        { label: "Milestones", path: "milestones", available: true, icon: Flag },
      ],
    },
    {
      label: "OPERATIONS",
      items: [
        { label: "Submissions", path: "submissions", available: true, icon: FileText },
        { label: "Evaluations", path: "evaluations", available: true, icon: CircleCheck },
        { label: "Reports", path: "reports", available: true, icon: ChartNoAxesCombined },
      ],
    },
    {
      label: "ADMINISTRATION",
      items: [
        { label: "Audit Logs", path: "audit-logs", available: true, icon: History },
        { label: "Settings", path: "settings", available: true, icon: Settings },
        { label: "Profile", path: "profile", available: true, icon: UserRound },
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

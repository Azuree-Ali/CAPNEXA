export type Permission = "allowed" | "assigned" | "restricted";
export interface SettingsPermissionsContent {
  title: string;
  description: string;
  parent: { label: string; route: string };
  search: string;
  area: string;
  all: string;
  capability: string;
  roles: readonly ["Admin", "Supervisor", "Student"];
  legend: Record<Permission, string>;
  entries: readonly {
    capability: string;
    area: string;
    grants: readonly [Permission, Permission, Permission];
  }[];
}
export const settingsPermissionsContent: SettingsPermissionsContent = {
  title: "Roles and permissions",
  description: "Review the role matrix. Server authorization governs live access.",
  parent: { label: "Academic configuration", route: "/admin/settings" },
  search: "Find a capability",
  area: "Capability area",
  all: "All areas",
  capability: "Capability",
  roles: ["Admin", "Supervisor", "Student"],
  legend: { allowed: "Allowed", assigned: "Assigned or own records", restricted: "Restricted" },
  entries: [
    {
      capability: "Review proposals",
      area: "Projects",
      grants: ["allowed", "restricted", "restricted"],
    },
    {
      capability: "Manage allocations",
      area: "Projects",
      grants: ["allowed", "restricted", "restricted"],
    },
    {
      capability: "Review assigned work",
      area: "Delivery",
      grants: ["allowed", "assigned", "restricted"],
    },
    {
      capability: "Submit deliverables",
      area: "Delivery",
      grants: ["restricted", "restricted", "assigned"],
    },
    {
      capability: "Manage academic configuration",
      area: "Administration",
      grants: ["allowed", "restricted", "restricted"],
    },
    {
      capability: "View institutional reports and audit logs",
      area: "Administration",
      grants: ["allowed", "restricted", "restricted"],
    },
  ],
};

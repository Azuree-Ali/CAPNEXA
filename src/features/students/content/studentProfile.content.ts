export interface StudentRecord {
  id: string;
  fullName: string;
  email: string;
  department: string;
  year: string;
  role: string;
  team: string;
  project: string;
  active: boolean;
}
export const exampleStudent: StudentRecord = {
  id: "alex-morgan",
  fullName: "Alex Morgan",
  email: "alex@example.edu",
  department: "Computer Science",
  year: "2025–2026",
  role: "Team lead",
  team: "Atlas",
  project: "Campus Atlas",
  active: true,
};
export const studentForm = {
  add: "Add student",
  edit: "Edit student details",
  team: "Reassign team",
  project: "Reassign project",
  fullName: "Full name",
  email: "Institutional email",
  department: "Department",
  year: "Academic year",
  role: "Team role",
  id: "Student ID",
  teamLabel: "Team",
  projectLabel: "Project",
  save: "Save changes",
  cancel: "Cancel",
  close: "Close",
  back: "Student directory",
  detail: "Student profile",
  empty: "Student record not found.",
  invalid: "Enter a name and a unique institutional email.",
  active: "Active",
  inactive: "Inactive",
  change: "Change account status",
  status: "Account status",
  preview: "Demonstration workspace · Changes stay in this tab until reload.",
  teams: ["Atlas", "Lens", "Maps"],
  projects: ["Campus Atlas", "Library Lens", "Campus Maps"],
  roles: ["Team lead", "Member"],
} as const;

export interface StudentAcademicContext {
  displayId: string;
  context: string;
  timeline: string;
  timelineRoute: string;
  currentTeam: string;
  currentProject: string;
}
export const studentAcademicContext: StudentAcademicContext = {
  displayId: "ST-001",
  context: "Academic progress",
  timeline: "View project milestones",
  timelineRoute: "/admin/milestones/timeline",
  currentTeam: "Current team",
  currentProject: "Current project",
};

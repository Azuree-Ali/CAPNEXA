import type { RegisterActions } from "@/components/forms/EditableRegister";
import type { WorkspaceContent } from "@/components/data-table/workspace.content";
export interface MilestonesContent extends WorkspaceContent {
  title: string;
}
export const milestonesContent: MilestonesContent = {
  title: "Milestone management",
  description: "Review the delivery schedule for the academic cycle.",
  columns: ["Milestone", "Due date", "Project"],
  records: [
    {
      cells: ["Design specification", "2025-12-15", "Campus Atlas"],
      route: "/admin/milestones/timeline",
    },
    {
      cells: ["Prototype review", "2026-01-20", "Campus Atlas"],
      route: "/admin/milestones/timeline",
    },
  ],
  links: [],
};

export const milestonesActions: RegisterActions = {
  add: "Add milestone",
  edit: "Edit milestone",
  save: "Save preview changes",
  cancel: "Cancel",
  notice: "Changes are local to this preview.",
  readOnlyNotice:
    "Milestone editing awaits role ownership and an approved API contract. Use this view to monitor the sample schedule.",
  saved: "Milestone saved in this preview.",
  projectFilter: "Project filter",
  allProjects: "All projects",
  template: "Milestone template",
  custom: "Custom milestone",
  templates: [
    { name: "Design review", values: ["Design review", "", "Campus Atlas"] },
    { name: "Final defense", values: ["Final defense", "", "Campus Atlas"] },
  ],
  duplicate: "Enter a unique name and complete all fields.",
};

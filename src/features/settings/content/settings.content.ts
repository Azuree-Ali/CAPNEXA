export interface AcademicSection {
  id: string;
  title: string;
  description: string;
  fields: readonly {
    id: string;
    label: string;
    type: "text" | "number" | "date";
    min?: number;
    max?: number;
  }[];
  rows: readonly { id: string; values: readonly string[] }[];
}
export interface SettingsContent {
  title: string;
  description: string;
  navigation: string;
  permissions: { label: string; route: string };
  add: string;
  edit: string;
  cancel: string;
  save: string;
  saved: string;
  invalid: string;
  empty: string;
  notice: string;
  sections: readonly AcademicSection[];
}
export const settingsContent: SettingsContent = {
  title: "Academic configuration",
  description: "Manage the capstone cycle, academic units, quotas, policies, and templates.",
  navigation: "Configuration sections",
  permissions: { label: "Roles and permissions", route: "/admin/settings/permissions" },
  add: "Add record",
  edit: "Edit",
  cancel: "Cancel",
  save: "Save preview changes",
  saved: "Changes saved in this preview.",
  invalid:
    "Complete all fields, use a unique name, and make sure any end date follows the start date.",
  empty: "No records in this section.",
  notice: "Changes stay in this tab until reload.",
  sections: [
    {
      id: "years",
      title: "Academic years",
      description: "Set the dates used to organise capstone cohorts.",
      fields: [
        { id: "name", label: "Academic year", type: "text" },
        { id: "start", label: "Start date", type: "date" },
        { id: "end", label: "End date", type: "date" },
      ],
      rows: [{ id: "year-current", values: ["2025–2026", "2025-09-01", "2026-06-30"] }],
    },
    {
      id: "departments",
      title: "Departments",
      description: "Maintain the academic units responsible for project cohorts.",
      fields: [
        { id: "name", label: "Department", type: "text" },
        { id: "code", label: "Department code", type: "text" },
        { id: "coordinator", label: "Coordinator", type: "text" },
      ],
      rows: [
        { id: "dept-cs", values: ["Computer Science", "CS", "Dr. Omar Hassan"] },
        { id: "dept-is", values: ["Information Systems", "IS", "Dr. Sam Reed"] },
      ],
    },
    {
      id: "quotas",
      title: "Quotas",
      description: "Set sample limits for capstone teams and supervisor workloads.",
      fields: [
        { id: "name", label: "Quota", type: "text" },
        { id: "limit", label: "Limit", type: "number", min: 1, max: 1000 },
        { id: "scope", label: "Applies to", type: "text" },
      ],
      rows: [
        { id: "quota-team", values: ["Default team size", "4", "Capstone teams"] },
        {
          id: "quota-supervisor",
          values: ["Maximum projects per supervisor", "3", "Faculty supervisors"],
        },
      ],
    },
    {
      id: "policies",
      title: "Policies",
      description: "Record the rules used during proposal review and submission.",
      fields: [
        { id: "name", label: "Policy", type: "text" },
        { id: "rule", label: "Rule", type: "text" },
        { id: "scope", label: "Applies to", type: "text" },
      ],
      rows: [
        {
          id: "policy-proposal",
          values: [
            "Proposal approval",
            "An approved proposal is required before supervisor assignment.",
            "Capstone projects",
          ],
        },
      ],
    },
    {
      id: "templates",
      title: "Templates",
      description: "Define templates that standardise capstone deliverables.",
      fields: [
        { id: "name", label: "Template", type: "text" },
        { id: "deliverable", label: "Deliverable", type: "text" },
        { id: "sections", label: "Required sections", type: "text" },
      ],
      rows: [
        {
          id: "template-design",
          values: [
            "Design specification",
            "Design brief",
            "Objectives, architecture, verification plan",
          ],
        },
      ],
    },
  ],
};

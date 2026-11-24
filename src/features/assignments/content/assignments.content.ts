import type { OperationListContent } from "@/features/dashboard/types/operations.types";
import type { WorkspaceContent } from "@/components/data-table/workspace.content";
export interface AssignmentsContent extends WorkspaceContent {
  live: OperationListContent;
}
export const assignmentsContent: AssignmentsContent = {
  live: {
    title: "Supervisor assignments",
    description: "Allocate approved projects to active supervisors with available capacity.",
    columns: ["Approved project", "Team", "Team leader", "Reviewed on"],
  },
  title: "Supervisor assignment",
  description: "Match approved projects with available supervisors.",
  columns: ["Project", "Team", "Supervisor"],
  records: [
    {
      cells: ["Campus Maps", "Maps", "Unassigned"],
    },
  ],
  links: [
    {
      label: "Compare recommended supervisors",
      route: "/admin/recommendations",
    },
  ],
};

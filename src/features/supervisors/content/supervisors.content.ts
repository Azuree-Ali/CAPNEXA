import type { OperationListContent } from "@/features/dashboard/types/operations.types";
import type { WorkspaceContent } from "@/components/data-table/workspace.content";
export interface SupervisorsContent extends WorkspaceContent {
  live: OperationListContent;
}
export const supervisorsContent: SupervisorsContent = {
  live: {
    title: "Supervisor directory",
    description: "Inspect faculty expertise, project load, and available supervision capacity.",
    columns: [
      "Supervisor",
      "Department",
      "Specialization",
      "Assigned / maximum",
      "Available",
      "Account",
    ],
  },
  title: "Supervisor directory",
  description: "Review faculty expertise and available supervision capacity.",
  columns: ["Supervisor", "Specialization", "Capacity"],
  records: [
    {
      cells: ["Dr. Olivia Bennett", "Distributed systems", "1 of 3 assigned"],
      route: "/admin/supervisors/olivia-bennett",
    },
  ],
  links: [],
};

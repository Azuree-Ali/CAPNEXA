import type { OperationListContent } from "@/features/dashboard/types/operations.types";
import type { WorkspaceContent } from "@/components/data-table/workspace.content";
export interface ProposalsContent extends WorkspaceContent {
  live: OperationListContent;
}
export const proposalsContent: ProposalsContent = {
  live: {
    title: "Proposal review",
    description: "Review submitted proposals and record the administrative decision.",
    columns: ["Proposal", "Team", "Team leader", "Submitted", "Status"],
  },
  title: "Proposal queue",
  description: "Inspect submitted project proposals and record a decision.",
  columns: ["Proposal", "Team", "Status"],
  records: [
    {
      cells: ["Library Lens", "Lens", "Pending review"],
      route: "/admin/proposals/review",
    },
  ],
  links: [],
};

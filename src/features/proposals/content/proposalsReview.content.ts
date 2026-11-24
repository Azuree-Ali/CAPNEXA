import type { WorkspaceContent } from "@/components/data-table/workspace.content";
export interface ProposalsReviewContent extends WorkspaceContent {
  title: string;
}
export const proposalsReviewContent: ProposalsReviewContent = {
  title: "Proposal review",
  description: "Library Lens · Team Lens",
  columns: ["Proposal section", "Evidence", "Review focus"],
  records: [
    {
      cells: ["Objective", "Find available study spaces", "Campus accessibility"],
    },
    {
      cells: ["Deliverable", "Room availability dashboard", "Data freshness"],
    },
    {
      cells: ["Evaluation", "Student usability study", "Task completion"],
    },
  ],
  links: [],
};
export interface DecisionCopy {
  approve: string;
  reject: string;
  revisions: string;
  cancel: string;
  confirm: string;
  reason: string;
  assign: string;
  capacity: string;
  pending: string;
  approved: string;
  rejected: string;
  returned: string;
  demo: string;
  faculty: string;
  unassigned: string;
}
export const decisions: DecisionCopy = {
  approve: "Approve proposal",
  reject: "Reject proposal",
  revisions: "Request revisions",
  cancel: "Cancel",
  confirm: "Confirm",
  reason: "Review notes",
  assign: "Assign supervisor",
  capacity: "Available supervisors",
  pending: "Pending review",
  approved: "Approved",
  rejected: "Rejected",
  returned: "Revisions requested",
  demo: "Preview only · No request will be sent.",
  faculty: "Dr. Olivia Bennett",
  unassigned: "Unassigned",
};

export interface ProposalDossierContent {
  name: string;
  team: string;
  parent: { label: string; route: string };
  evidence: string;
  history: string;
  attachments: readonly { name: string; detail: string }[];
  historyEntries: readonly string[];
  guidance: Record<"approve" | "reject" | "revisions", string>;
  requiredReason: string;
}
export const proposalDossier: ProposalDossierContent = {
  name: "Library Lens",
  team: "Lens",
  parent: { label: "Proposal queue", route: "/admin/proposals" },
  evidence: "Proposal attachments",
  history: "Review history",
  attachments: [
    {
      name: "Project scope",
      detail: "The project maps accessible study rooms and publishes their availability.",
    },
    {
      name: "Evaluation plan",
      detail: "The team will measure task completion during a student usability study.",
    },
  ],
  historyEntries: [
    "Team Lens submitted the proposal on 10 December.",
    "The proposal entered the administrative review queue.",
  ],
  guidance: {
    approve:
      "Confirm that the scope, deliverable, and evaluation plan are ready for supervisor assignment.",
    reject: "Record why the proposal cannot proceed.",
    revisions: "Describe the changes the team should make before resubmitting.",
  },
  requiredReason: "Enter a reason before recording this decision.",
};

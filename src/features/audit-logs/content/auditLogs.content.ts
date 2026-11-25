export interface AuditLogContent {
  title: string;
  description: string;
  category: string;
  all: string;
  search: string;
  fields: readonly [string, string, string, string];
  events: readonly {
    id: string;
    action: string;
    category: string;
    actor: string;
    target: string;
    date: string;
    detail: string;
    route: string;
  }[];
}
export const auditLogsContent: AuditLogContent = {
  title: "Audit logs",
  description: "Review example academic events and trace them to the associated records.",
  category: "Event category",
  all: "All categories",
  search: "Filter audit events",
  fields: ["Actor", "Resource", "Recorded on", "Evidence"],
  events: [
    {
      id: "EV-026",
      action: "Proposal submitted",
      category: "Proposals",
      actor: "Team Lens",
      target: "Library Lens",
      date: "10 December",
      detail: "The team submitted its proposal for administrative review.",
      route: "/admin/proposals",
    },
    {
      id: "EV-031",
      action: "Design revision submitted",
      category: "Submissions",
      actor: "Team Atlas",
      target: "Campus Atlas",
      date: "12 December",
      detail: "Version 2 of the design brief was recorded for a follow-up review.",
      route: "/admin/reviews/atlas-design",
    },
  ],
};

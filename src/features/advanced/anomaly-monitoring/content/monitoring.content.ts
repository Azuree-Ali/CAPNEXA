export interface MonitoringContent {
  title: string;
  description: string;
  evidence: string;
  all: string;
  filter: string;
  total: string;
  open: string;
  empty: string;
  marked: string;
  dismiss: string;
  reopen: string;
  source: string;
  signals: readonly {
    id: string;
    category: string;
    severity: string;
    title: string;
    state: string;
    description: string;
    route: string;
  }[];
}
export const monitoringContent: MonitoringContent = {
  title: "Anomaly monitoring",
  description: "Inspect example signals and the supporting record for each signal.",
  evidence: "Evidence",
  all: "All categories",
  filter: "Signal category",
  total: "Signals in this view",
  open: "Awaiting review",
  empty: "No signals match this category.",
  marked: "Reviewed",
  dismiss: "Mark reviewed",
  reopen: "Reopen review",
  source: "Open supporting record",
  signals: [
    {
      id: "late-design",
      category: "Academic delivery",
      severity: "Medium",
      title: "Late design revision",
      state: "Review required",
      description:
        "The revision was submitted after the example team's internal target. The formal milestone deadline is still 15 December.",
      route: "/admin/reviews/atlas-design",
    },
    {
      id: "role-review",
      category: "Access review",
      severity: "Low",
      title: "Proposal role assignment check",
      state: "Review required",
      description:
        "Review the example capability matrix before assigning administrative proposal review.",
      route: "/admin/settings/permissions",
    },
  ],
};

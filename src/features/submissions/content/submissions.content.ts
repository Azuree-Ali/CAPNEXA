export interface SubmissionMonitoringContent {
  title: string;
  description: string;
  search: string;
  filter: string;
  all: string;
  metrics: { total: string; review: string; reviewed: string };
  reviewState: string;
  labels: { project: string; version: string; analysis: string; received: string };
  history: string;
  records: readonly SubmissionRecord[];
}
interface SubmissionRecord {
  id: string;
  title: string;
  project: string;
  version: string;
  state: string;
  analysis: string;
  received: string;
  route?: string;
}
export const submissionsContent: SubmissionMonitoringContent = {
  title: "Submission monitoring",
  description:
    "Track submitted versions and supervisor review status in this read-only administrative view.",
  search: "Filter submissions",
  filter: "Review status",
  all: "All statuses",
  metrics: { total: "Submissions", review: "Awaiting review", reviewed: "Reviewed" },
  reviewState: "Awaiting review",
  labels: { project: "Project", version: "Version", analysis: "Analysis", received: "Received" },
  history: "Review history",
  records: [
    {
      id: "SUB-026",
      title: "Atlas design brief",
      project: "Campus Atlas",
      version: "v2",
      state: "Awaiting review",
      analysis: "Analysis service unavailable",
      received: "12 December",
      route: "/admin/reviews/atlas-design",
    },
    {
      id: "SUB-024",
      title: "Atlas route mapping outline",
      project: "Campus Atlas",
      version: "v1",
      state: "Reviewed",
      analysis: "No analysis recorded",
      received: "28 November",
    },
  ],
};

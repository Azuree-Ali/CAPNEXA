export interface AnalyticsContent {
  title: string;
  description: string;
  stageHeading: string;
  period: string;
  projectsLabel: string;
  milestonesLabel: string;
  overdueLabel: string;
  stages: readonly { label: string; count: number; route: string }[];
  reviews: readonly { title: string; duration: number; route: string }[];
  reviewHeading: string;
  days: string;
  delayHeading: string;
  riskLabel: string;
  completedLabel: string;
  average: string;
  empty: string;
  milestones: readonly {
    title: string;
    state: "complete" | "scheduled" | "overdue";
    route: string;
  }[];
}
export const analyticsContent: AnalyticsContent = {
  title: "Academic analytics",
  description:
    "Inspect project stages, milestone delays, and review turnaround for the example cohort.",
  stageHeading: "Project stages",
  period: "Demonstration cohort",
  projectsLabel: "Projects",
  milestonesLabel: "Milestones",
  overdueLabel: "Overdue milestones",
  stages: [
    { label: "Design", count: 1, route: "/admin/projects/campus-atlas" },
    { label: "Proposal review", count: 1, route: "/admin/proposals" },
  ],
  reviews: [
    { title: "Atlas design brief · v1", duration: 4, route: "/admin/reviews/atlas-design" },
  ],
  reviewHeading: "Review turnaround",
  days: "days",
  delayHeading: "Milestone delivery",
  riskLabel: "Scheduled",
  completedLabel: "Completed milestones",
  average: "Average review time",
  empty: "No records in this view.",
  milestones: [
    { title: "Proposal approval", state: "complete", route: "/admin/milestones/timeline" },
    { title: "Design specification", state: "scheduled", route: "/admin/milestones/timeline" },
    { title: "Prototype review", state: "scheduled", route: "/admin/milestones/timeline" },
  ],
};

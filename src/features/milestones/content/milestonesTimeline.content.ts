export interface MilestoneTimelineContent {
  title: string;
  description: string;
  parent: { label: string; route: string };
  progress: string;
  stages: readonly {
    title: string;
    due: string;
    deliverable: string;
    status: string;
    complete: boolean;
  }[];
}
export const milestonesTimelineContent: MilestoneTimelineContent = {
  title: "Project milestone timeline",
  description: "Campus Atlas · Academic delivery schedule",
  parent: { label: "Milestone management", route: "/admin/milestones" },
  progress: "Milestones completed",
  stages: [
    {
      title: "Proposal approval",
      due: "1 November",
      deliverable: "Approved project scope",
      status: "Complete",
      complete: true,
    },
    {
      title: "Design specification",
      due: "15 December",
      deliverable: "Design brief and accessible route criteria",
      status: "In progress",
      complete: false,
    },
    {
      title: "Prototype review",
      due: "20 January",
      deliverable: "Interactive route map prototype",
      status: "Scheduled",
      complete: false,
    },
  ],
};

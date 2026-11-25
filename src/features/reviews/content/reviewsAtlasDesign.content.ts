export interface ReviewHistoryContent {
  title: string;
  description: string;
  parent: { label: string; route: string };
  versions: {
    id: string;
    title: string;
    reviewer: string;
    feedback: string;
    date: string;
    status: string;
  }[];
}
export const reviewsAtlasDesignContent: ReviewHistoryContent = {
  title: "Review history and feedback",
  description: "Atlas design brief · Campus Atlas",
  parent: { label: "Submission monitoring", route: "/admin/submissions" },
  versions: [
    {
      id: "v2",
      title: "Revised design brief",
      reviewer: "Pending review",
      feedback: "Team Atlas added wheelchair route criteria and updated its map legend.",
      date: "12 December",
      status: "Awaiting review",
    },
    {
      id: "v1",
      title: "Initial design brief",
      reviewer: "Dr. Olivia Bennett",
      feedback:
        "Clarify wheelchair route criteria and document accessibility checks for each building.",
      date: "5 December",
      status: "Changes requested",
    },
  ],
};

export interface ProjectDetailContent {
  title: string;
  description: string;
  parent: { label: string; route: string };
  facts: { label: string; value: string }[];
  tabs: {
    id: string;
    label: string;
    title: string;
    items: string[];
    related?: { label: string; route: string };
  }[];
  view: string;
}
export const projectsCampusAtlasContent: ProjectDetailContent = {
  title: "Campus Atlas",
  description: "A student-built accessibility map of campus facilities.",
  parent: { label: "Project management", route: "/admin/projects" },
  facts: [
    { label: "Project code", value: "CAP-026" },
    { label: "Stage", value: "Design" },
    { label: "Supervisor", value: "Dr. Olivia Bennett" },
  ],
  tabs: [
    {
      id: "overview",
      label: "Overview",
      title: "Project objectives",
      items: [
        "Map wheelchair-accessible campus routes.",
        "Publish an accessible building directory.",
        "Validate the design with a student usability review.",
      ],
    },
    {
      id: "team",
      label: "Team",
      title: "Atlas team",
      items: ["Alex Morgan · Team lead", "Robin Ellis · Developer"],
      related: { label: "Open team details", route: "/admin/teams/atlas" },
    },
    {
      id: "milestones",
      label: "Milestones",
      title: "Upcoming deliverables",
      items: ["Design specification · 15 December", "Prototype review · 20 January"],
      related: { label: "Open project timeline", route: "/admin/milestones/timeline" },
    },
    {
      id: "submissions",
      label: "Submissions",
      title: "Latest deliverable",
      items: ["Atlas design brief · Version 2 · Awaiting review"],
      related: { label: "Inspect submissions", route: "/admin/submissions" },
    },
    {
      id: "evaluations",
      label: "Evaluations",
      title: "Assessment snapshot",
      items: ["Design rationale · 18 / 20", "Implementation plan · 16 / 20"],
      related: { label: "View evaluation results", route: "/admin/evaluations" },
    },
  ],
  view: "Project workspace views",
};

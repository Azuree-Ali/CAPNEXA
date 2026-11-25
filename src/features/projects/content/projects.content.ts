export interface ProjectDirectoryContent {
  title: string;
  description: string;
  search: string;
  stage: string;
  allStages: string;
  columns: readonly [string, string, string, string];
  records: readonly ProjectDirectoryRecord[];
}
interface ProjectDirectoryRecord {
  id: string;
  title: string;
  team: string;
  stage: string;
  progress: number;
  route?: string;
}
export const projectsContent: ProjectDirectoryContent = {
  title: "Project management",
  description: "Monitor capstone projects, their stages, and the teams responsible for delivery.",
  search: "Find a project",
  stage: "Project stage",
  allStages: "All stages",
  columns: ["Project", "Team", "Stage", "Progress"],
  records: [
    {
      id: "CAP-026",
      title: "Campus Atlas",
      team: "Atlas",
      stage: "Design",
      progress: 60,
      route: "/admin/projects/campus-atlas",
    },
    {
      id: "CAP-031",
      title: "Library Lens",
      team: "Lens",
      stage: "Proposal review",
      progress: 20,
      route: "/admin/proposals/review",
    },
  ],
};

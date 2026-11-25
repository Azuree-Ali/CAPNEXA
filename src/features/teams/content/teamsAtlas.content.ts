import type { WorkspaceContent } from "@/components/data-table/workspace.content";
export interface TeamsAtlasContent extends WorkspaceContent {
  title: string;
}
export const teamsAtlasContent: TeamsAtlasContent = {
  title: "Atlas team",
  description: "Team membership and student responsibilities.",
  columns: ["Member", "Role", "Project"],
  records: [
    {
      cells: ["Alex Morgan", "Team lead", "Campus Atlas"],
      route: "/admin/students/alex-morgan",
    },
    {
      cells: ["Robin Ellis", "Developer", "Campus Atlas"],
    },
  ],
  links: [],
};

export interface TeamContextContent {
  facts: readonly { label: string; value: string }[];
  parent: { label: string; route: string };
}
export const atlasTeamContext: TeamContextContent = {
  facts: [
    { label: "Project", value: "Campus Atlas" },
    { label: "Supervisor", value: "Dr. Olivia Bennett" },
    { label: "Team code", value: "ATLAS-026" },
  ],
  parent: { label: "Campus Atlas", route: "/admin/projects/campus-atlas?view=team" },
};

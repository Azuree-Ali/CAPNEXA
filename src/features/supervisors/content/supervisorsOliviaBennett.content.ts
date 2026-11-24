import type { WorkspaceContent } from "@/components/data-table/workspace.content";
export interface SupervisorsOliviaBennettContent extends WorkspaceContent {
  title: string;
}
export const supervisorsOliviaBennettContent: SupervisorsOliviaBennettContent = {
  title: "Supervisor workload",
  description: "Dr. Olivia Bennett · Computer Science",
  columns: ["Project", "Team", "Status"],
  records: [
    {
      cells: ["Campus Atlas", "Atlas", "In progress"],
      route: "/admin/projects/campus-atlas",
    },
  ],
  links: [],
};

export interface SupervisorWorkloadContent {
  parent: { label: string; route: string };
  assigned: string;
  available: string;
  maximum: string;
  capacity: number;
  specialty: string;
  interestsLabel: string;
  interests: readonly string[];
}
export const supervisorWorkload: SupervisorWorkloadContent = {
  parent: { label: "Supervisor directory", route: "/admin/supervisors" },
  assigned: "Assigned projects",
  available: "Available slots",
  maximum: "Maximum projects",
  capacity: 3,
  specialty: "Distributed systems",
  interestsLabel: "Research interests",
  interests: ["Distributed data ingestion", "Accessible web platforms", "Information retrieval"],
};

import type { OperationListContent } from "@/features/dashboard/types/operations.types";
import type { WorkspaceContent } from "@/components/data-table/workspace.content";
export interface StudentsContent extends WorkspaceContent {
  live: OperationListContent;
}
export const studentsContent: StudentsContent = {
  live: {
    title: "Student directory",
    description: "Find student records, review academic profiles, and manage account access.",
    columns: ["Student", "Student number", "Department", "Academic year", "Account"],
  },
  title: "Student directory",
  description: "Find a student and review their academic record.",
  columns: ["Full name", "Student ID", "Department"],
  records: [],
  links: [],
};

export interface DirectoryMetricsContent {
  total: string;
  active: string;
  assigned: string;
}
export const studentDirectoryMetrics: DirectoryMetricsContent = {
  total: "Total students",
  active: "Active accounts",
  assigned: "Assigned to a team",
};

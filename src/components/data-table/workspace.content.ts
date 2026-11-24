export interface WorkspaceRecord {
  cells: [string, string, string];
  route?: string;
}
export interface WorkspaceContent {
  title: string;
  description: string;
  columns: [string, string, string];
  records: WorkspaceRecord[];
  links: { label: string; route: string }[];
}
export const workspaceCopy = {
  preview: "Demonstration workspace · Fictional records",
  search: "Filter records",
  empty: "No matching records.",
  clear: "Clear filter",
  back: "Administration overview",
  section: "Records",
  noAnswer: "No verified answer is available for this query.",
};

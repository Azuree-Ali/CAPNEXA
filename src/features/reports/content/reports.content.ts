export interface ReportRecord {
  id: string;
  name: string;
  department: string;
  academicYear: string;
  metric: string;
  source: string;
}
export interface ReportsContent {
  title: string;
  description: string;
  builder: string;
  scope: string;
  department: string;
  year: string;
  columns: readonly string[];
  all: string;
  count: string;
  templatesCount: string;
  results: string;
  clear: string;
  download: string;
  filename: string;
  notice: string;
  templates: readonly { id: string; name: string; records: readonly ReportRecord[] }[];
}
export const reportsContent: ReportsContent = {
  title: "Institutional reports",
  description: "Build a report from capstone project, supervisor, or assessment records.",
  builder: "Build a report",
  scope: "Report template",
  department: "Department",
  year: "Academic year",
  columns: ["Record", "Department", "Academic year", "Result"],
  all: "All",
  count: "Matching records",
  templatesCount: "Report templates",
  results: "Report preview",
  clear: "Reset filters",
  download: "Download CSV",
  filename: "capnexa-demo-reports.csv",
  notice: "The CSV contains the records shown in this preview.",
  templates: [
    {
      id: "projects",
      name: "Project progress",
      records: [
        {
          id: "CAP-026",
          name: "Campus Atlas",
          department: "Computer Science",
          academicYear: "2025–2026",
          metric: "Design · 60%",
          source: "/admin/projects/campus-atlas",
        },
        {
          id: "CAP-031",
          name: "Library Lens",
          department: "Information Systems",
          academicYear: "2025–2026",
          metric: "Proposal review · 20%",
          source: "/admin/proposals",
        },
      ],
    },
    {
      id: "supervisors",
      name: "Supervisor capacity",
      records: [
        {
          id: "FAC-026",
          name: "Dr. Olivia Bennett",
          department: "Computer Science",
          academicYear: "2025–2026",
          metric: "2 available slots",
          source: "/admin/supervisors/olivia-bennett",
        },
      ],
    },
    {
      id: "evaluations",
      name: "Evaluation outcomes",
      records: [
        {
          id: "EVAL-026",
          name: "Campus Atlas",
          department: "Computer Science",
          academicYear: "2025–2026",
          metric: "34 / 40",
          source: "/admin/evaluations",
        },
      ],
    },
  ],
};

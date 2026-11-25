import {
  Activity,
  ArrowUpRight,
  CalendarDays,
  ClipboardCheck,
  Download,
  Folder,
  GraduationCap,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";

export interface DashboardProposal {
  id: string;
  title: string;
  team: string;
  leader: string | null;
  department: string | null;
  submitted: string | null;
  route: string;
}

export interface DashboardSupervisor {
  id: string;
  name: string;
  area: string | null;
  assigned: number;
  capacity: number;
  available: number;
}

export interface DashboardContent {
  title: string;
  description: string;
  cohort: string;
  liveContext: string;
  preview: string;
  live: string;
  export: string;
  exportFilename: string;
  metricColumn: string;
  valueColumn: string;
  unavailable: string;
  missingField: string;
  metrics: readonly {
    id: "total" | "active" | "pending" | "overdue" | "supervisors";
    label: string;
    detail: string;
    route: string;
    icon: LucideIcon;
  }[];
  icons: Record<"export" | "external" | "calendar" | "activity", LucideIcon>;
  lifecycle: { title: string; description: string };
  health: {
    title: string;
    description: string;
    total: string;
    labels: readonly [string, string, string];
  };
  approvals: {
    title: string;
    description: string;
    empty: string;
    link: string;
    route: string;
    review: string;
    columns: readonly [string, string, string, string, string];
  };
  allocation: {
    title: string;
    description: string;
    empty: string;
    link: string;
    route: string;
    available: string;
    assigned: string;
  };
  deadlines: { title: string; description: string; route: string; link: string };
  liveActivity: string;
  quickLinksLabel: string;
  quickLinks: readonly { label: string; route: string }[];
  activity: { title: string; description: string; route: string; link: string };
  demo: {
    active: number;
    stages: readonly { label: string; count: number }[];
    health: readonly [number, number, number];
    proposals: readonly DashboardProposal[];
    supervisors: readonly DashboardSupervisor[];
    deadlines: readonly {
      label: string;
      project: string;
      date: string;
      status: string;
      overdue: boolean;
    }[];
    activity: readonly { label: string; date: string }[];
  };
}

export const dashboardContent: DashboardContent = {
  title: "Academic overview",
  description: "Monitor capstone progress, review proposals, and balance faculty workload.",
  cohort: "Academic year 2025–2026 · 12 Dec 2025",
  liveContext: "Administration workspace",
  preview: "Preview data",
  live: "Connected",
  export: "Export preview",
  exportFilename: "capnexa-preview-overview.csv",
  metricColumn: "Metric",
  valueColumn: "Count",
  unavailable: "University data is unavailable for this section.",
  missingField: "Unavailable",
  metrics: [
    {
      id: "total",
      label: "Total projects",
      detail: "Registered capstone projects",
      route: "/admin/projects",
      icon: Folder,
    },
    {
      id: "active",
      label: "Active projects",
      detail: "Projects in delivery",
      route: "/admin/projects",
      icon: Activity,
    },
    {
      id: "pending",
      label: "Pending proposals",
      detail: "Awaiting an administrative review",
      route: "/admin/proposals",
      icon: ClipboardCheck,
    },
    {
      id: "overdue",
      label: "Overdue milestones",
      detail: "Scheduled milestones past due",
      route: "/admin/milestones",
      icon: CalendarDays,
    },
    {
      id: "supervisors",
      label: "Active supervisors",
      detail: "Active faculty advisors",
      route: "/admin/supervisors",
      icon: GraduationCap,
    },
  ],
  icons: { export: Download, external: ArrowUpRight, calendar: CalendarDays, activity: Activity },
  lifecycle: {
    title: "Lifecycle stage distribution",
    description: "Registered projects by current stage",
  },
  health: {
    title: "Project health status",
    description: "Delivery status across the cohort",
    total: "Total projects",
    labels: ["On track", "Needs attention / review", "Overdue / delayed"],
  },
  approvals: {
    title: "Pending approvals",
    description: "Proposals awaiting review",
    empty: "No proposals are awaiting review.",
    link: "Open proposal queue",
    route: "/admin/proposals",
    review: "Review proposal",
    columns: ["Project", "Team & lead", "Department", "Submitted", "Action"],
  },
  allocation: {
    title: "Faculty supervisory allocation",
    description: "Assigned projects and remaining capacity",
    empty: "No active supervisors found.",
    link: "Open supervisor directory",
    route: "/admin/supervisors",
    available: "available",
    assigned: "Assigned projects",
  },
  deadlines: {
    title: "Upcoming deadlines",
    description: "Open milestones in deadline order",
    route: "/admin/milestones/timeline",
    link: "Open timeline",
  },
  liveActivity: "Academic activity across the university",
  quickLinksLabel: "Workspace shortcuts",
  quickLinks: [
    { label: "Student directory", route: "/admin/students" },
    { label: "Supervisor assignments", route: "/admin/assignments" },
    { label: "Advanced analytics", route: "/admin/analytics" },
    { label: "Academic assistant", route: "/admin/assistant" },
    { label: "Anomaly monitoring", route: "/admin/monitoring" },
    { label: "Notifications", route: "/admin/notifications" },
  ],
  activity: {
    title: "Recent activity",
    description: "Academic events in the preview workspace",
    route: "/admin/audit-logs",
    link: "Open audit log",
  },
  demo: {
    active: 1,
    stages: [
      { label: "Proposal review", count: 1 },
      { label: "Design", count: 1 },
      { label: "Implementation", count: 0 },
      { label: "Final defense", count: 0 },
    ],
    health: [1, 1, 0],
    proposals: [
      {
        id: "CAP-031",
        title: "Library Lens",
        team: "Lens",
        leader: "Priya Shah",
        department: "Computer Science",
        submitted: "2025-12-10",
        route: "/admin/proposals/review",
      },
    ],
    supervisors: [
      {
        id: "olivia-bennett",
        name: "Dr. Olivia Bennett",
        area: "Computer Science",
        assigned: 1,
        capacity: 3,
        available: 2,
      },
    ],
    deadlines: [
      {
        label: "Design specification",
        project: "Campus Atlas",
        date: "2025-12-15",
        status: "In progress",
        overdue: false,
      },
      {
        label: "Prototype review",
        project: "Campus Atlas",
        date: "2026-01-20",
        status: "Scheduled",
        overdue: false,
      },
    ],
    activity: [
      { label: "Team Lens submitted Library Lens for review.", date: "2025-12-10" },
      { label: "Dr. Olivia Bennett was assigned to Campus Atlas.", date: "2025-11-05" },
      { label: "The Campus Atlas proposal was approved.", date: "2025-11-01" },
    ],
  },
};

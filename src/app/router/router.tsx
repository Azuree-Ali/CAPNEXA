import { AccessStatePage } from "@/features/auth/pages/AccessStatePage";
import { SupervisorWorkloadPage } from "@/features/supervisors/pages/SupervisorWorkloadPage";
import { AuditLogPage } from "@/features/audit-logs/pages/AuditLogPage";
import { TeamDetailPage } from "@/features/teams/pages/TeamDetailPage";
import { NotificationsPage } from "@/features/notifications/pages/NotificationsPage";
import { AssistantPage } from "@/features/advanced/academic-assistant/pages/AssistantPage";
import { RecommendationsPage } from "@/features/advanced/ai-matching/pages/RecommendationsPage";
import { RecommendationComparisonPage } from "@/features/advanced/ai-matching/pages/RecommendationComparisonPage";
import { AnalyticsPage } from "@/features/advanced/analytics/pages/AnalyticsPage";
import { MonitoringPage } from "@/features/advanced/anomaly-monitoring/pages/MonitoringPage";
import { ProjectsPage } from "@/features/projects/pages/ProjectsPage";
import { ProjectDetailPage } from "@/features/projects/pages/ProjectDetailPage";
import { SubmissionsPage } from "@/features/submissions/pages/SubmissionsPage";
import { ReviewHistoryPage } from "@/features/reviews/pages/ReviewHistoryPage";
import { EvaluationsPage } from "@/features/evaluations/pages/EvaluationsPage";
import { MilestoneTimelinePage } from "@/features/milestones/pages/MilestoneTimelinePage";
import type { WorkspaceContent } from "@/components/data-table/workspace.content";
import { EditableRegister } from "@/components/forms/EditableRegister";
import { milestonesActions } from "@/features/milestones/content/milestones.content";
import { AcademicConfigurationPage } from "@/features/settings/pages/AcademicConfigurationPage";
import { RolesPermissionsPage } from "@/features/settings/pages/RolesPermissionsPage";
import { ReportsPage } from "@/features/reports/pages/ReportsPage";
import { DashboardPage } from "@/features/dashboard/pages/DashboardPage";
import { StudentPreviewPage } from "@/features/students/pages/StudentPreviewPage";
import { ProposalPreviewPage } from "@/features/proposals/pages/ProposalPreviewPage";
import { OperationsPage } from "@/features/dashboard/components/OperationsPage";
import { connected } from "@/lib/api/api-client";
import { workspaceRoutes } from "./workspace-routes";
import { WorkspaceView } from "@/components/data-table/WorkspaceView";
import { Navigate, Route, Routes } from "react-router-dom";
import { AuthLayout } from "../layouts/AuthLayout";
import { LoginPage } from "../../features/auth/pages/LoginPage";
import { ForgotPasswordPage } from "../../features/auth/pages/ForgotPasswordPage";
import { AdminLayout } from "../layouts/AdminLayout";
import { SupervisorLayout } from "../layouts/SupervisorLayout";
import { ProfilePage } from "../../features/settings/pages/ProfilePage";
function workspaceElement(
  path: string,
  content: WorkspaceContent | { title: string; description: string },
) {
  if (
    path === "students" ||
    path === "supervisors" ||
    path === "proposals" ||
    path === "assignments"
  ) {
    return <OperationsPage key={path} domain={path} />;
  }
  switch (path) {
    case "supervisors/olivia-bennett":
      if (connected) return <Navigate to="/admin/supervisors" replace />;
      return <SupervisorWorkloadPage />;
    case "teams/atlas":
      return <TeamDetailPage />;
    case "audit-logs":
      return <AuditLogPage />;
    case "notifications":
      return <NotificationsPage />;
    case "assistant":
      return <AssistantPage />;
    case "recommendations":
      return <RecommendationsPage />;
    case "recommendations/compare":
      return <RecommendationComparisonPage />;
    case "analytics":
      return <AnalyticsPage />;
    case "monitoring":
      return <MonitoringPage />;

    case "projects":
      return <ProjectsPage />;
    case "projects/campus-atlas":
      return <ProjectDetailPage />;
    case "submissions":
      return <SubmissionsPage />;
    case "reviews/atlas-design":
      return <ReviewHistoryPage />;
    case "evaluations":
      return <EvaluationsPage />;
    case "milestones/timeline":
      return <MilestoneTimelinePage />;

    case "dashboard":
      return <DashboardPage />;
    case "reports":
      return <ReportsPage />;
    case "settings":
      return <AcademicConfigurationPage />;
    case "settings/permissions":
      return <RolesPermissionsPage />;
    case "milestones":
      if (!("columns" in content)) break;
      return (
        <EditableRegister
          key={path}
          route={path}
          content={content}
          actions={milestonesActions}
          readOnly={connected}
        />
      );
    case "proposals/review":
      if (connected) return <Navigate to="/admin/proposals" replace />;
      if ("columns" in content)
        return <ProposalPreviewPage key={path} route={path} content={content} />;
      break;
    default:
      if ("columns" in content) return <WorkspaceView key={path} route={path} content={content} />;
  }
  throw new Error("Workspace route has no view: " + path);
}

export function AppRouter() {
  return (
    <Routes>
      <Route path="admin" element={<AdminLayout />}>
        <Route index element={<Navigate to="dashboard" replace />} />
        {workspaceRoutes.map(({ path, content }) => (
          <Route key={path} path={path} element={workspaceElement(path, content)} />
        ))}
        {!connected && <Route path="students/:id" element={<StudentPreviewPage />} />}
        <Route path="profile" element={<ProfilePage role="Admin" />} />
      </Route>
      <Route path="supervisor" element={<SupervisorLayout />}>
        <Route index element={<Navigate to="profile" replace />} />
        <Route path="profile" element={<ProfilePage role="Supervisor" />} />
      </Route>
      <Route element={<AuthLayout />}>
        <Route path="session-expired" element={<AccessStatePage state="expired" />} />
        <Route path="forbidden" element={<AccessStatePage state="forbidden" />} />
        <Route index element={<Navigate to="/login" replace />} />
        <Route path="login" element={<LoginPage />} />
        <Route path="forgot-password" element={<ForgotPasswordPage />} />
        <Route path="*" element={<AccessStatePage state="missing" />} />
      </Route>
    </Routes>
  );
}

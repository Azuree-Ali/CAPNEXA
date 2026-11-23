import { Outlet } from "react-router-dom";
import { DashboardShell } from "../../components/layout/DashboardShell";
export function AdminLayout() {
  return (
    <DashboardShell role="Admin">
      <Outlet />
    </DashboardShell>
  );
}

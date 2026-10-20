import { Outlet } from "react-router-dom";
import { DashboardShell } from "../../components/layout/DashboardShell";
export function SupervisorLayout() {
  return (
    <DashboardShell role="Supervisor">
      <Outlet />
    </DashboardShell>
  );
}

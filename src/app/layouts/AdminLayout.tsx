import { useSyncExternalStore } from "react";
import { Navigate } from "react-router-dom";
import { connected, currentUser, sessionExpired, subscribeSession } from "@/lib/api/api-client";
import { Outlet } from "react-router-dom";
import { DashboardShell } from "../../components/layout/DashboardShell";
export function AdminLayout() {
  const user = useSyncExternalStore(subscribeSession, currentUser);
  if (connected && !user)
    return <Navigate to={sessionExpired() ? "/session-expired" : "/login"} replace />;
  if (connected && !user?.roles.includes("Admin")) return <Navigate to="/forbidden" replace />;
  return (
    <DashboardShell role="Admin">
      <Outlet />
    </DashboardShell>
  );
}

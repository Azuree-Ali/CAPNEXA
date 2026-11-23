import { Navigate, Route, Routes, Link } from "react-router-dom";
import { AuthLayout } from "../layouts/AuthLayout";
import { LoginPage } from "../../features/auth/pages/LoginPage";
import { ForgotPasswordPage } from "../../features/auth/pages/ForgotPasswordPage";
import { AdminLayout } from "../layouts/AdminLayout";
import { SupervisorLayout } from "../layouts/SupervisorLayout";
import { ProfilePage } from "../../features/settings/pages/ProfilePage";
export function AppRouter() {
  return (
    <Routes>
      {/* TODO(BACKEND): These public UI previews are NOT protected routes.
        Add current-user loading and server-backed role guards before production.
        Login remains unchanged until its actual authentication contract is agreed. */}
      <Route path="admin" element={<AdminLayout />}>
        <Route index element={<Navigate to="profile" replace />} />
        <Route path="profile" element={<ProfilePage role="Admin" />} />
      </Route>
      <Route path="supervisor" element={<SupervisorLayout />}>
        <Route index element={<Navigate to="profile" replace />} />
        <Route path="profile" element={<ProfilePage role="Supervisor" />} />
      </Route>
      <Route element={<AuthLayout />}>
        <Route index element={<Navigate to="/login" replace />} />
        <Route path="login" element={<LoginPage />} />
        <Route path="forgot-password" element={<ForgotPasswordPage />} />
        <Route
          path="*"
          element={
            <section className="not-found">
              <h1>Page not found</h1>
              <Link to="/login">Back to sign-in</Link>
            </section>
          }
        />
      </Route>
    </Routes>
  );
}

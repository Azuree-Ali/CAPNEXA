import { Navigate, Route, Routes, Link } from "react-router-dom";
import { AuthLayout } from "../layouts/AuthLayout";
import { LoginPage } from "../../features/auth/pages/LoginPage";
import { ForgotPasswordPage } from "../../features/auth/pages/ForgotPasswordPage";
export function AppRouter() {
  return <Routes>
    <Route element={<AuthLayout />}>
      <Route index element={<Navigate to="/login" replace />} />
      <Route path="login" element={<LoginPage />} />
      <Route path="forgot-password" element={<ForgotPasswordPage />} />
      <Route path="*" element={<section className="not-found"><h1>Page not found</h1><Link to="/login">Back to sign-in</Link></section>} />
    </Route>
  </Routes>;
}
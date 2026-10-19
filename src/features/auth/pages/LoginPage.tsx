import { useOutletContext } from "react-router-dom";
import { Fragment } from "react";
import { StitchIcon } from "../components/StitchIcon";
import { AuthBrandPanel } from "../components/AuthBrandPanel";
import { LoginForm } from "../components/LoginForm";
export function LoginPage() {
  const { openInfo } = useOutletContext<{ openInfo: (label: string) => void }>();
  return <div className="login-container"><section className="login-card">
    <AuthBrandPanel /><div className="login-form-panel"><div className="login-form-content">
      <span className="access-pill"><StitchIcon name="admin_panel_settings" size={15} />Administrator &amp; Supervisor access</span>
      <h2>Welcome back</h2><p className="form-intro">Sign in with your university credentials to oversee capstone projects.</p>
      <LoginForm />
    </div><div className="legal-links">{["Privacy Policy", "Terms of Service", "Help & Support"].map((label, index) =>
      <Fragment key={label}>{index > 0 && <span aria-hidden="true">•</span>}<button type="button" onClick={() => openInfo(label)}>{label}</button></Fragment>)}</div></div>
  </section></div>;
}
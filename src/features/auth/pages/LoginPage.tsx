import { loginCopy as copy } from "@/features/auth/content/login.content";
import { connected } from "@/lib/api/api-client";
import { Link, useOutletContext } from "react-router-dom";
import { Fragment } from "react";
import { StitchIcon } from "../components/StitchIcon";
import { AuthBrandPanel } from "../components/AuthBrandPanel";
import { LoginForm } from "../components/LoginForm";
export function LoginPage() {
  const { openInfo } = useOutletContext<{ openInfo: (label: string) => void }>();
  return (
    <div className="login-container">
      <section className="login-card">
        <AuthBrandPanel />
        <div className="login-form-panel">
          <div className="login-form-content">
            <span className="access-pill">
              <StitchIcon name="admin_panel_settings" size={15} />
              {copy.administratorSupervisorAccess}
            </span>
            <h2>{copy.welcomeBack}</h2>
            <p className="form-intro">{copy.signInWithYourUniversityCredentialsToOversee}</p>
            <LoginForm />
            {!connected && (
              <Link className="preview-entry" to="/admin/dashboard">
                {copy.openDemonstrationWorkspace}
              </Link>
            )}
          </div>
          <div className="legal-links">
            {copy.legalLinks.map((label, index) => (
              <Fragment key={label}>
                {index > 0 && <span aria-hidden="true">{copy.separator}</span>}
                <button type="button" onClick={() => openInfo(label)}>
                  {label}
                </button>
              </Fragment>
            ))}
          </div>
        </div>
      </section>
    </div>
  );
}

import { Link } from "react-router-dom";
import { ArrowLeft, RotateCcwKey } from "lucide-react";
import { ForgotPasswordForm } from "../components/ForgotPasswordForm";
import { BrandLogo } from "../../../components/branding/BrandLogo";
export function ForgotPasswordPage() {
  return (
    <section className="recovery-container">
      <div className="recovery-card">
        <div className="recovery-heading">
          <div className="portal-brand">
            <div className="portal-brand-logo">
              <BrandLogo variant="compact" />
            </div>
            <i aria-hidden="true">|</i>
            <small>Faculty Portal</small>
          </div>
          <div className="recovery-glyph">
            <span>
              <RotateCcwKey size={22} />
            </span>
          </div>
          <h1>Reset your password</h1>
          <p>Enter the email address associated with your account.</p>
        </div>
        <ForgotPasswordForm />
        <Link to="/login" className="back-link">
          <ArrowLeft size={16} />
          Back to sign-in
        </Link>
      </div>
    </section>
  );
}

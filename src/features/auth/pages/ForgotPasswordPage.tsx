import { recoveryCopy as copy } from "@/features/auth/content/recovery.content";
import { Link } from "react-router-dom";
import { ArrowLeft, RotateCcwKey } from "lucide-react";
import { ForgotPasswordForm } from "../components/ForgotPasswordForm";
import { BrandLogo } from "../../../components/branding/BrandLogo";
import { connected } from "@/lib/api/api-client";

export function ForgotPasswordPage() {
  return (
    <section className="recovery-container">
      <div className="recovery-card">
        <div className="recovery-heading">
          <div className="portal-brand">
            <div className="portal-brand-logo">
              <BrandLogo variant="compact" />
            </div>
            <i aria-hidden="true">{copy.separator}</i>
            <small>{copy.facultyPortal}</small>
          </div>
          <div className="recovery-glyph">
            <span>
              <RotateCcwKey size={22} />
            </span>
          </div>
          <h1>{copy.resetYourPassword}</h1>
          <p>
            {connected ? copy.liveUnavailable : copy.enterTheEmailAddressAssociatedWithYourAccount}
          </p>
        </div>
        {!connected && <ForgotPasswordForm />}
        <Link to="/login" className="back-link">
          <ArrowLeft size={16} />
          {copy.backToSignIn}
        </Link>
      </div>
    </section>
  );
}

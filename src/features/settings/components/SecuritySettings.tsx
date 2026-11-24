import { profileCopy as copy } from "@/features/settings/content/profile.content";
export function SecuritySettings({
  onOptions,
}: {
  onOptions: (type: "password" | "verification") => void;
}) {
  // BACKEND: Replace preview actions with server-backed security policy and account
  // management. Do not implement real password changes, MFA or sessions in this preview.
  return (
    <section
      className="profile-card"
      id="security-settings"
      tabIndex={-1}
      aria-labelledby="security-title"
    >
      <div className="profile-card-heading profile-divider">
        <div>
          <h2 id="security-title">{copy.securitySignIn}</h2>
          <p>{copy.exploreAccountSecurityOptionsInThisDemo}</p>
        </div>
      </div>
      <div className="profile-security-options">
        <div className="profile-security-row">
          <div>
            <h3>{copy.passwordOptions}</h3>
            <p>{copy.noPasswordChangesAreMadeInThisPreview}</p>
          </div>
          <button type="button" className="profile-button" onClick={() => onOptions("password")}>
            {copy.viewOptions}
          </button>
        </div>
        <div className="profile-security-row">
          <div>
            <h3>{copy.twoStepVerification}</h3>
            <p>{copy.noVerificationMethodIsConfiguredByThisDemo}</p>
          </div>
          <button
            type="button"
            className="profile-button"
            onClick={() => onOptions("verification")}
          >
            {copy.viewOptions}
          </button>
        </div>
        <div className="profile-security-row">
          <div>
            <h3>{copy.previewSession}</h3>
            <p>{copy.thisPageDoesNotManageRealAccountSessions}</p>
          </div>
          <span className="profile-pill">{copy.previewOnly}</span>
        </div>
      </div>
    </section>
  );
}

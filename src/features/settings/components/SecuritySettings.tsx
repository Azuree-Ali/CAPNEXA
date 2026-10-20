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
          <h2 id="security-title">Security & sign-in</h2>
          <p>Explore account security options in this demo.</p>
        </div>
      </div>
      <div className="profile-security-options">
        <div className="profile-security-row">
          <div>
            <h3>Password options</h3>
            <p>No password changes are made in this preview.</p>
          </div>
          <button type="button" className="profile-button" onClick={() => onOptions("password")}>
            View options
          </button>
        </div>
        <div className="profile-security-row">
          <div>
            <h3>Two-step verification</h3>
            <p>No verification method is configured by this demo.</p>
          </div>
          <button
            type="button"
            className="profile-button"
            onClick={() => onOptions("verification")}
          >
            View options
          </button>
        </div>
        <div className="profile-security-row">
          <div>
            <h3>Preview session</h3>
            <p>This page does not manage real account sessions.</p>
          </div>
          <span className="profile-pill">Preview only</span>
        </div>
      </div>
    </section>
  );
}

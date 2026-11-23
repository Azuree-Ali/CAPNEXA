import type { ProfilePreferences, DigestSchedule, SubmissionAlerts } from "../types/profile.types";
export function PreferencesForm({
  value,
  disabled,
  dirty,
  onChange,
}: {
  value: ProfilePreferences;
  disabled: boolean;
  dirty: boolean;
  onChange: (value: ProfilePreferences) => void;
}) {
  // BACKEND: Load allowed schedule/timezone choices and notification policy from
  // the agreed contract. Preview choices do not schedule emails or live alerts.
  return (
    <section
      className="profile-card"
      id="preferences-settings"
      tabIndex={-1}
      aria-labelledby="preferences-title"
    >
      <div className="profile-card-heading profile-divider">
        <div>
          <h2 id="preferences-title">Notification & Workspace Preferences</h2>
          <p>Choose how you receive project updates.</p>
        </div>
      </div>
      <div className="profile-preferences-form">
        <label className="profile-field">
          <span>Email digest schedule</span>
          <select
            disabled={disabled}
            value={value.digest}
            onChange={(e) => onChange({ ...value, digest: e.target.value as DigestSchedule })}
          >
            <option value="daily">Daily 08:00 EST (Morning Briefing)</option>
            <option value="weekly">Weekly Monday 08:00 EST</option>
            <option value="off">Off</option>
          </select>
          <small>
            Aggregates submission uploads and student milestone progress into one email.
          </small>
        </label>
        <label className="profile-field">
          <span>Deliverable submission alerts</span>
          <select
            disabled={disabled}
            value={value.alerts}
            onChange={(e) => onChange({ ...value, alerts: e.target.value as SubmissionAlerts })}
          >
            <option value="immediate">Immediate notification</option>
            <option value="digest">Include in digest only</option>
            <option value="off">Off</option>
          </select>
        </label>
        <div className="profile-fields">
          <label className="profile-field">
            <span>Interface language</span>
            <input value={value.locale} readOnly />
          </label>
          <label className="profile-field">
            <span>Interface theme</span>
            <input value="Light Mode (Institutional)" readOnly />
          </label>
        </div>
      </div>
      <div className="profile-form-actions profile-divider-top">
        <button
          type="submit"
          className="profile-button profile-button-primary"
          disabled={disabled || !dirty}
        >
          {disabled ? "Updating..." : "Update preferences"}
        </button>
      </div>
    </section>
  );
}

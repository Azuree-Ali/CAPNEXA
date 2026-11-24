import { profileCopy as copy } from "@/features/settings/content/profile.content";
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
          <h2 id="preferences-title">{copy.notificationWorkspacePreferences}</h2>
          <p>{copy.chooseHowYouReceiveProjectUpdates}</p>
        </div>
      </div>
      <div className="profile-preferences-form">
        <label className="profile-field">
          <span>{copy.emailDigestSchedule}</span>
          <select
            disabled={disabled}
            value={value.digest}
            onChange={(e) => onChange({ ...value, digest: e.target.value as DigestSchedule })}
          >
            <option value="daily">{copy.daily0800ESTMorningBriefing}</option>
            <option value="weekly">{copy.weeklyMonday0800EST}</option>
            <option value="off">{copy.off}</option>
          </select>
          <small>{copy.aggregatesSubmissionUploadsAndStudentMilestoneProgressInto}</small>
        </label>
        <label className="profile-field">
          <span>{copy.deliverableSubmissionAlerts}</span>
          <select
            disabled={disabled}
            value={value.alerts}
            onChange={(e) => onChange({ ...value, alerts: e.target.value as SubmissionAlerts })}
          >
            <option value="immediate">{copy.immediateNotification}</option>
            <option value="digest">{copy.includeInDigestOnly}</option>
            <option value="off">{copy.off}</option>
          </select>
        </label>
        <div className="profile-fields">
          <label className="profile-field">
            <span>{copy.interfaceLanguage}</span>
            <input value={value.locale} readOnly />
          </label>
          <label className="profile-field">
            <span>{copy.interfaceTheme}</span>
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

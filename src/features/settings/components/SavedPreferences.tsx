import { digestLabels, alertLabels } from "../types/profile.types";
import type { ProfilePreferences } from "../types/profile.types";
export function SavedPreferences({ preferences }: { preferences: ProfilePreferences }) {
  const rows = [
    ["Locale", preferences.locale],
    ["Digest Schedule", digestLabels[preferences.digest]],
    ["Submission Alerts", alertLabels[preferences.alerts]],
    ["Interface Theme", preferences.theme],
  ];
  return (
    <section className="profile-card" aria-labelledby="saved-preferences-title">
      <div className="profile-card-heading">
        <div>
          <h2 id="saved-preferences-title">Saved Preferences</h2>
          <p>Configured system locale and notification rules.</p>
        </div>
      </div>
      <dl className="profile-preferences-list">
        {rows.map(([label, value]) => (
          <div key={label}>
            <dt>{label}</dt>
            <dd>
              {label === "Submission Alerts" ? (
                <span className="profile-pill profile-pill-blue">{value}</span>
              ) : (
                value
              )}
            </dd>
          </div>
        ))}
      </dl>
      <p className="profile-saved-note">Updates appear here after you save.</p>
    </section>
  );
}

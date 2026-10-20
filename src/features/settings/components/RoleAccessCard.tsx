import type { Profile } from "../types/profile.types";
import { ReadOnlyField } from "./InstitutionDetails";
export function RoleAccessCard({ profile }: { profile: Profile }) {
  return (
    <section
      className="profile-card"
      id="role-access"
      tabIndex={-1}
      aria-labelledby="role-access-title"
    >
      <div className="profile-card-heading">
        <div>
          <h2 id="role-access-title">Role & Access</h2>
          <p>Permissions granted to your administrator account.</p>
        </div>
      </div>
      <div className="profile-fields">
        <ReadOnlyField label="Role" value={profile.title} />
        <ReadOnlyField label="Access Scope" value={profile.accessScope ?? ""} />
      </div>
    </section>
  );
}

import { profileCopy as copy } from "@/features/settings/content/profile.content";
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
          <h2 id="role-access-title">{copy.roleAccess}</h2>
          <p>{copy.permissionsGrantedToYourAdministratorAccount}</p>
        </div>
      </div>
      <div className="profile-fields">
        <ReadOnlyField label="Role" value={profile.title} />
        <ReadOnlyField label="Access Scope" value={profile.accessScope ?? ""} />
      </div>
    </section>
  );
}

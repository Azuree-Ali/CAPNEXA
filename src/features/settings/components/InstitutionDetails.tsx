import { LockKeyhole } from "lucide-react";
import type { Profile } from "../types/profile.types";
export function ReadOnlyField({ label, value }: { label: string; value: string }) {
  return (
    <label className="profile-field">
      <span>{label}</span>
      <div className="profile-input-lock">
        <input readOnly value={value} />
        <LockKeyhole size={15} aria-hidden="true" />
      </div>
    </label>
  );
}
export function InstitutionDetails({ profile }: { profile: Profile }) {
  return (
    <section
      id="institution-details"
      className="profile-card"
      tabIndex={-1}
      aria-labelledby="institution-title"
    >
      <div className="profile-card-heading">
        <div>
          <h2 id="institution-title">Institution-managed Details</h2>
          <p>
            {profile.role === "Admin"
              ? "Core profile data synchronized with University Directory records."
              : "Core academic profile data synchronized directly with University Directory records."}
          </p>
        </div>
        <span className="profile-pill">
          <LockKeyhole size={13} />
          Read-only
        </span>
      </div>
      <div className="profile-fields">
        <ReadOnlyField label="First Name" value={profile.firstName} />
        <ReadOnlyField label="Last Name" value={profile.lastName} />
        <ReadOnlyField label="Institutional Email" value={profile.email} />
        <ReadOnlyField
          label={profile.role === "Admin" ? "Office" : "Academic Department"}
          value={profile.role === "Admin" ? "Academic Affairs" : profile.department}
        />
      </div>
    </section>
  );
}

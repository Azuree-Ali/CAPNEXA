import { LockKeyhole, Mail, GraduationCap } from "lucide-react";
import type { Profile } from "../types/profile.types";
export function ProfileHeader({ profile }: { profile: Profile }) {
  return (
    <section className="profile-header profile-card" aria-label="Profile summary">
      <span className="profile-large-avatar" aria-hidden="true">
        {profile.initials}
      </span>
      <div className="profile-header-identity">
        <div className="profile-name-row">
          <h2>{profile.name}</h2>
          <span className="profile-pill profile-pill-blue">{profile.title}</span>
        </div>
        <div className="profile-meta">
          <span>
            <Mail size={16} />
            {profile.email}
          </span>
          <span className="profile-meta-dot">·</span>
          <span>
            <GraduationCap size={17} />
            {profile.department}
          </span>
        </div>
      </div>
      <div className="profile-managed">
        <span className="profile-pill">
          <LockKeyhole size={14} />
          Managed by institution
        </span>
        <span className="profile-id">{profile.institutionId}</span>
      </div>
    </section>
  );
}

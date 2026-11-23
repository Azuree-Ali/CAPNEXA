import { MapPin, Clock3 } from "lucide-react";
import type { PortalRole } from "../../../config/navigation";
export function AvailabilityForm({
  role,
  location,
  hours,
  disabled,
  onChange,
}: {
  role: PortalRole;
  location: string;
  hours: string;
  disabled: boolean;
  onChange: (key: "officeLocation" | "officeHours", value: string) => void;
}) {
  return (
    <section
      className="profile-card"
      id="availability"
      tabIndex={-1}
      aria-labelledby="availability-title"
    >
      <div className="profile-card-heading">
        <div>
          <h2 id="availability-title">
            {role === "Admin" ? "Contact & Availability" : "Office & Availability"}
          </h2>
          <p>
            {role === "Admin"
              ? "Shown to supervisors when they request administrative help."
              : "Physical consultation locations and scheduled weekly capstone advisory hours."}
          </p>
        </div>
      </div>
      <div className="profile-fields">
        <label className="profile-field">
          <span>Office Location</span>
          <div className="profile-input-icon">
            <input
              maxLength={160}
              disabled={disabled}
              value={location}
              onChange={(e) => onChange("officeLocation", e.target.value)}
            />
            <MapPin size={16} aria-hidden="true" />
          </div>
        </label>
        <label className="profile-field">
          <span>{role === "Admin" ? "Support Hours" : "Preferred Office Hours"}</span>
          <div className="profile-input-icon">
            <input
              maxLength={160}
              disabled={disabled}
              value={hours}
              onChange={(e) => onChange("officeHours", e.target.value)}
            />
            <Clock3 size={16} aria-hidden="true" />
          </div>
        </label>
      </div>
    </section>
  );
}

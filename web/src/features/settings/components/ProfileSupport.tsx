import { Mail } from "lucide-react";
export function ProfileSupport({ onContact }: { onContact: () => void }) {
  // BACKEND: Supply the actual department support contact; no email address is assumed.
  return (
    <section className="profile-card" aria-labelledby="profile-support-title">
      <h2 id="profile-support-title">Profile Support</h2>
      <p>Need to update institution-managed details? Contact your department.</p>
      <button type="button" className="profile-button" onClick={onContact}>
        <Mail size={17} />
        Contact Department
      </button>
    </section>
  );
}

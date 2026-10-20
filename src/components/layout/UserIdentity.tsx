import type { PortalIdentity } from "../../config/navigation";
export function UserIdentity({ user }: { user: PortalIdentity }) {
  return (
    <div className="portal-user">
      <span className="portal-avatar" aria-hidden="true">
        {user.initials}
      </span>
      <span className="portal-user-text">
        <strong>{user.name}</strong>
        <small>{user.title}</small>
      </span>
    </div>
  );
}

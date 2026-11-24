import { NavLink } from "react-router-dom";
import type { PortalIdentity } from "../../config/navigation";

export function UserIdentity({
  user,
  to,
  label,
}: {
  user: PortalIdentity;
  to?: string;
  label?: string;
}) {
  const content = (
    <>
      <span className="portal-avatar" aria-hidden="true">
        {user.initials}
      </span>
      <span className="portal-user-text">
        <strong>{user.name}</strong>
        <small>{user.title}</small>
      </span>
    </>
  );
  if (!to) return <div className="portal-user">{content}</div>;
  return (
    <NavLink
      to={to}
      end
      aria-label={label}
      title={label}
      className={({ isActive }) => `portal-user portal-user--link${isActive ? " is-active" : ""}`}
    >
      {content}
    </NavLink>
  );
}

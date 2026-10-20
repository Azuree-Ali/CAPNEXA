import { NavLink, useNavigate } from "react-router-dom";
import { X, LogOut } from "lucide-react";
import { BrandLogo } from "../branding/BrandLogo";
import { navigation } from "../../config/navigation";
import type { PortalIdentity, PortalRole } from "../../config/navigation";
import { UserIdentity } from "./UserIdentity";
export function Sidebar({
  role,
  user,
  onClose,
}: {
  role: PortalRole;
  user: PortalIdentity;
  onClose?: () => void;
}) {
  const navigate = useNavigate();
  function logout() {
    // BACKEND: Invalidate the authenticated server session and clear the user/query
    // cache through the real auth service. This preview has no auth token or session.
    // Remove only CAPNEXA preview data, never clear unrelated browser storage.
    try {
      sessionStorage.removeItem("capnexa:profile-preview:v1:Admin");
      sessionStorage.removeItem("capnexa:profile-preview:v1:Supervisor");
    } catch {
      /* Navigation still works when browser storage is blocked. */
    }
    onClose?.();
    navigate("/login", { replace: true });
  }
  const base = role === "Admin" ? "/admin" : "/supervisor";
  return (
    <>
      <div className="portal-brand-row">
        <div>
          <BrandLogo variant="compact" />
          <small>AY 2025–2026</small>
        </div>
        {onClose ? (
          <button
            className="portal-icon portal-close"
            onClick={onClose}
            aria-label="Close navigation"
          >
            <X size={20} />
          </button>
        ) : null}
      </div>
      <nav className="portal-nav" aria-label={`${role} navigation`}>
        {navigation[role].map((group, index) => (
          <div className="portal-nav-group" key={group.label ?? index}>
            {group.label ? <p className="portal-group-label">{group.label}</p> : null}
            {group.items.map(({ label, path, icon: Icon, badge, available }) =>
              available ? (
                <NavLink
                  key={path}
                  to={`${base}/${path}`}
                  onClick={onClose}
                  className={({ isActive }) => `portal-nav-item${isActive ? " is-active" : ""}`}
                >
                  <Icon size={20} strokeWidth={1.75} />
                  <span>{label}</span>
                </NavLink>
              ) : (
                <span
                  key={path}
                  className="portal-nav-item portal-unavailable"
                  aria-disabled="true"
                  title="Planned screen — not implemented yet"
                >
                  <Icon size={20} strokeWidth={1.75} />
                  <span>{label}</span>
                  {badge ? <span className="portal-count">{badge}</span> : null}
                </span>
              ),
            )}
          </div>
        ))}
      </nav>
      <div className="portal-sidebar-user">
        <UserIdentity user={user} />
        <button
          type="button"
          className="portal-icon portal-logout"
          onClick={logout}
          aria-label="Log out"
          title="Log out"
        >
          <LogOut size={20} />
        </button>
      </div>
    </>
  );
}

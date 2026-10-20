import { useEffect, useRef, useState } from "react";
import type { ReactNode } from "react";
import { X } from "lucide-react";
import { Sidebar } from "./Sidebar";
import { Topbar } from "./Topbar";
import { previewIdentity } from "../../config/navigation";
import type { PortalRole } from "../../config/navigation";
import "../../styles/portal.css";
export function DashboardShell({ role, children }: { role: PortalRole; children: ReactNode }) {
  const drawer = useRef<HTMLDialogElement>(null);
  const help = useRef<HTMLDialogElement>(null);
  const menuButton = useRef<HTMLElement | null>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);
  // TODO(BACKEND): Use the authenticated current-user identity instead of this fixture.
  const user = previewIdentity[role];
  function closeDrawer() {
    drawer.current?.close();
  }
  function openDrawer() {
    menuButton.current = document.activeElement as HTMLElement;
    drawer.current?.showModal();
    setDrawerOpen(true);
  }
  useEffect(() => {
    const media = window.matchMedia("(min-width: 1024px)");
    const close = () => {
      if (media.matches) drawer.current?.close();
    };
    media.addEventListener("change", close);
    return () => media.removeEventListener("change", close);
  }, []);
  useEffect(() => {
    if (!drawerOpen) return;
    const previous = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    return () => {
      document.body.style.overflow = previous;
    };
  }, [drawerOpen]);
  return (
    <div className="portal-shell">
      <a className="portal-skip" href="#portal-main">
        Skip to content
      </a>
      <aside className="portal-sidebar">
        <Sidebar role={role} user={user} />
      </aside>
      <dialog
        ref={drawer}
        className="portal-drawer"
        aria-label={`${role} navigation`}
        onClick={(e) => {
          if (e.target === e.currentTarget) closeDrawer();
        }}
        onClose={() => {
          setDrawerOpen(false);
          menuButton.current?.focus();
        }}
      >
        <div className="portal-drawer-inner">
          <Sidebar role={role} user={user} onClose={closeDrawer} />
        </div>
      </dialog>
      <div className="portal-column">
        <Topbar
          role={role}
          user={user}
          onMenu={openDrawer}
          onHelp={() => help.current?.showModal()}
        />
        <main id="portal-main" className="portal-main" tabIndex={-1}>
          {children}
        </main>
      </div>
      <dialog ref={help} className="portal-help" aria-labelledby="portal-help-title">
        <button
          className="portal-icon"
          autoFocus
          onClick={() => help.current?.close()}
          aria-label="Close help"
        >
          <X size={20} />
        </button>
        <h2 id="portal-help-title">CAPNEXA — Profile help</h2>
        <p>
          Manage personal information, availability, research interests and preferences. Search
          helps you find a profile section. Notifications are currently unavailable.
        </p>
        <p>
          Profile changes are saved only in this browser preview. Account security options are not
          connected yet.
        </p>
      </dialog>
    </div>
  );
}

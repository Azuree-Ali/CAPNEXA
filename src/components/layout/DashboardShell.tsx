import { connected, currentUser } from "@/lib/api/api-client";
import { SiteFooter } from "./SiteFooter";
import { branding } from "../branding/branding.content";
import { useEffect, useRef, useState } from "react";
import type { ReactNode } from "react";
import { X } from "lucide-react";
import { Sidebar } from "./Sidebar";
import { Topbar } from "./Topbar";
import { previewIdentity } from "../../config/navigation";
import type { PortalRole } from "../../config/navigation";
import "../../styles/portal.css";
import { portalCopy as copy } from "./portal.content";
export function DashboardShell({ role, children }: { role: PortalRole; children: ReactNode }) {
  const drawer = useRef<HTMLDialogElement>(null);
  const privacy = useRef<HTMLDialogElement>(null);
  const help = useRef<HTMLDialogElement>(null);
  const menuButton = useRef<HTMLElement | null>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const identity = currentUser();
  const user = identity
    ? {
        name: [identity.firstName, identity.lastName].join(" "),
        initials: (identity.firstName[0] ?? "") + (identity.lastName[0] ?? ""),
        title: role,
      }
    : previewIdentity[role];
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
    <div className="portal-shell" data-portal-role={role}>
      <a className="portal-skip" href="#portal-main">
        {copy.skip}
      </a>
      <aside className="portal-sidebar">
        <Sidebar role={role} user={user} />
      </aside>
      <dialog
        ref={drawer}
        className="portal-drawer"
        aria-label={`${role} ${copy.navigation}`}
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
        <SiteFooter
          onPrivacy={() => privacy.current?.showModal()}
          onHelp={() => help.current?.showModal()}
        />
      </div>
      <dialog
        ref={privacy}
        className="info-dialog"
        aria-labelledby="privacy-heading"
        data-backdrop-close
      >
        <h2 id="privacy-heading">{branding.privacy}</h2>
        <p>{branding.privacyNotice}</p>
        <button type="button" className="primary-button" onClick={() => privacy.current?.close()}>
          {branding.close}
        </button>
      </dialog>
      <dialog
        ref={help}
        className="portal-help"
        aria-labelledby="portal-help-title"
        data-backdrop-close
      >
        <button
          className="portal-icon"
          autoFocus
          onClick={() => help.current?.close()}
          aria-label={copy.closeHelp}
        >
          <X size={20} />
        </button>
        <h2 id="portal-help-title">{copy.helpTitle}</h2>
        <p>{connected ? copy.liveHelp : copy.previewHelp}</p>
        {role === "Admin" && <p>{copy.notificationHelp}</p>}
      </dialog>
    </div>
  );
}

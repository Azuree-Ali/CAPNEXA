import { connected } from "@/lib/api/api-client";
import { portalCopy as copy } from "./portal.content";
import { WorkspaceSearch } from "./WorkspaceSearch";
import { workspaceRoutes } from "@/app/router/workspace-routes";
import { useEffect, useId, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Bell, CircleHelp, Search, Menu } from "lucide-react";
import { BrandLogo } from "../branding/BrandLogo";
import { UserIdentity } from "./UserIdentity";
import type { PortalIdentity, PortalRole } from "../../config/navigation";
export function Topbar({
  role,
  user,
  onMenu,
  onHelp,
}: {
  role: PortalRole;
  user: PortalIdentity;
  onMenu: () => void;
  onHelp: () => void;
}) {
  const navigate = useNavigate();
  const [query, setQuery] = useState("");
  const [open, setOpen] = useState(false);
  const [selected, setSelected] = useState(-1);
  const box = useRef<HTMLDivElement>(null);
  const input = useRef<HTMLInputElement>(null);
  const listId = useId();
  const base = role === "Admin" ? "/admin/profile" : "/supervisor/profile";
  // BACKEND: Replace local profile-section search with the agreed permission-filtered
  // global-search API. Add cancellation, loading/errors and server pagination. No
  // project, student or submission records are invented by this preview.
  const search = query.trim().toLowerCase();
  const destinations =
    role === "Admin"
      ? workspaceRoutes.map(({ path, content }) => ({
          label: content.title,
          tab: "",
          id: "/admin/" + path,
          terms: content.description,
        }))
      : [];
  const profileSections = connected
    ? [{ label: copy.accountProfile, tab: "", id: base, terms: copy.accountTerms }]
    : copy.profileSections;
  const results = search
    ? [...profileSections, ...destinations].filter(
        (s) =>
          !(role === "Admin" && s.id === "research-interests") &&
          !(role === "Supervisor" && s.id === "role-access") &&
          `${s.label} ${s.terms}`.toLowerCase().includes(search),
      )
    : [];
  function choose(index: number) {
    const item = results[index];
    if (!item) return;
    navigate(item.tab ? `${base}?tab=${item.tab}#${item.id}` : item.id);
    setOpen(false);
    setQuery("");
    setSelected(-1);
  }
  useEffect(() => {
    function outside(event: PointerEvent) {
      if (!box.current?.contains(event.target as Node)) setOpen(false);
    }
    document.addEventListener("pointerdown", outside);
    return () => document.removeEventListener("pointerdown", outside);
  }, []);
  return (
    <header className="portal-topbar">
      <span className="topbar-wordmark">
        <BrandLogo variant="footer" showSymbol={false} />
      </span>
      <button
        type="button"
        className="portal-icon portal-menu"
        onClick={onMenu}
        aria-label={copy.menu}
        aria-haspopup="dialog"
      >
        <Menu size={22} />
      </button>
      <div
        className="portal-search-container"
        ref={box}
        onBlur={(event) => {
          if (!event.currentTarget.contains(event.relatedTarget as Node | null)) setOpen(false);
        }}
      >
        <form
          role="search"
          className="portal-search"
          onSubmit={(event) => {
            event.preventDefault();
            if (results.length) choose(selected >= 0 ? selected : 0);
            else setOpen(true);
          }}
        >
          <Search size={18} aria-hidden="true" />
          <input
            ref={input}
            role="combobox"
            aria-label={copy.searchLabel}
            aria-autocomplete="list"
            aria-expanded={open && !!search}
            aria-controls={listId}
            aria-activedescendant={
              open && selected >= 0 && results[selected] ? `${listId}-${selected}` : undefined
            }
            autoComplete="off"
            value={query}
            placeholder={copy.searchPlaceholder}
            onFocus={() => setOpen(true)}
            onChange={(event) => {
              setQuery(event.target.value);
              setSelected(-1);
              setOpen(true);
            }}
            onKeyDown={(event) => {
              if (event.key === "Escape") {
                setOpen(false);
                setSelected(-1);
              }
              if ((event.key === "ArrowDown" || event.key === "ArrowUp") && results.length) {
                event.preventDefault();
                setOpen(true);
                setSelected((old) =>
                  event.key === "ArrowDown"
                    ? (old + 1) % results.length
                    : old <= 0
                      ? results.length - 1
                      : old - 1,
                );
              }
            }}
          />
        </form>
        {open && search ? (
          <div className="portal-search-results">
            <p>{copy.searchHeading}</p>
            <ul role="listbox" id={listId} aria-label={copy.searchResults}>
              {results.map((item, index) => (
                <li
                  role="option"
                  aria-selected={selected === index}
                  id={`${listId}-${index}`}
                  key={item.id}
                  onPointerDown={(event) => event.preventDefault()}
                  onClick={() => choose(index)}
                  onPointerMove={() => setSelected(index)}
                >
                  {item.label}
                </li>
              ))}
            </ul>
            {!results.length ? <p role="status">{copy.noDestinations}</p> : null}
            <small>{copy.searchHint}</small>
          </div>
        ) : (
          <ul role="listbox" id={listId} hidden aria-label={copy.searchResults} />
        )}
      </div>
      <div className="portal-topbar-actions">
        {role === "Admin" && <WorkspaceSearch />}
        {/* BACKEND: Supply notification items and unread count before enabling the bell. */}
        <button
          type="button"
          className="portal-icon portal-bell"
          disabled={role !== "Admin"}
          onClick={() => navigate("/admin/notifications")}
          aria-label={copy.notifications}
          title={copy.notifications}
        >
          <Bell size={20} />
        </button>
        <button
          type="button"
          className="portal-icon portal-help-btn"
          onClick={onHelp}
          aria-label={copy.helpLabel}
        >
          <CircleHelp size={18} />
        </button>
        <UserIdentity user={user} to={base} label={`${user.name}, ${copy.accountProfile}`} />
      </div>
    </header>
  );
}

import { useEffect, useId, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Bell, CircleHelp, SlidersHorizontal, Search, Menu } from "lucide-react";
import { UserIdentity } from "./UserIdentity";
import type { PortalIdentity, PortalRole } from "../../config/navigation";
const sections = [
  {
    label: "Personal Information",
    tab: "personal",
    id: "institution-details",
    terms: "name email department office profile",
  },
  {
    label: "Office & Availability",
    tab: "personal",
    id: "availability",
    terms: "contact location hours schedule",
  },
  {
    label: "Research Interests",
    tab: "personal",
    id: "research-interests",
    terms: "topics artificial intelligence distributed systems",
  },
  {
    label: "Role & Access",
    tab: "personal",
    id: "role-access",
    terms: "permissions administrator scope",
  },
  {
    label: "Security & sign-in",
    tab: "security",
    id: "security-settings",
    terms: "password verification session security",
  },
  {
    label: "Notification & Workspace Preferences",
    tab: "preferences",
    id: "preferences-settings",
    terms: "digest alerts notifications language theme",
  },
];
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
  const results = search
    ? sections.filter(
        (s) =>
          !(role === "Admin" && s.id === "research-interests") &&
          !(role === "Supervisor" && s.id === "role-access") &&
          `${s.label} ${s.terms}`.toLowerCase().includes(search),
      )
    : [];
  function choose(index: number) {
    const item = results[index];
    if (!item) return;
    navigate(`${base}?tab=${item.tab}#${item.id}`);
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
      <button
        type="button"
        className="portal-icon portal-menu"
        onClick={onMenu}
        aria-label="Open navigation"
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
            aria-label="Search workspace"
            aria-autocomplete="list"
            aria-expanded={open && !!search}
            aria-controls={listId}
            aria-activedescendant={
              open && selected >= 0 && results[selected] ? `${listId}-${selected}` : undefined
            }
            autoComplete="off"
            value={query}
            placeholder={
              role === "Admin"
                ? "Search users, projects, cohorts..."
                : "Search projects, submissions, reviews..."
            }
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
            <p>Profile sections</p>
            <ul role="listbox" id={listId} aria-label="Search results">
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
            {!results.length ? <p role="status">No matching profile sections.</p> : null}
            <small>Search currently covers your profile sections.</small>
          </div>
        ) : (
          <ul role="listbox" id={listId} hidden aria-label="Search results" />
        )}
      </div>
      <div className="portal-topbar-actions">
        {/* BACKEND: Supply notification items and unread count before enabling the bell. */}
        <button
          type="button"
          className="portal-icon portal-bell"
          disabled
          aria-label="Notifications — no items yet"
          title="No notifications yet"
        >
          <Bell size={20} />
        </button>
        <button
          type="button"
          className="portal-icon"
          onClick={onHelp}
          aria-label="Help & Documentation"
        >
          <CircleHelp size={18} />
        </button>
        <button
          type="button"
          className="portal-icon"
          onClick={() => navigate(`${base}?tab=preferences`)}
          aria-label="Open preferences"
        >
          <SlidersHorizontal size={18} />
        </button>
        <UserIdentity user={user} />
      </div>
    </header>
  );
}

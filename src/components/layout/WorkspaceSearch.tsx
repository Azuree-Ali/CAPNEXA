import { useEffect, useRef, useState } from "react";
import { Link } from "react-router-dom";
import { CornerDownLeft, Search, X } from "lucide-react";
import { workspaceRoutes } from "@/app/router/workspace-routes";
import { palette } from "./workspaceSearch.content";
export function WorkspaceSearch() {
  const dialog = useRef<HTMLDialogElement>(null);
  const [query, setQuery] = useState("");
  function open() {
    setQuery("");
    if (!dialog.current?.open) dialog.current?.showModal();
  }
  useEffect(() => {
    function key(e: KeyboardEvent) {
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "k") {
        e.preventDefault();
        open();
      }
    }
    document.addEventListener("keydown", key);
    return () => document.removeEventListener("keydown", key);
  }, []);
  const results = workspaceRoutes.filter(({ content }) =>
    (content.title + " " + content.description).toLowerCase().includes(query.trim().toLowerCase()),
  );
  return (
    <>
      <button
        className="portal-icon portal-palette-btn"
        type="button"
        onClick={open}
        aria-label={palette.title}
        title={palette.shortcut}
      >
        <Search size={18} />
      </button>
      <dialog
        ref={dialog}
        className="info-dialog academic-palette"
        aria-labelledby="palette-title"
        data-backdrop-close
      >
        <header className="palette-header">
          <h2 id="palette-title">{palette.title}</h2>
          <button
            type="button"
            className="palette-close"
            aria-label={palette.close}
            title={palette.close}
            onClick={() => dialog.current?.close()}
          >
            <X size={18} aria-hidden="true" />
          </button>
        </header>
        <label className="palette-field">
          <Search size={18} aria-hidden="true" />
          <input
            autoFocus
            aria-label={palette.search}
            placeholder={palette.search}
            type="search"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
          />
          <kbd aria-hidden="true">Esc</kbd>
        </label>
        {results.length ? (
          <ul className="palette-results">
            {results.map(({ path, content }) => (
              <li key={path}>
                <Link onClick={() => dialog.current?.close()} to={"/admin/" + path}>
                  {content.title}
                  <CornerDownLeft size={14} aria-hidden="true" />
                </Link>
              </li>
            ))}
          </ul>
        ) : (
          <p role="status" className="palette-empty">
            {palette.empty}
          </p>
        )}
      </dialog>
    </>
  );
}

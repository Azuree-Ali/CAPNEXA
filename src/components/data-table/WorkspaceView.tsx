import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { ArrowUpRight, Search } from "lucide-react";
import { workspaceCopy as copy, type WorkspaceContent } from "./workspace.content";

interface Props {
  content: WorkspaceContent;
  route: string;
  embedded?: boolean;
}
export function WorkspaceView({ content, route, embedded }: Props) {
  const [query, setQuery] = useState("");
  useEffect(() => {
    document.title = content.title + " | CAPNEXA";
  }, [content.title]);
  const records = content.records.filter((row) =>
    row.cells.join(" ").toLowerCase().includes(query.trim().toLowerCase()),
  );
  return (
    <section className="academic-workspace">
      {!embedded && (
        <header>
          {route !== "dashboard" && <Link to="/admin/dashboard">{copy.back}</Link>}
          <h1>{content.title}</h1>
          <p>{content.description}</p>
          <small>{copy.preview}</small>
        </header>
      )}
      <div className="academic-records">
        <div className="academic-tools">
          <h2>{copy.section}</h2>
          <label>
            <Search size={17} aria-hidden="true" />
            <input
              type="search"
              aria-label={copy.search}
              placeholder={copy.search}
              value={query}
              onChange={(e) => setQuery(e.target.value)}
            />
          </label>
        </div>
        {records.length ? (
          <div className="academic-scroll">
            <table>
              <thead>
                <tr>
                  {content.columns.map((label) => (
                    <th scope="col" key={label}>
                      {label}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {records.map((row) => (
                  <tr key={row.cells[0]}>
                    <th scope="row">
                      {row.route ? (
                        <Link to={row.route}>
                          {row.cells[0]} <ArrowUpRight size={14} />
                        </Link>
                      ) : (
                        row.cells[0]
                      )}
                    </th>
                    <td>{row.cells[1]}</td>
                    <td>{row.cells[2]}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div role="status" className="academic-empty">
            <p>{route === "assistant" ? copy.noAnswer : copy.empty}</p>
            <button type="button" onClick={() => setQuery("")}>
              {copy.clear}
            </button>
          </div>
        )}
      </div>
      {!!content.links.length && (
        <nav className="academic-links">
          {content.links.map((link) => (
            <Link key={link.route} to={link.route}>
              {link.label}
              <ArrowUpRight size={16} />
            </Link>
          ))}
        </nav>
      )}
    </section>
  );
}

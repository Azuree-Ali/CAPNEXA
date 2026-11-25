import { useState } from "react";
import { Link } from "react-router-dom";
import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { WorkspaceMetrics } from "@/components/data-table/WorkspaceMetrics";
import { workspaceCopy } from "@/components/data-table/workspace.content";
import { auditLogsContent as copy } from "../content/auditLogs.content";

export function AuditLogPage() {
  const [search, setSearch] = useState("");
  const [category, setCategory] = useState("");
  const categories = [...new Set(copy.events.map((event) => event.category))];
  const events = copy.events.filter(
    (event) =>
      (!category || event.category === category) &&
      [event.action, event.actor, event.target, event.id]
        .join(" ")
        .toLowerCase()
        .includes(search.trim().toLowerCase()),
  );
  return (
    <section className="academic-workspace">
      <WorkspaceHeading title={copy.title} description={copy.description} />
      <div className="academic-records">
        <div className="academic-tools">
          <label>
            {copy.search}
            <input
              type="search"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
            />
          </label>
          <label>
            {copy.category}
            <select value={category} onChange={(event) => setCategory(event.target.value)}>
              <option value="">{copy.all}</option>
              {categories.map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>
        </div>
        <div className="academic-disclosures">
          {events.map((event) => (
            <details key={event.id}>
              <summary>
                <span>
                  <strong>{event.action}</strong>
                  <small className="academic-secondary">
                    {event.id} · {event.date}
                  </small>
                </span>
                <span className="academic-status">{event.category}</span>
              </summary>
              <div className="academic-panel">
                <WorkspaceMetrics
                  items={[
                    { label: copy.fields[0], value: event.actor },
                    { label: copy.fields[1], value: event.target },
                    { label: copy.fields[2], value: event.date },
                  ]}
                />
                <p>{event.detail}</p>
                <Link to={event.route}>{copy.fields[3]}</Link>
              </div>
            </details>
          ))}
        </div>
        {!events.length && (
          <div className="academic-empty">
            <p role="status">{workspaceCopy.empty}</p>
            <button
              onClick={() => {
                setSearch("");
                setCategory("");
              }}
            >
              {workspaceCopy.clear}
            </button>
          </div>
        )}
      </div>
    </section>
  );
}

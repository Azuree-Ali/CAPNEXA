import { useState } from "react";
import { Link } from "react-router-dom";
import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { WorkspaceMetrics } from "@/components/data-table/WorkspaceMetrics";
import { workspaceCopy } from "@/components/data-table/workspace.content";
import { submissionsContent as copy } from "../content/submissions.content";

export function SubmissionsPage() {
  const [query, setQuery] = useState("");
  const [status, setStatus] = useState("");
  const statuses = [...new Set(copy.records.map((record) => record.state))];
  const awaiting = copy.records.filter((record) => record.state === copy.reviewState).length;
  const records = copy.records.filter(
    (record) =>
      (!status || status === record.state) &&
      [record.title, record.project, record.id]
        .join(" ")
        .toLowerCase()
        .includes(query.trim().toLowerCase()),
  );
  return (
    <section className="academic-workspace">
      <WorkspaceHeading title={copy.title} description={copy.description} />
      <WorkspaceMetrics
        items={[
          { label: copy.metrics.total, value: copy.records.length },
          { label: copy.metrics.review, value: awaiting },
          { label: copy.metrics.reviewed, value: copy.records.length - awaiting },
        ]}
      />
      <div className="academic-records">
        <div className="academic-tools">
          <label>
            {copy.search}
            <input type="search" value={query} onChange={(event) => setQuery(event.target.value)} />
          </label>
          <label>
            {copy.filter}
            <select value={status} onChange={(event) => setStatus(event.target.value)}>
              <option value="">{copy.all}</option>
              {statuses.map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>
        </div>
        {records.length ? (
          <div className="academic-disclosures">
            {records.map((record) => (
              <details key={record.id}>
                <summary>
                  <span>
                    <strong>{record.title}</strong>
                    <small className="academic-secondary">{record.id}</small>
                  </span>
                  <span className="academic-status">{record.state}</span>
                </summary>
                <div className="academic-panel">
                  <WorkspaceMetrics
                    items={[
                      { label: copy.labels.project, value: record.project },
                      { label: copy.labels.version, value: record.version },
                      { label: copy.labels.received, value: record.received },
                      { label: copy.labels.analysis, value: record.analysis },
                    ]}
                  />
                  {record.route && <Link to={record.route}>{copy.history}</Link>}
                </div>
              </details>
            ))}
          </div>
        ) : (
          <div className="academic-empty">
            <p role="status">{workspaceCopy.empty}</p>
            <button
              type="button"
              onClick={() => {
                setQuery("");
                setStatus("");
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

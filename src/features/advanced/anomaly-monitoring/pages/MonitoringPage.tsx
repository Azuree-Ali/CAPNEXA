import { useState } from "react";
import { Link } from "react-router-dom";
import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { WorkspaceMetrics } from "@/components/data-table/WorkspaceMetrics";
import { monitoringContent as copy } from "../content/monitoring.content";

const reviewedSignals = new Set<string>();
const categories = [...new Set(copy.signals.map((signal) => signal.category))];

export function MonitoringPage() {
  const [category, setCategory] = useState("");
  const [reviewed, setReviewed] = useState(() => new Set(reviewedSignals));
  const signals = copy.signals.filter((signal) => !category || signal.category === category);
  function toggle(id: string) {
    if (reviewedSignals.has(id)) reviewedSignals.delete(id);
    else reviewedSignals.add(id);
    setReviewed(new Set(reviewedSignals));
  }
  return (
    <section className="academic-workspace">
      <WorkspaceHeading title={copy.title} description={copy.description} />
      <WorkspaceMetrics
        items={[
          { label: copy.total, value: signals.length },
          { label: copy.open, value: signals.filter((signal) => !reviewed.has(signal.id)).length },
        ]}
      />
      <div className="academic-records">
        <div className="academic-tools">
          <label>
            {copy.filter}
            <select value={category} onChange={(event) => setCategory(event.target.value)}>
              <option value="">{copy.all}</option>
              {categories.map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>
        </div>
        {signals.length ? (
          <div className="academic-disclosures">
            {signals.map((signal) => (
              <details key={signal.id}>
                <summary>
                  <span>
                    <strong>{signal.title}</strong>
                    <small className="academic-secondary">
                      {signal.category} · {signal.severity}
                    </small>
                  </span>
                  <span className="academic-status">
                    {reviewed.has(signal.id) ? copy.marked : signal.state}
                  </span>
                </summary>
                <section className="academic-panel">
                  <h2>{copy.evidence}</h2>
                  <p>{signal.description}</p>
                  <div className="academic-inline-actions">
                    <Link to={signal.route}>{copy.source}</Link>
                    <button type="button" onClick={() => toggle(signal.id)}>
                      {reviewed.has(signal.id) ? copy.reopen : copy.dismiss}
                    </button>
                  </div>
                </section>
              </details>
            ))}
          </div>
        ) : (
          <p className="academic-panel" role="status">
            {copy.empty}
          </p>
        )}
      </div>
    </section>
  );
}

import { useState } from "react";
import { Link } from "react-router-dom";
import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { recommendationsContent as copy } from "../content/recommendations.content";

export function RecommendationsPage() {
  const [selected, setSelected] = useState<string[]>([]);
  return (
    <section className="academic-workspace">
      <WorkspaceHeading title={copy.title} description={copy.description} />
      <div className="academic-card-grid">
        {copy.candidates.map((candidate) => (
          <article key={candidate.id} className="academic-records academic-panel">
            <div className="academic-card-heading">
              <h2>{candidate.name}</h2>
              <span className="academic-status">
                {copy.capacity} · {candidate.free}
              </span>
            </div>
            <p>{candidate.expertise}</p>
            <h3>{copy.evidence}</h3>
            <p>{candidate.evidence}</p>
            <label className="academic-choice">
              <input
                type="checkbox"
                checked={selected.includes(candidate.id)}
                onChange={(event) => {
                  const checked = event.target.checked;
                  setSelected((previous) =>
                    checked
                      ? [...previous, candidate.id]
                      : previous.filter((id) => id !== candidate.id),
                  );
                }}
              />
              {copy.candidateLabel}
              <span className="academic-sr-only"> {candidate.name}</span>
            </label>
          </article>
        ))}
      </div>
      {selected.length >= 2 ? (
        <Link
          className="academic-compare"
          to={
            copy.comparisonRoute +
            "?" +
            new URLSearchParams(selected.map((id) => ["candidate", id]))
          }
        >
          {copy.compare}
        </Link>
      ) : (
        <p className="academic-result" role="status">
          {copy.nothing}
        </p>
      )}
    </section>
  );
}

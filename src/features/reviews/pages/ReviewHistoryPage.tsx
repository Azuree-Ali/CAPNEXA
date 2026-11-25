import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { reviewsAtlasDesignContent as copy } from "../content/reviewsAtlasDesign.content";

export function ReviewHistoryPage() {
  return (
    <section className="academic-workspace">
      <WorkspaceHeading title={copy.title} description={copy.description} parent={copy.parent} />
      <ol className="academic-timeline">
        {copy.versions.map((version) => (
          <li key={version.id} className="academic-records academic-panel">
            <div className="academic-card-heading">
              <h2>
                {version.id} · {version.title}
              </h2>
              <span className="academic-status">{version.status}</span>
            </div>
            <p className="academic-secondary">
              {version.date} · {version.reviewer}
            </p>
            <p>{version.feedback}</p>
          </li>
        ))}
      </ol>
    </section>
  );
}

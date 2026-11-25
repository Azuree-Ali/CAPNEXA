import { Link, useSearchParams } from "react-router-dom";
import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { WorkspaceMetrics } from "@/components/data-table/WorkspaceMetrics";
import { projectsCampusAtlasContent as copy } from "../content/projectsCampusAtlas.content";

export function ProjectDetailPage() {
  const [params] = useSearchParams();
  const selected = copy.tabs.find((tab) => tab.id === params.get("view")) ?? copy.tabs[0];
  return (
    <section className="academic-workspace">
      <WorkspaceHeading title={copy.title} description={copy.description} parent={copy.parent} />
      <WorkspaceMetrics items={copy.facts} />
      <nav className="academic-views" aria-label={copy.view}>
        {copy.tabs.map((tab) => (
          <Link
            key={tab.id}
            to={`?view=${tab.id}`}
            aria-current={tab.id === selected.id ? "page" : undefined}
            preventScrollReset
          >
            {tab.label}
          </Link>
        ))}
      </nav>
      <section className="academic-records academic-panel" aria-labelledby="project-view-heading">
        <h2 id="project-view-heading">{selected.title}</h2>
        <ul className="academic-list">
          {selected.items.map((item) => (
            <li key={item}>{item}</li>
          ))}
        </ul>
        {selected.related && <Link to={selected.related.route}>{selected.related.label}</Link>}
      </section>
    </section>
  );
}

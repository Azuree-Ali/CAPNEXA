import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { WorkspaceMetrics } from "@/components/data-table/WorkspaceMetrics";
import { milestonesTimelineContent as copy } from "../content/milestonesTimeline.content";

export function MilestoneTimelinePage() {
  const complete = copy.stages.filter((stage) => stage.complete).length;
  return (
    <section className="academic-workspace">
      <WorkspaceHeading title={copy.title} description={copy.description} parent={copy.parent} />
      <WorkspaceMetrics
        items={[{ label: copy.progress, value: `${complete} / ${copy.stages.length}` }]}
      />
      <ol className="academic-timeline">
        {copy.stages.map((stage) => (
          <li key={stage.title} className="academic-records academic-panel">
            <div className="academic-card-heading">
              <h2>{stage.title}</h2>
              <span className="academic-status">{stage.status}</span>
            </div>
            <p className="academic-secondary">{stage.due}</p>
            <p>{stage.deliverable}</p>
          </li>
        ))}
      </ol>
    </section>
  );
}

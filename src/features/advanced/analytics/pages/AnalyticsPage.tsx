import { Link } from "react-router-dom";
import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { WorkspaceMetrics } from "@/components/data-table/WorkspaceMetrics";
import { analyticsContent as copy } from "../content/analytics.content";

export function AnalyticsPage() {
  const projects = copy.stages.reduce((sum, stage) => sum + stage.count, 0);
  const overdue = copy.milestones.filter((item) => item.state === "overdue").length;
  const average = copy.reviews.length
    ? Math.round(
        copy.reviews.reduce((sum, review) => sum + review.duration, 0) / copy.reviews.length,
      )
    : null;
  return (
    <section className="academic-workspace">
      <WorkspaceHeading title={copy.title} description={copy.description} />
      <WorkspaceMetrics
        items={[
          { label: copy.projectsLabel, value: projects },
          { label: copy.milestonesLabel, value: copy.milestones.length },
          { label: copy.overdueLabel, value: overdue },
          { label: copy.average, value: average === null ? "—" : average + " " + copy.days },
        ]}
      />
      <div className="academic-card-grid">
        <section className="academic-records academic-panel">
          <h2>{copy.stageHeading}</h2>
          {copy.stages.map((stage) => (
            <div key={stage.label} className="academic-chart-row">
              <Link to={stage.route}>{stage.label}</Link>
              <progress aria-label={stage.label} value={stage.count} max={projects || 1} />
              <strong>{stage.count}</strong>
            </div>
          ))}
        </section>
        <section className="academic-records academic-panel">
          <h2>{copy.reviewHeading}</h2>
          {!copy.reviews.length && <p>{copy.empty}</p>}
          {copy.reviews.map((review) => (
            <div key={review.title} className="academic-card-heading">
              <Link to={review.route}>{review.title}</Link>
              <strong>
                {review.duration} {copy.days}
              </strong>
            </div>
          ))}
        </section>
      </div>
      <section className="academic-records academic-panel academic-result">
        <h2>{copy.delayHeading}</h2>
        {!copy.milestones.length && <p>{copy.empty}</p>}
        {copy.milestones.map((item) => (
          <div key={item.title} className="academic-card-heading">
            <Link to={item.route}>{item.title}</Link>
            <span className="academic-status">
              {item.state === "complete"
                ? copy.completedLabel
                : item.state === "overdue"
                  ? copy.overdueLabel
                  : copy.riskLabel}
            </span>
          </div>
        ))}
      </section>
    </section>
  );
}

import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { WorkspaceMetrics } from "@/components/data-table/WorkspaceMetrics";
import { evaluationsContent as copy } from "../content/evaluations.content";

export function EvaluationsPage() {
  const total = copy.assessments.reduce((sum, item) => sum + item.score, 0);
  const maximum = copy.assessments.reduce((sum, item) => sum + item.maximum, 0);
  return (
    <section className="academic-workspace">
      <WorkspaceHeading title={copy.title} description={copy.description} />
      <WorkspaceMetrics
        items={[
          { label: copy.projectLabel, value: copy.project },
          { label: copy.overall, value: `${total} / ${maximum}` },
          {
            label: copy.percentage,
            value: maximum ? `${Math.round((total / maximum) * 100)}%` : "—",
          },
        ]}
      />
      <div className="academic-records academic-scroll">
        <table>
          <thead>
            <tr>
              {copy.columns.map((label) => (
                <th key={label} scope="col">
                  {label}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {copy.assessments.map((item) => (
              <tr key={item.criterion}>
                <th scope="row">{item.criterion}</th>
                <td>
                  {item.score} / {item.maximum}
                </td>
                <td>{item.feedback}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}

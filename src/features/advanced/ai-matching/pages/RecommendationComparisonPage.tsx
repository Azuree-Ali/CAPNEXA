import { useSearchParams } from "react-router-dom";
import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { recommendationsContent } from "../content/recommendations.content";
import { recommendationsCompareContent as copy } from "../content/recommendationsCompare.content";

export function RecommendationComparisonPage() {
  const [params] = useSearchParams();
  const selected = params.getAll("candidate");
  const matching = recommendationsContent.candidates.filter((candidate) =>
    selected.includes(candidate.id),
  );
  const candidates = matching.length >= 2 ? matching : recommendationsContent.candidates;
  const criteria = [
    { label: copy.expertise, values: candidates.map((candidate) => candidate.expertise) },
    { label: copy.availability, values: candidates.map((candidate) => candidate.free) },
    { label: copy.evidence, values: candidates.map((candidate) => candidate.evidence) },
  ];
  return (
    <section className="academic-workspace">
      <WorkspaceHeading title={copy.title} description={copy.description} parent={copy.parent} />
      <div className="academic-records academic-scroll">
        <table>
          <thead>
            <tr>
              <th scope="col">{copy.field}</th>
              {candidates.map((candidate) => (
                <th key={candidate.id} scope="col">
                  {candidate.name}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {criteria.map((criterion) => (
              <tr key={criterion.label}>
                <th scope="row">{criterion.label}</th>
                {criterion.values.map((value, index) => (
                  <td key={candidates[index].id}>{value}</td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}

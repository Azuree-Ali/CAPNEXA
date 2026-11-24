import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { WorkspaceMetrics } from "@/components/data-table/WorkspaceMetrics";
import { WorkspaceView } from "@/components/data-table/WorkspaceView";
import {
  supervisorsOliviaBennettContent as copy,
  supervisorWorkload as workload,
} from "../content/supervisorsOliviaBennett.content";

export function SupervisorWorkloadPage() {
  const assigned = copy.records.length;
  return (
    <section className="academic-workspace">
      <WorkspaceHeading
        title={copy.title}
        description={copy.description}
        parent={workload.parent}
      />
      <WorkspaceMetrics
        items={[
          { label: workload.assigned, value: assigned },
          { label: workload.available, value: Math.max(0, workload.capacity - assigned) },
          { label: workload.maximum, value: workload.capacity },
        ]}
      />
      <section className="academic-records academic-panel academic-result">
        <h2>{workload.interestsLabel}</h2>
        <p>{workload.specialty}</p>
        <ul className="academic-list">
          {workload.interests.map((interest) => (
            <li key={interest}>{interest}</li>
          ))}
        </ul>
      </section>
      <div className="academic-result">
        <WorkspaceView content={copy} route="supervisors/olivia-bennett" embedded />
      </div>
    </section>
  );
}

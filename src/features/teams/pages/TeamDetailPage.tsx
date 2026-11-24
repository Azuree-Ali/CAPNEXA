import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { WorkspaceMetrics } from "@/components/data-table/WorkspaceMetrics";
import { WorkspaceView } from "@/components/data-table/WorkspaceView";
import { connected } from "@/lib/api/api-client";
import { teamsAtlasContent as copy, atlasTeamContext } from "../content/teamsAtlas.content";

export function TeamDetailPage() {
  return (
    <section className="academic-workspace">
      <WorkspaceHeading
        title={copy.title}
        description={copy.description}
        parent={atlasTeamContext.parent}
      />
      <WorkspaceMetrics items={atlasTeamContext.facts} />
      <WorkspaceView
        content={
          connected ? { ...copy, records: copy.records.map((row) => ({ cells: row.cells })) } : copy
        }
        route="teams/atlas"
        embedded
      />
    </section>
  );
}

import { useRef, useState } from "react";
import {
  applyPreviewChange,
  previewProposalByTitle,
} from "@/features/dashboard/hooks/operationsPreview";
import { useNavigate } from "react-router-dom";
import { WorkspaceView } from "@/components/data-table/WorkspaceView";
import type { WorkspaceContent, WorkspaceRecord } from "@/components/data-table/workspace.content";
import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { decisions as t, proposalDossier as dossier } from "../content/proposalsReview.content";
type Action = "approve" | "reject" | "revisions" | "assign";
let decision = t.pending;
let notes = "";
const allocations = new Map<string, string>();
export function ProposalPreviewPage({
  route,
  content,
}: {
  route: string;
  content: WorkspaceContent;
}) {
  const navigate = useNavigate();
  const [action, setAction] = useState<Action>("approve");
  const [value, setValue] = useState("");
  const [error, setError] = useState("");
  const [project, setProject] = useState("");
  const dialog = useRef<HTMLDialogElement>(null);
  const open = (a: Action, p: string) => {
    setError("");
    setAction(a);
    setProject(p);
    setValue("");
    dialog.current?.showModal();
  };
  const proposal = previewProposalByTitle(dossier.name);
  const shownDecision =
    proposal?.status === "Approved"
      ? t.approved
      : proposal?.status === "Rejected"
        ? t.rejected
        : decision;
  const shownNotes = proposal?.review?.reason ?? notes;
  const projectNames = [
    ...content.records.map((record) => record.cells[0]),
    ...(decision === t.approved ? [dossier.name] : []),
  ];
  let records: WorkspaceRecord[] = content.records;
  if (route === "proposals")
    records = records.map((r) => ({ ...r, cells: [r.cells[0], r.cells[1], decision] }));
  if (route === "assignments")
    records = projectNames.map((p, i) => ({
      cells: [p, i === 0 ? "Maps" : "Lens", allocations.get(p) ?? t.unassigned],
    }));
  function submit() {
    if ((action === "reject" || action === "revisions") && !value.trim()) {
      setError(dossier.requiredReason);
      return;
    }
    if ((action === "approve" || action === "reject") && proposal) {
      try {
        applyPreviewChange(action, String(proposal.projectId), value);
      } catch (cause) {
        setError(cause instanceof Error ? cause.message : dossier.requiredReason);
        return;
      }
    }
    if (action === "assign") allocations.set(project, value);
    else {
      decision = action === "approve" ? t.approved : action === "reject" ? t.rejected : t.returned;
      notes = value.trim();
    }
    dialog.current?.close();
    navigate(
      action === "approve" || action === "assign" ? "/admin/assignments" : "/admin/proposals",
    );
  }
  return (
    <div className="academic-workspace">
      {route === "proposals/review" && (
        <WorkspaceHeading
          title={content.title}
          description={content.description}
          parent={dossier.parent}
        >
          <span className="academic-status academic-result">{shownDecision}</span>
        </WorkspaceHeading>
      )}
      <WorkspaceView
        content={{ ...content, records }}
        route={route}
        embedded={route === "proposals/review"}
      />
      {route === "proposals/review" && (
        <>
          <div className="academic-card-grid academic-result">
            <section className="academic-records academic-panel">
              <h2>{dossier.evidence}</h2>
              <div className="academic-disclosures">
                {dossier.attachments.map((item) => (
                  <details key={item.name}>
                    <summary>
                      <span>{item.name}</span>
                    </summary>
                    <p className="academic-panel">{item.detail}</p>
                  </details>
                ))}
              </div>
            </section>
            <section className="academic-records academic-panel">
              <h2>{dossier.history}</h2>
              <ol className="academic-list">
                {dossier.historyEntries.map((item) => (
                  <li key={item}>{item}</li>
                ))}
                {shownNotes && <li>{shownNotes}</li>}
              </ol>
            </section>
          </div>
          {shownDecision === t.pending && (
            <div className="academic-tools">
              {(["approve", "reject", "revisions"] as const).map((a) => (
                <button key={a} onClick={() => open(a, dossier.name)}>
                  {t[a]}
                </button>
              ))}
            </div>
          )}
        </>
      )}
      {route === "assignments" && (
        <div className="academic-tools">
          {projectNames
            .filter((p) => !allocations.has(p))
            .map((p) => (
              <button key={p} onClick={() => open("assign", p)}>
                {t.assign} · {p}
              </button>
            ))}
        </div>
      )}
      <dialog ref={dialog} className="info-dialog" aria-labelledby="decision-heading">
        <form
          onSubmit={(e) => {
            e.preventDefault();
            submit();
          }}
        >
          <h2 id="decision-heading">{t[action]}</h2>
          <p>{project}</p>
          <small>{t.demo}</small>
          {action !== "assign" && <p>{dossier.guidance[action]}</p>}
          {error && <p role="alert">{error}</p>}
          {action === "assign" ? (
            <label>
              {t.capacity}
              <select required value={value} onChange={(e) => setValue(e.target.value)}>
                <option value="">—</option>
                <option>{t.faculty}</option>
              </select>
            </label>
          ) : (
            <label>
              {t.reason}
              <textarea
                required={action !== "approve"}
                maxLength={2000}
                value={value}
                onChange={(e) => setValue(e.target.value)}
              />
            </label>
          )}
          <button type="button" onClick={() => dialog.current?.close()}>
            {t.cancel}
          </button>
          <button>{t.confirm}</button>
        </form>
      </dialog>
    </div>
  );
}

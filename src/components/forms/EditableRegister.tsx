import { useRef, useState } from "react";
import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { WorkspaceView } from "@/components/data-table/WorkspaceView";
import type { WorkspaceContent, WorkspaceRecord } from "@/components/data-table/workspace.content";
export interface RegisterActions {
  add: string;
  edit: string;
  save: string;
  cancel: string;
  notice: string;
  readOnlyNotice: string;
  duplicate: string;
  saved: string;
  projectFilter: string;
  allProjects: string;
  template: string;
  custom: string;
  templates: readonly { name: string; values: readonly [string, string, string] }[];
}
interface Props {
  content: WorkspaceContent;
  actions: RegisterActions;
  route: string;
  readOnly?: boolean;
}
const previewRecords = new Map<string, WorkspaceRecord[]>();
export function EditableRegister({ content, actions, route, readOnly = false }: Props) {
  const [records, setRecords] = useState(() => previewRecords.get(route) ?? content.records);
  const [project, setProject] = useState("");
  const [saved, setSaved] = useState(false);
  const visibleRecords = records.filter((record) => !project || record.cells[2] === project);
  const projects = [...new Set(records.map((record) => record.cells[2]))];
  const [templateName, setTemplateName] = useState("");
  const [index, selectIndex] = useState(-1);
  const [cells, setCells] = useState<[string, string, string]>(["", "", ""]);
  const [error, setError] = useState("");
  const dialog = useRef<HTMLDialogElement>(null);
  function open(i: number) {
    if (readOnly) return;
    setSaved(false);
    setTemplateName("");
    selectIndex(i);
    setCells(i < 0 ? ["", "", ""] : [...records[i].cells]);
    setError("");
    dialog.current?.showModal();
  }
  function save() {
    if (readOnly) return;
    const trimmed: [string, string, string] = [cells[0].trim(), cells[1].trim(), cells[2].trim()];
    if (
      trimmed.some((s) => !s) ||
      records.some((r, i) => i !== index && r.cells[0].toLowerCase() === trimmed[0].toLowerCase())
    ) {
      setError(actions.duplicate);
      return;
    }
    const next = records.slice();
    next[index < 0 ? next.length : index] = { ...records[index], cells: trimmed };
    previewRecords.set(route, next);
    setRecords(next);
    setProject("");
    setSaved(true);
    dialog.current?.close();
  }
  return (
    <div className="academic-workspace">
      <WorkspaceHeading title={content.title} description={content.description} />
      {readOnly && <p className="academic-panel academic-records">{actions.readOnlyNotice}</p>}
      <div className="academic-records academic-tools">
        <label>
          {actions.projectFilter}
          <select value={project} onChange={(event) => setProject(event.target.value)}>
            <option value="">{actions.allProjects}</option>
            {projects.map((item) => (
              <option key={item}>{item}</option>
            ))}
          </select>
        </label>
        {!readOnly && (
          <>
            <button onClick={() => open(-1)}>{actions.add}</button>
            <label>
              {actions.edit}
              <select value="" onChange={(e) => open(Number(e.target.value))}>
                <option value="" disabled>
                  —
                </option>
                {records.map((r, i) => (
                  <option key={r.cells[0]} value={i}>
                    {r.cells[0]}
                  </option>
                ))}
              </select>
            </label>
          </>
        )}
      </div>
      {saved && (
        <p className="academic-panel" role="status">
          {actions.saved}
        </p>
      )}
      <div className="academic-result">
        <WorkspaceView route={route} content={{ ...content, records: visibleRecords }} embedded />
      </div>
      {!readOnly && (
        <dialog className="info-dialog" ref={dialog} aria-labelledby="register-title">
          <form
            onSubmit={(e) => {
              e.preventDefault();
              save();
            }}
          >
            <h2 id="register-title">{index < 0 ? actions.add : actions.edit}</h2>
            <p>{actions.notice}</p>
            {index < 0 && (
              <label>
                {actions.template}
                <select
                  value={templateName}
                  onChange={(event) => {
                    setTemplateName(event.target.value);
                    const template = actions.templates.find(
                      (item) => item.name === event.target.value,
                    );
                    setCells(template ? [...template.values] : ["", "", project]);
                  }}
                >
                  <option value="">{actions.custom}</option>
                  {actions.templates.map((template) => (
                    <option key={template.name}>{template.name}</option>
                  ))}
                </select>
              </label>
            )}
            {content.columns.map((label, i) => (
              <label key={label}>
                {label}
                <input
                  required
                  type={i === 1 ? "date" : "text"}
                  maxLength={180}
                  value={cells[i]}
                  onChange={(e) => {
                    const next: [string, string, string] = [...cells];
                    next[i] = e.target.value;
                    setCells(next);
                  }}
                />
              </label>
            ))}
            {error && <p role="alert">{error}</p>}
            <button type="button" onClick={() => dialog.current?.close()}>
              {actions.cancel}
            </button>
            <button>{actions.save}</button>
          </form>
        </dialog>
      )}
    </div>
  );
}

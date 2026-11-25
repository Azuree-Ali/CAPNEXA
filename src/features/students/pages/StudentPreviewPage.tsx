import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { WorkspaceMetrics } from "@/components/data-table/WorkspaceMetrics";
import { useRef, useState, useSyncExternalStore } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  exampleStudent,
  studentForm as t,
  studentAcademicContext as context,
  type StudentRecord,
} from "../content/studentProfile.content";
import { studentsContent, studentDirectoryMetrics as metrics } from "../content/students.content";
import { WorkspaceView } from "@/components/data-table/WorkspaceView";
import { subscribe, snapshot, save } from "../hooks/studentPreview";
type Mode = "add" | "edit" | "team" | "project";
export function StudentPreviewPage() {
  const students = useSyncExternalStore(subscribe, snapshot);
  const { id } = useParams();
  const navigate = useNavigate();
  const student = students.find((s) => s.id === id);
  const dialog = useRef<HTMLDialogElement>(null);
  const [draft, setDraft] = useState<StudentRecord>(exampleStudent);
  const [mode, setMode] = useState<Mode>("add");
  const [error, setError] = useState("");
  function open(next: Mode) {
    setMode(next);
    setError("");
    setDraft(
      next === "add"
        ? {
            ...exampleStudent,
            id: crypto.randomUUID(),
            fullName: "",
            email: "",
            role: t.roles[1],
            team: "",
            project: "",
          }
        : { ...(student ?? exampleStudent) },
    );
    dialog.current?.showModal();
  }
  function submit() {
    if (
      !draft.fullName.trim() ||
      !draft.email.trim() ||
      students.some(
        (s) => s.id !== draft.id && s.email.toLowerCase() === draft.email.trim().toLowerCase(),
      )
    ) {
      setError(t.invalid);
      return;
    }
    save({ ...draft, fullName: draft.fullName.trim(), email: draft.email.trim() });
    dialog.current?.close();
    navigate("/admin/students/" + draft.id);
  }
  const fields = ["fullName", "email", "department", "year"] as const;
  return (
    <div className="academic-workspace">
      {!id ? (
        <>
          <WorkspaceHeading
            title={studentsContent.title}
            description={studentsContent.description}
          />
          <WorkspaceMetrics
            items={[
              { label: metrics.total, value: students.length },
              { label: metrics.active, value: students.filter((record) => record.active).length },
              { label: metrics.assigned, value: students.filter((record) => record.team).length },
            ]}
          />
          <WorkspaceView
            embedded
            route="students"
            content={{
              ...studentsContent,
              records: students.map((s) => ({
                cells: [
                  s.fullName,
                  s.id === exampleStudent.id ? context.displayId : s.id,
                  s.department,
                ],
                route: "/admin/students/" + s.id,
              })),
            }}
          />
          <button onClick={() => open("add")}>{t.add}</button>
        </>
      ) : student ? (
        <>
          <Link to="/admin/students">{t.back}</Link>
          <header>
            <h1>{student.fullName}</h1>
            <p>{t.detail}</p>
            <small>{t.preview}</small>
          </header>
          <div className="academic-records">
            <dl className="record-fields">
              {fields.map((f) => (
                <div key={f}>
                  <dt>{t[f]}</dt>
                  <dd>{student[f]}</dd>
                </div>
              ))}
              <div>
                <dt>{t.teamLabel}</dt>
                <dd>{student.team || "—"}</dd>
              </div>
              <div>
                <dt>{t.projectLabel}</dt>
                <dd>{student.project || "—"}</dd>
              </div>
              <div>
                <dt>{t.role}</dt>
                <dd>{student.role}</dd>
              </div>
              <div>
                <dt>{t.status}</dt>
                <dd>{student.active ? t.active : t.inactive}</dd>
              </div>
            </dl>
            <div className="academic-tools">
              {(["edit", "team", "project"] as const).map((m) => (
                <button key={m} onClick={() => open(m)}>
                  {t[m]}
                </button>
              ))}
              <button onClick={() => save({ ...student, active: !student.active })}>
                {t.change}
              </button>
            </div>
          </div>
          {student.project === exampleStudent.project && (
            <section className="academic-records academic-panel academic-result">
              <h2>{context.context}</h2>
              <Link to={context.timelineRoute}>{context.timeline}</Link>
            </section>
          )}
        </>
      ) : (
        <p>{t.empty}</p>
      )}
      <dialog ref={dialog} className="info-dialog" aria-labelledby="student-form-title">
        <form
          onSubmit={(e) => {
            e.preventDefault();
            submit();
          }}
        >
          <h2 id="student-form-title">{t[mode]}</h2>
          <button
            type="button"
            className="dialog-close"
            onClick={() => dialog.current?.close()}
            aria-label={t.close}
          >
            ×
          </button>
          <small>{t.preview}</small>
          {(mode === "add" || mode === "edit") && (
            <>
              <label>
                {t.id}
                <input
                  value={draft.id === exampleStudent.id ? context.displayId : draft.id}
                  readOnly
                />
              </label>
              {fields.map((f) => (
                <label key={f}>
                  {t[f]}
                  <input
                    required
                    maxLength={160}
                    value={draft[f]}
                    type={f === "email" ? "email" : "text"}
                    onChange={(e) => setDraft({ ...draft, [f]: e.target.value })}
                  />
                </label>
              ))}
              <label>
                {t.role}
                <select
                  value={draft.role}
                  onChange={(e) => setDraft({ ...draft, role: e.target.value })}
                >
                  {t.roles.map((x) => (
                    <option key={x}>{x}</option>
                  ))}
                </select>
              </label>
            </>
          )}
          {(mode === "team" || mode === "project") && (
            <>
              <p>
                {mode === "team" ? context.currentTeam : context.currentProject} ·{" "}
                {student?.[mode] || "—"}
              </p>
              <label>
                {t[mode]}
                <select
                  required
                  value={draft[mode]}
                  onChange={(e) => setDraft({ ...draft, [mode]: e.target.value })}
                >
                  <option value="">—</option>
                  {(mode === "team" ? t.teams : t.projects).map((x) => (
                    <option key={x}>{x}</option>
                  ))}
                </select>
              </label>
            </>
          )}
          {error && <p role="alert">{error}</p>}
          <button type="button" onClick={() => dialog.current?.close()}>
            {t.cancel}
          </button>
          <button>{t.save}</button>
        </form>
      </dialog>
    </div>
  );
}

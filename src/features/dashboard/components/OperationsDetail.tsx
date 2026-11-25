import { connected } from "@/lib/api/api-client";
import { TableSkeleton } from "@/components/feedback/Skeleton";
import { useEffect, useRef, useState } from "react";
import { ArrowLeft, CheckCircle2, X } from "lucide-react";
import { SupervisorPicker } from "@/features/assignments/components/SupervisorPicker";
import { operationPages, operations as copy } from "../content/operations.content";
import { applyChange, loadDetail } from "../hooks/operations";
import type {
  Change,
  Detail,
  Entry,
  OperationDomain,
  RecordAction,
  RecordField,
} from "../types/operations.types";

const t = copy.text;
const maxCapacity = 20;
const maxReasonLength = 2000;
type Result = { data: Detail } | { error: string } | null;

interface DialogTarget {
  action: Change;
  id: string;
  title: string;
  isSubrecord: boolean;
}

function Fields({ fields }: { fields: RecordField[] }) {
  return (
    <dl className="record-fields">
      {fields.map(([label, value], index) => (
        <div key={label + index}>
          <dt>{label}</dt>
          <dd>{value}</dd>
        </div>
      ))}
    </dl>
  );
}

export function OperationsDetail({
  domain,
  row,
  onClose,
}: {
  domain: OperationDomain;
  row: Entry;
  onClose: (change?: Change) => void;
}) {
  const [result, setResult] = useState<Result>(null);
  const [revision, reload] = useState(0);
  const [target, setTarget] = useState<DialogTarget | null>(null);
  const [value, setValue] = useState("");
  const [error, setError] = useState("");
  const [subSuccess, setSubSuccess] = useState("");
  const [busy, setBusy] = useState(false);
  const writeRequest = useRef<AbortController>(null);
  const dialog = useRef<HTMLDialogElement>(null);
  const title = useRef<HTMLHeadingElement>(null);
  const data = result && "data" in result ? result.data : null;
  const minimumCapacity = data?.capacity?.assigned ?? 0;

  useEffect(() => {
    title.current?.focus();
    return () => writeRequest.current?.abort();
  }, []);

  useEffect(() => {
    const abort = new AbortController();
    // [BACKEND-API] Read the selected student, supervisor, or proposal. See operations.ts for path params and detail DTOs.
    loadDetail(domain, row, abort.signal).then(
      (data) => {
        if (!abort.signal.aborted) setResult({ data });
      },
      (error: unknown) => {
        if (!abort.signal.aborted)
          setResult({ error: error instanceof Error ? error.message : t.error });
      },
    );
    return () => abort.abort();
  }, [domain, row, revision]);

  function openChange(change: Change) {
    setError("");
    setSubSuccess("");
    setTarget({ action: change, id: row.id, title: data?.title ?? row.title, isSubrecord: false });
    setValue(change === "capacity" && data?.capacity ? String(data.capacity.maximum) : "");
    dialog.current?.showModal();
  }

  function openSubrecordChange(recordAction: RecordAction) {
    setError("");
    setSubSuccess("");
    setTarget({
      action: recordAction.change,
      id: recordAction.targetId,
      title: recordAction.targetTitle,
      isSubrecord: true,
    });
    setValue("");
    dialog.current?.showModal();
  }

  async function confirm() {
    if (!target || writeRequest.current) return;
    const { action, id, isSubrecord } = target;
    if (
      action === "capacity" &&
      (!value.trim() ||
        !Number.isInteger(Number(value)) ||
        Number(value) < minimumCapacity ||
        Number(value) > maxCapacity)
    ) {
      setError(t.capacityError);
      return;
    }
    if (action === "assign" && (!Number.isSafeInteger(Number(value)) || Number(value) < 1)) {
      setError(t.requiredSupervisor);
      return;
    }
    if ((action === "reject" && !value.trim()) || value.length > maxReasonLength) {
      setError(t.reasonRequired);
      return;
    }

    const request = new AbortController();
    writeRequest.current = request;
    setBusy(true);
    setError("");
    try {
      // [BACKEND-API] Submit the selected PUT operation. See operations.ts for request DTO and response contract by action.
      await applyChange(action, id, value, request.signal);
      if (!request.signal.aborted) {
        dialog.current?.close();
        if (isSubrecord) {
          setSubSuccess(t.reassignSuccess);
          setResult(null);
          reload((v) => v + 1);
        } else {
          onClose(action);
        }
      }
    } catch (error: unknown) {
      if (!request.signal.aborted) setError(error instanceof Error ? error.message : t.error);
    } finally {
      writeRequest.current = null;
      if (!request.signal.aborted) setBusy(false);
    }
  }

  const action = target?.action ?? null;
  const DialogIcon = action ? copy.actionIcons[action] : null;
  const dialogHeading = target?.isSubrecord ? t.reassign : action ? t[action] : "";

  return (
    <section className="academic-workspace connected-workspace operation-detail">
      <header className="academic-heading">
        <button type="button" onClick={() => onClose()} className="operation-back">
          <ArrowLeft size={16} aria-hidden="true" />
          {t.back}
        </button>
        <h1 tabIndex={-1} ref={title}>
          {data?.title ?? row.title}
        </h1>
        <p>{operationPages[domain].description}</p>
        {data && data.actions.length > 0 && (
          <div className="operation-actions">
            {data.actions.map((item) => {
              const Icon = copy.actionIcons[item];
              return (
                <button
                  type="button"
                  key={item}
                  data-action={item}
                  onClick={() => openChange(item)}
                >
                  <Icon size={16} aria-hidden="true" />
                  {t[item]}
                </button>
              );
            })}
          </div>
        )}
      </header>
      {subSuccess && (
        <div className="operation-success" role="status">
          <CheckCircle2 size={18} aria-hidden="true" />
          <span>
            {subSuccess}
            {!connected && ` ${t.preview}`}
          </span>
        </div>
      )}
      {!result && (
        <div className="academic-records academic-empty is-loading" role="status">
          {t.loading}
          <TableSkeleton rows={4} columns={3} />
        </div>
      )}
      {result && "error" in result && (
        <div className="academic-records academic-empty">
          <p role="alert">{result.error}</p>
          <button
            type="button"
            onClick={() => {
              setResult(null);
              reload((value) => value + 1);
            }}
          >
            {t.retry}
          </button>
        </div>
      )}
      {data && (
        <>
          <section className="academic-records operation-overview">
            <h2>{t.recordOverview}</h2>
            <Fields fields={data.fields} />
            {data.capacity && (
              <div className="operation-capacity">
                <div className="operation-capacity-meta">
                  <span>
                    {t.assigned} · {data.capacity.assigned} / {data.capacity.maximum}
                  </span>
                  <span>
                    {t.remainingSlots}:{" "}
                    {Math.max(0, data.capacity.maximum - data.capacity.assigned)}
                  </span>
                </div>
                <progress
                  aria-label={t.assigned}
                  value={data.capacity.assigned}
                  max={Math.max(1, data.capacity.maximum)}
                />
              </div>
            )}
          </section>
          {data.sections?.map((group) => (
            <section
              key={group.title}
              className="academic-records operation-overview academic-result"
            >
              <h2>{group.title}</h2>
              {group.records.length ? (
                group.records.map((record) => (
                  <div className="operation-subrecord" key={record.id}>
                    <div className="operation-subrecord-head">
                      <h3>{record.title}</h3>
                      {record.action && (
                        <button
                          type="button"
                          className="operation-subaction"
                          onClick={() => openSubrecordChange(record.action!)}
                        >
                          {record.action.label}
                        </button>
                      )}
                    </div>
                    <Fields fields={record.fields} />
                  </div>
                ))
              ) : (
                <p className="academic-empty">{group.empty}</p>
              )}
            </section>
          ))}
        </>
      )}
      <dialog
        className="info-dialog operation-dialog"
        ref={dialog}
        aria-labelledby="change-title"
        aria-describedby="change-description"
        onClose={() => setTarget(null)}
        onCancel={(event) => {
          if (busy) event.preventDefault();
        }}
      >
        <form
          aria-busy={busy}
          onSubmit={(event) => {
            event.preventDefault();
            void confirm();
          }}
        >
          <div className="operation-dialog-head">
            <div className="operation-dialog-title">
              {DialogIcon && (
                <span className="operation-dialog-icon" data-action={action} aria-hidden="true">
                  <DialogIcon size={20} strokeWidth={1.8} />
                </span>
              )}
              <h2 id="change-title">{dialogHeading}</h2>
            </div>
            <button
              type="button"
              className="dialog-close"
              disabled={busy}
              aria-label={t.closeDialog}
              onClick={() => dialog.current?.close()}
            >
              <X size={18} aria-hidden="true" />
            </button>
          </div>
          <p className="operation-dialog-record">
            <strong>{target?.title ?? data?.title ?? row.title}</strong>
          </p>
          <p id="change-description">
            {action && copy.confirmation[action]}
            {!connected && <span className="operation-secondary">{t.preview}</span>}
          </p>
          {action === "assign" && (
            <SupervisorPicker
              value={value}
              onChange={setValue}
              busy={busy}
              excludedId={target?.isSubrecord ? Number(row.id) : undefined}
            />
          )}
          {action === "capacity" && (
            <label>
              {t.capacity}
              <input
                type="number"
                step="1"
                required
                min={minimumCapacity}
                max={maxCapacity}
                value={value}
                disabled={busy}
                onChange={(event) => setValue(event.target.value)}
              />
              <small>{t.capacityHint}</small>
            </label>
          )}
          {(action === "approve" || action === "reject") && (
            <label>
              {t.reason}
              <textarea
                maxLength={maxReasonLength}
                disabled={busy}
                required={action === "reject"}
                value={value}
                onChange={(event) => setValue(event.target.value)}
              />
            </label>
          )}
          {error && (
            <p role="alert" className="operation-error">
              {error}
            </p>
          )}
          {busy && <p role="status">{t.saving}</p>}
          <div className="operation-actions">
            <button type="button" disabled={busy} onClick={() => dialog.current?.close()}>
              {t.cancel}
            </button>
            <button
              type="submit"
              data-action={action ?? undefined}
              disabled={busy || (action === "assign" && !value)}
            >
              {t.confirm}
            </button>
          </div>
        </form>
      </dialog>
    </section>
  );
}

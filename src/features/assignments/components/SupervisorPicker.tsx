import { ApiHttpError } from "@/lib/api/api-client";
import { useEffect, useState } from "react";
import { listSupervisors } from "@/features/supervisors/api/supervisors.api";
import type { AdminSupervisorListDto } from "@/features/supervisors/types/supervisors.types";
import { supervisorPickerContent as copy } from "../content/supervisorPicker.content";

type Result = { data: AdminSupervisorListDto } | { error: string | null } | null;
export function SupervisorPicker({
  value,
  onChange,
  busy,
}: {
  value: string;
  onChange: (id: string) => void;
  busy: boolean;
}) {
  const [input, setInput] = useState("");
  const [request, setRequest] = useState({ search: "", page: 1 });
  const [result, setResult] = useState<Result>(null);
  useEffect(() => {
    const controller = new AbortController();
    // [BACKEND-API] Find eligible faculty. GET /api/Admin/Supervisors. AdminSupervisorQueryDto -> AdminSupervisorListDto.
    listSupervisors(
      { ...request, pageSize: 20, isActive: true, hasCapacity: true },
      controller.signal,
    )
      .then((data) => {
        if (!controller.signal.aborted) setResult({ data });
      })
      .catch((error: unknown) => {
        if (!controller.signal.aborted)
          setResult({ error: error instanceof ApiHttpError ? error.message : null });
      });
    return () => controller.abort();
  }, [request]);

  function load(search: string, page: number) {
    onChange("");
    setResult(null);
    setRequest({ search, page });
  }

  const options = result && "data" in result ? result.data.items : [];
  return (
    <div className="academic-picker">
      <div className="academic-inline-actions">
        <label>
          {copy.search}
          <input
            value={input}
            disabled={busy}
            onChange={(event) => setInput(event.target.value)}
            onKeyDown={(event) => {
              if (event.key === "Enter") {
                event.preventDefault();
                load(input.trim(), 1);
              }
            }}
          />
        </label>
        <button type="button" disabled={busy} onClick={() => load(input.trim(), 1)}>
          {copy.filter}
        </button>
      </div>
      {!result && <p role="status">{copy.loading}</p>}
      {result && "error" in result && (
        <div role="alert">
          {copy.error} {result.error}{" "}
          <button type="button" onClick={() => load(request.search, request.page)}>
            {copy.retry}
          </button>
        </div>
      )}
      {result && "data" in result && !options.length && <p role="status">{copy.empty}</p>}
      {!!options.length && (
        <label>
          {copy.label}
          <select
            value={value}
            onChange={(event) => onChange(event.target.value)}
            required
            disabled={busy}
          >
            <option value="">{copy.choose}</option>
            {options.map((option) => (
              <option key={option.supervisorId} value={option.supervisorId}>
                {option.firstName} {option.lastName} · {option.availableCapacity} {copy.slots}
              </option>
            ))}
          </select>
        </label>
      )}
      {result && "data" in result && result.data.totalPages > 1 && (
        <div className="academic-inline-actions">
          <button
            type="button"
            disabled={busy || request.page <= 1}
            onClick={() => load(request.search, request.page - 1)}
          >
            {copy.previous}
          </button>
          <span>
            {request.page} / {result.data.totalPages}
          </span>
          <button
            type="button"
            disabled={busy || request.page >= result.data.totalPages}
            onClick={() => load(request.search, request.page + 1)}
          >
            {copy.next}
          </button>
        </div>
      )}
    </div>
  );
}

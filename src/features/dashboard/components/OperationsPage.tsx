import { assignmentsContent } from "@/features/assignments/content/assignments.content";
import { connected } from "@/lib/api/api-client";
import { StudentPreviewPage } from "@/features/students/pages/StudentPreviewPage";
import "./operations.css";
import { useEffect, useRef, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import {
  ArrowLeft,
  CheckCircle2,
  ChevronLeft,
  ChevronRight,
  RefreshCw,
  Search,
} from "lucide-react";
import {
  operationPages,
  operations as copy,
  type ProposalStatus,
} from "../content/operations.content";
import { dashboardContent } from "../content/dashboard.content";
import { loadList } from "../hooks/operations";
import type { Change, Entry, Listing, OperationDomain } from "../types/operations.types";
import { OperationsDetail } from "./OperationsDetail";
import { TableSkeleton } from "@/components/feedback/Skeleton";
import { WorkspaceMetrics } from "@/components/data-table/WorkspaceMetrics";

const t = copy.text;
type Result = { data: Listing } | { error: string } | null;
type BooleanFilter = "" | "true" | "false";

function badgeTone(value: string): "success" | "danger" | "pending" | null {
  if (value === t.active || value === copy.proposalStatuses.Approved) return "success";
  if (value === t.inactive || value === copy.proposalStatuses.Rejected) return "danger";
  if (value === copy.proposalStatuses.PendingReview) return "pending";
  return null;
}

export function OperationsPage({ domain }: { domain: OperationDomain }) {
  const [params] = useSearchParams();
  if (!connected && domain === "students" && params.has("preview-tools"))
    return <StudentPreviewPage />;
  return <OperationsDirectory domain={domain} />;
}

function OperationsDirectory({ domain }: { domain: OperationDomain }) {
  const [department, setDepartment] = useState("");
  const [year, setYear] = useState("");
  const [academicFilters, setAcademicFilters] = useState({ department: "", academicYear: "" });
  const [row, select] = useState<Entry | null>(null);
  const [page, setPage] = useState(1);
  const [proposalStatus, setProposalStatus] = useState<ProposalStatus>("PendingReview");
  const [isActiveFilter, setIsActiveFilter] = useState<BooleanFilter>("");
  const [hasCapacityFilter, setHasCapacityFilter] = useState<BooleanFilter>("");
  const [search, setSearch] = useState("");
  const [query, setQuery] = useState("");
  const [revision, reload] = useState(0);
  const [result, setResult] = useState<Result>(null);
  const [completed, setCompleted] = useState<Change | null>(null);
  const focusOnReturn = useRef(false);
  const title = operationPages[domain].title;

  useEffect(() => {
    if (row) return;
    const abort = new AbortController();
    // [BACKEND-API] Read the selected domain's existing GET list. See operations.ts for query and list DTOs.
    loadList(domain, query, page, abort.signal, {
      status: proposalStatus,
      ...academicFilters,
      isActive: isActiveFilter === "" ? undefined : isActiveFilter === "true",
      hasCapacity: hasCapacityFilter === "" ? undefined : hasCapacityFilter === "true",
    }).then(
      (data) => {
        if (!abort.signal.aborted) setResult({ data });
      },
      (error: unknown) => {
        if (!abort.signal.aborted)
          setResult({ error: error instanceof Error ? error.message : t.error });
      },
    );
    return () => abort.abort();
  }, [
    domain,
    query,
    page,
    revision,
    proposalStatus,
    isActiveFilter,
    hasCapacityFilter,
    academicFilters,
    row,
  ]);

  useEffect(() => {
    document.title = (row?.title ?? title) + " | CAPNEXA";
  }, [row, title]);

  function refresh() {
    setResult(null);
    reload((value) => value + 1);
  }

  function goToPage(next: number) {
    setResult(null);
    setPage(next);
  }

  function clearFilters() {
    setCompleted(null);
    setDepartment("");
    setYear("");
    setAcademicFilters({ department: "", academicYear: "" });
    setSearch("");
    setQuery("");
    setIsActiveFilter("");
    setHasCapacityFilter("");
    setProposalStatus("PendingReview");
    setPage(1);
    refresh();
  }

  const count = result && "data" in result ? result.data.totalCount : null;
  const hasActiveFilter =
    Boolean(query || academicFilters.department || academicFilters.academicYear) ||
    isActiveFilter !== "" ||
    hasCapacityFilter !== "" ||
    (domain === "proposals" && proposalStatus !== "PendingReview");

  if (row)
    return (
      <OperationsDetail
        domain={domain}
        row={row}
        onClose={(change) => {
          setCompleted(change ?? null);
          focusOnReturn.current = true;
          select(null);
          if (change) setPage(1);
          refresh();
        }}
      />
    );

  return (
    <section className="academic-workspace connected-workspace">
      <header className="academic-heading">
        <Link to="/admin/dashboard">
          <ArrowLeft size={16} aria-hidden="true" />
          {dashboardContent.title}
        </Link>
        <h1
          tabIndex={-1}
          ref={(heading) => {
            if (heading && focusOnReturn.current) {
              heading.focus();
              focusOnReturn.current = false;
            }
          }}
        >
          {title}
        </h1>
        <p>{operationPages[domain].description}</p>
        <small className="academic-status" data-connected={connected}>
          {connected ? t.live : t.preview}
        </small>
        {!connected && (
          <nav className="operation-preview-links">
            {domain === "students" && <Link to="?preview-tools">{t.previewTools}</Link>}
            {domain === "supervisors" && (
              <Link to="/admin/supervisors/olivia-bennett">{t.supervisorDemo}</Link>
            )}
            {domain === "proposals" && <Link to="/admin/proposals/review">{t.proposalDemo}</Link>}
            {domain === "assignments" &&
              assignmentsContent.links.map((link) => (
                <Link to={link.route} key={link.route}>
                  {link.label}
                </Link>
              ))}
          </nav>
        )}
      </header>
      {completed && (
        <div className="operation-success" role="status">
          <CheckCircle2 size={18} aria-hidden="true" />
          <span>
            {copy.success[completed]}
            {!connected && ` ${t.preview}`}
          </span>
          {completed === "approve" && (
            <Link to="/admin/assignments">{operationPages.assignments.title}</Link>
          )}
        </div>
      )}
      {count !== null && <WorkspaceMetrics items={[{ label: t.total, value: count }]} />}
      <div className="academic-records academic-result">
        <form
          className="academic-tools operation-tools"
          onSubmit={(event) => {
            event.preventDefault();
            setCompleted(null);
            setQuery(search.trim());
            setAcademicFilters({ department: department.trim(), academicYear: year.trim() });
            setPage(1);
            refresh();
          }}
        >
          <div className="operation-filter">
            <Search size={17} aria-hidden="true" />
            <input
              type="search"
              aria-label={t.search}
              placeholder={t.search}
              value={search}
              onChange={(event) => setSearch(event.target.value)}
            />
            <button type="submit">{t.search}</button>
          </div>
          {(domain === "students" || domain === "supervisors") && (
            <label>
              <span>{t.accountFilter}</span>
              <select
                value={isActiveFilter}
                aria-label={t.accountFilter}
                onChange={(event) => {
                  const next = event.target.value;
                  if (next === "" || next === "true" || next === "false") {
                    setCompleted(null);
                    setIsActiveFilter(next);
                    goToPage(1);
                  }
                }}
              >
                <option value="">{t.all}</option>
                <option value="true">{t.active}</option>
                <option value="false">{t.inactive}</option>
              </select>
            </label>
          )}
          {(domain === "students" || domain === "supervisors") && (
            <label>
              <span>{t.department}</span>
              <input value={department} onChange={(event) => setDepartment(event.target.value)} />
            </label>
          )}
          {domain === "students" && (
            <label>
              <span>{t.year}</span>
              <input value={year} onChange={(event) => setYear(event.target.value)} />
            </label>
          )}
          {domain === "supervisors" && (
            <label>
              <span>{t.capacityFilter}</span>
              <select
                value={hasCapacityFilter}
                aria-label={t.capacityFilter}
                onChange={(event) => {
                  const next = event.target.value;
                  if (next === "" || next === "true" || next === "false") {
                    setCompleted(null);
                    setHasCapacityFilter(next);
                    goToPage(1);
                  }
                }}
              >
                <option value="">{t.all}</option>
                <option value="true">{t.availableSlots}</option>
                <option value="false">{t.atCapacity}</option>
              </select>
            </label>
          )}
          {domain === "proposals" && (
            <label>
              <span>{t.status}</span>
              <select
                value={proposalStatus}
                aria-label={t.status}
                onChange={(event) => {
                  const status = event.target.value;
                  if (
                    status === "PendingReview" ||
                    status === "Approved" ||
                    status === "Rejected"
                  ) {
                    setCompleted(null);
                    setProposalStatus(status);
                    goToPage(1);
                  }
                }}
              >
                <option value="PendingReview">{copy.proposalStatuses.PendingReview}</option>
                <option value="Approved">{copy.proposalStatuses.Approved}</option>
                <option value="Rejected">{copy.proposalStatuses.Rejected}</option>
              </select>
            </label>
          )}
          {hasActiveFilter && (
            <button type="button" className="operation-clear" onClick={clearFilters}>
              {t.clearFilters}
            </button>
          )}
          <button type="button" onClick={refresh} disabled={!result} className="operation-refresh">
            <RefreshCw size={15} aria-hidden="true" />
            {t.refresh}
          </button>
        </form>

        {!result ? (
          <div className="academic-empty is-loading" role="status">
            {t.loading}
            <TableSkeleton />
          </div>
        ) : "error" in result ? (
          <div className="academic-empty">
            <p role="alert">{result.error}</p>
            <button type="button" onClick={refresh}>
              {t.retry}
            </button>
          </div>
        ) : (
          <>
            {result.data.items.length === 0 ? (
              <div className="academic-empty" role="status">
                <p>{t.empty}</p>
              </div>
            ) : (
              <div
                className="academic-scroll operation-table"
                role="region"
                aria-label={title}
                tabIndex={0}
              >
                <table>
                  <caption className="academic-sr-only">{title}</caption>
                  <thead>
                    <tr>
                      {operationPages[domain].columns.map((label) => (
                        <th key={label} scope="col">
                          {label}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {result.data.items.map((item) => (
                      <tr key={item.id}>
                        <th scope="row">
                          <button type="button" onClick={() => select(item)}>
                            {item.title}
                          </button>
                          {item.secondary && (
                            <small className="operation-secondary">{item.secondary}</small>
                          )}
                        </th>
                        {item.cells.map((cell, index) => {
                          const tone = badgeTone(cell);
                          return (
                            <td key={operationPages[domain].columns[index + 1]}>
                              {tone ? (
                                <span className={`operation-badge is-${tone}`}>{cell}</span>
                              ) : (
                                cell
                              )}
                            </td>
                          );
                        })}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
            {(page > 1 || result.data.totalPages > 1) && (
              <nav className="academic-tools operation-pagination" aria-label={t.recordsNavigation}>
                <button type="button" disabled={page <= 1} onClick={() => goToPage(page - 1)}>
                  <ChevronLeft size={16} aria-hidden="true" />
                  {t.previous}
                </button>
                <span>
                  {t.page} {page} / {Math.max(page, result.data.totalPages)}
                </span>
                <button
                  type="button"
                  disabled={page >= result.data.totalPages}
                  onClick={() => goToPage(page + 1)}
                >
                  {t.next}
                  <ChevronRight size={16} aria-hidden="true" />
                </button>
              </nav>
            )}
          </>
        )}
      </div>
    </section>
  );
}

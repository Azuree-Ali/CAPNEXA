import { Link } from "react-router-dom";
import { useId, type CSSProperties, type ReactNode } from "react";
import {
  dashboardContent as copy,
  type DashboardProposal,
  type DashboardSupervisor,
} from "../content/dashboard.content";

const dateFormatter = new Intl.DateTimeFormat("en", { dateStyle: "medium", timeZone: "UTC" });
export function DashboardDate({ value }: { value: string | null }) {
  const date = value ? new Date(value) : null;
  if (!date || Number.isNaN(date.getTime())) return copy.missingField;
  return <time dateTime={date.toISOString()}>{dateFormatter.format(date)}</time>;
}

export function DashboardPanel({
  title,
  description,
  link,
  children,
}: {
  title: string;
  description: string;
  link?: { route: string; label: string };
  children: ReactNode;
}) {
  const id = useId();
  return (
    <section className="dashboard-panel" aria-labelledby={id}>
      <header className="dashboard-panel-heading">
        <div>
          <h2 id={id}>{title}</h2>
          <p>{description}</p>
        </div>
        {link && (
          <Link to={link.route}>
            {link.label}
            <copy.icons.external size={14} aria-hidden="true" />
          </Link>
        )}
      </header>
      {children}
    </section>
  );
}

export function LifecycleChart({ total }: { total: number }) {
  return (
    <>
      <div className="dashboard-distribution" aria-hidden="true">
        {copy.demo.stages.map((stage, index) => (
          <span
            key={stage.label}
            data-series={index}
            style={{ flexGrow: stage.count }}
            hidden={!stage.count}
          />
        ))}
      </div>
      <ul className="dashboard-stage-list">
        {copy.demo.stages.map((stage, index) => (
          <li key={stage.label}>
            <span data-series={index} className="dashboard-dot" aria-hidden="true" />
            <span>{stage.label}</span>
            <strong>{stage.count}</strong>
            <span>{Math.round((stage.count * 100) / (total || 1))}%</span>
          </li>
        ))}
      </ul>
    </>
  );
}

export function HealthChart() {
  const [onTrack, attention, overdue] = copy.demo.health;
  const total = onTrack + attention + overdue;
  const size = total || 1;
  const style = {
    "--on-track-end": `${(onTrack / size) * 100}%`,
    "--attention-end": `${((onTrack + attention) / size) * 100}%`,
  } as CSSProperties;
  return (
    <>
      <div className="dashboard-donut" style={style} data-empty={total === 0}>
        <div>
          <strong>{total}</strong>
          <span>{copy.health.total}</span>
        </div>
      </div>
      <ul className="dashboard-health-list">
        {copy.health.labels.map((label, index) => (
          <li key={label}>
            <span className="dashboard-dot" data-health={index} aria-hidden="true" />
            {label}
            <strong>{copy.demo.health[index]}</strong>
          </li>
        ))}
      </ul>
    </>
  );
}

export function ApprovalsTable({ rows }: { rows: readonly DashboardProposal[] }) {
  const captionId = useId();
  if (!rows.length)
    return (
      <p className="dashboard-panel-message" role="status">
        {copy.approvals.empty}
      </p>
    );
  return (
    <div className="dashboard-table-scroll" tabIndex={0} role="region" aria-labelledby={captionId}>
      <table>
        <caption className="academic-sr-only" id={captionId}>
          {copy.approvals.title}
        </caption>
        <thead>
          <tr>
            {copy.approvals.columns.map((label) => (
              <th key={label} scope="col">
                {label}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={row.id}>
              <th scope="row">
                {row.title}
                <small>{row.id}</small>
              </th>
              <td>
                {row.team}
                <small>{row.leader ?? copy.missingField}</small>
              </td>
              <td>{row.department ?? copy.missingField}</td>
              <td>
                <DashboardDate value={row.submitted} />
              </td>
              <td>
                <Link to={row.route}>
                  {copy.approvals.review}
                  <span className="academic-sr-only">{` · ${row.title}`}</span>
                </Link>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function AllocationList({ rows }: { rows: readonly DashboardSupervisor[] }) {
  if (!rows.length)
    return (
      <p className="dashboard-panel-message" role="status">
        {copy.allocation.empty}
      </p>
    );
  return (
    <ul className="dashboard-allocation-list">
      {rows.map((row) => (
        <li key={row.id}>
          <div className="dashboard-allocation-row">
            <div>
              <strong>{row.name}</strong>
              <small>{row.area ?? copy.missingField}</small>
            </div>
            <div>
              <strong>
                {row.assigned} / {row.capacity}
              </strong>
              <small>
                {row.available} {copy.allocation.available}
              </small>
            </div>
          </div>
          <progress
            max={Math.max(row.capacity, 1)}
            value={row.assigned}
            aria-label={`${copy.allocation.assigned} · ${row.name}`}
          />
        </li>
      ))}
    </ul>
  );
}

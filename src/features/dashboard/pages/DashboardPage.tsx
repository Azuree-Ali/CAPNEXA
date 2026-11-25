import { useEffect } from "react";
import { Link } from "react-router-dom";
import { connected } from "@/lib/api/api-client";
import { CountUp } from "@/lib/motion/CountUp";
import { useReveal } from "@/lib/motion/useReveal";
import { dashboardContent as copy } from "../content/dashboard.content";
import {
  AllocationList,
  ApprovalsTable,
  DashboardDate,
  DashboardPanel,
  HealthChart,
  LifecycleChart,
} from "../components/DashboardPanels";

export function DashboardPage() {
  const scope = useReveal<HTMLDivElement>(".dashboard-heading h1, .dashboard-metrics > li");
  useEffect(() => {
    document.title = copy.title + " | CAPNEXA";
  }, []);
  const total = copy.demo.stages.reduce((sum, stage) => sum + stage.count, 0);
  const values = {
    total,
    active: copy.demo.active,
    pending: copy.demo.proposals.length,
    overdue: copy.demo.deadlines.filter((deadline) => deadline.overdue).length,
    supervisors: copy.demo.supervisors.length,
  };
  const unavailable = <p className="dashboard-panel-message">{copy.unavailable}</p>;

  function exportOverview() {
    const rows = [
      [copy.metricColumn, copy.valueColumn],
      ...copy.metrics.map(({ id, label }) => [label, String(values[id])]),
    ];
    const csv = rows
      .map((row) => row.map((cell) => '"' + cell.replaceAll('"', '""') + '"').join(","))
      .join("\r\n");
    const url = URL.createObjectURL(new Blob(["\uFEFF", csv], { type: "text/csv;charset=utf-8" }));
    const link = document.createElement("a");
    link.href = url;
    link.download = copy.exportFilename;
    link.click();
    window.setTimeout(() => URL.revokeObjectURL(url), 1000);
  }

  return (
    <div className="admin-dashboard" ref={scope}>
      <header className="dashboard-heading">
        <div>
          <div className="dashboard-context">
            <span>{connected ? copy.liveContext : copy.cohort}</span>
            <span className="dashboard-mode" data-connected={connected}>
              {connected ? copy.live : copy.preview}
            </span>
          </div>
          <h1>{copy.title}</h1>
          <p>{copy.description}</p>
        </div>
        {!connected && (
          <button type="button" className="dashboard-primary" onClick={exportOverview}>
            <copy.icons.export size={17} aria-hidden="true" />
            {copy.export}
          </button>
        )}
      </header>

      <ul className="dashboard-metrics">
        {copy.metrics.map(({ id, label, detail, route, icon: Icon }) => (
          <li key={id}>
            <Link to={route} data-metric={id}>
              <span className="dashboard-metric-top">
                <span className="dashboard-metric-icon">
                  <Icon size={19} aria-hidden="true" />
                </span>
                <copy.icons.external size={15} aria-hidden="true" />
              </span>
              <span>{label}</span>
              <strong className={connected ? "dashboard-metric-unavailable" : undefined}>
                {connected ? copy.missingField : <CountUp value={values[id]} />}
              </strong>
              <small>{detail}</small>
            </Link>
          </li>
        ))}
      </ul>

      <div className="dashboard-grid">
        <div className="dashboard-primary-column">
          <DashboardPanel {...copy.lifecycle}>
            {connected ? unavailable : <LifecycleChart total={total} />}
          </DashboardPanel>

          <DashboardPanel
            {...copy.approvals}
            link={{ route: copy.approvals.route, label: copy.approvals.link }}
          >
            {connected ? unavailable : <ApprovalsTable rows={copy.demo.proposals} />}
          </DashboardPanel>

          <DashboardPanel
            {...copy.allocation}
            link={{ route: copy.allocation.route, label: copy.allocation.link }}
          >
            {connected ? unavailable : <AllocationList rows={copy.demo.supervisors} />}
          </DashboardPanel>
          <nav className="dashboard-shortcuts" aria-label={copy.quickLinksLabel}>
            {copy.quickLinks.map(({ label, route }) => (
              <Link key={route} to={route}>
                {label}
                <copy.icons.external size={14} aria-hidden="true" />
              </Link>
            ))}
          </nav>
        </div>

        <div className="dashboard-secondary-column">
          <DashboardPanel {...copy.health}>
            {connected ? unavailable : <HealthChart />}
          </DashboardPanel>

          <DashboardPanel
            {...copy.deadlines}
            link={
              connected ? undefined : { route: copy.deadlines.route, label: copy.deadlines.link }
            }
          >
            {connected ? (
              unavailable
            ) : (
              <ul className="dashboard-event-list">
                {copy.demo.deadlines.map((item) => (
                  <li key={item.label}>
                    <span className="dashboard-event-icon">
                      <copy.icons.calendar size={16} aria-hidden="true" />
                    </span>
                    <div>
                      <strong>{item.label}</strong>
                      <small>
                        {item.project} · <DashboardDate value={item.date} />
                      </small>
                      <span className="dashboard-status">{item.status}</span>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </DashboardPanel>

          <DashboardPanel
            title={copy.activity.title}
            description={connected ? copy.liveActivity : copy.activity.description}
            link={connected ? undefined : { route: copy.activity.route, label: copy.activity.link }}
          >
            {connected ? (
              unavailable
            ) : (
              <ul className="dashboard-event-list">
                {copy.demo.activity.map((item) => (
                  <li key={item.label}>
                    <span className="dashboard-event-icon">
                      <copy.icons.activity size={16} aria-hidden="true" />
                    </span>
                    <div>
                      <strong>{item.label}</strong>
                      <small>
                        <DashboardDate value={item.date} />
                      </small>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </DashboardPanel>
        </div>
      </div>
    </div>
  );
}

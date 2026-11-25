import { useState } from "react";
import { Link } from "react-router-dom";
import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { WorkspaceMetrics } from "@/components/data-table/WorkspaceMetrics";
import { workspaceCopy } from "@/components/data-table/workspace.content";
import { reportsContent as copy } from "../content/reports.content";

const allRecords = copy.templates.flatMap((template) => template.records);
const departments = [...new Set(allRecords.map((record) => record.department))];
const years = [...new Set(allRecords.map((record) => record.academicYear))];

function csvCell(value: string) {
  const safe = /^(?:\s*[=+@-]|[\t\r\n])/.test(value) ? "'" + value : value;
  return '"' + safe.replaceAll('"', '""') + '"';
}
export function ReportsPage() {
  const [scope, setScope] = useState("");
  const [department, setDepartment] = useState("");
  const [year, setYear] = useState("");
  const rows = copy.templates
    .filter((template) => !scope || template.id === scope)
    .flatMap((template) => template.records)
    .filter(
      (record) =>
        (!department || record.department === department) &&
        (!year || record.academicYear === year),
    );
  function reset() {
    setScope("");
    setDepartment("");
    setYear("");
  }
  function download() {
    const values = rows.map((record) => [
      record.name,
      record.department,
      record.academicYear,
      record.metric,
    ]);
    const csv = [copy.columns, ...values].map((row) => row.map(csvCell).join(",")).join("\r\n");
    const url = URL.createObjectURL(new Blob(["\uFEFF", csv], { type: "text/csv;charset=utf-8" }));
    const link = document.createElement("a");
    link.href = url;
    link.download = copy.filename;
    link.click();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  }
  return (
    <section className="academic-workspace">
      <WorkspaceHeading title={copy.title} description={copy.description} />
      <WorkspaceMetrics
        items={[
          { label: copy.templatesCount, value: copy.templates.length },
          { label: copy.count, value: rows.length },
        ]}
      />
      <section className="academic-records academic-panel" aria-labelledby="report-builder">
        <h2 id="report-builder">{copy.builder}</h2>
        <div className="academic-tools academic-filter-fields">
          <label>
            {copy.scope}
            <select value={scope} onChange={(event) => setScope(event.target.value)}>
              <option value="">{copy.all}</option>
              {copy.templates.map((template) => (
                <option key={template.id} value={template.id}>
                  {template.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            {copy.department}
            <select value={department} onChange={(event) => setDepartment(event.target.value)}>
              <option value="">{copy.all}</option>
              {departments.map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>
          <label>
            {copy.year}
            <select value={year} onChange={(event) => setYear(event.target.value)}>
              <option value="">{copy.all}</option>
              {years.map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>
        </div>
        <div className="academic-inline-actions">
          <button onClick={download} disabled={!rows.length}>
            {copy.download}
          </button>
          <button onClick={reset}>{copy.clear}</button>
        </div>
        <small>{copy.notice}</small>
      </section>
      <section className="academic-records academic-result" aria-labelledby="report-results">
        <div className="academic-tools">
          <h2 id="report-results">{copy.results}</h2>
          <span role="status">
            {rows.length} · {copy.count}
          </span>
        </div>
        {rows.length ? (
          <div className="academic-scroll">
            <table>
              <thead>
                <tr>
                  {copy.columns.map((column) => (
                    <th key={column} scope="col">
                      {column}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {rows.map((record) => (
                  <tr key={record.id}>
                    <th scope="row">
                      <Link to={record.source}>{record.name}</Link>
                    </th>
                    <td>{record.department}</td>
                    <td>{record.academicYear}</td>
                    <td>{record.metric}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <p className="academic-panel" role="status">
            {workspaceCopy.empty}
          </p>
        )}
      </section>
    </section>
  );
}

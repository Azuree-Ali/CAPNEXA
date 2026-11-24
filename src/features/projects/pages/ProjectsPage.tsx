import { useState } from "react";
import { Link } from "react-router-dom";
import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { workspaceCopy } from "@/components/data-table/workspace.content";
import { projectsContent as copy } from "../content/projects.content";

export function ProjectsPage() {
  const [search, setSearch] = useState("");
  const [stage, setStage] = useState("");
  const stages = [...new Set(copy.records.map((record) => record.stage))];
  const records = copy.records.filter(
    (record) =>
      (!stage || record.stage === stage) &&
      [record.title, record.id, record.team]
        .join(" ")
        .toLowerCase()
        .includes(search.trim().toLowerCase()),
  );
  return (
    <section className="academic-workspace">
      <WorkspaceHeading title={copy.title} description={copy.description} />
      <div className="academic-records">
        <div className="academic-tools">
          <label>
            {copy.search}
            <input
              type="search"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
            />
          </label>
          <label>
            {copy.stage}
            <select value={stage} onChange={(event) => setStage(event.target.value)}>
              <option value="">{copy.allStages}</option>
              {stages.map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>
        </div>
        {records.length ? (
          <div className="academic-scroll">
            <table>
              <thead>
                <tr>
                  {copy.columns.map((label) => (
                    <th key={label} scope="col">
                      {label}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {records.map((record) => (
                  <tr key={record.id}>
                    <th scope="row">
                      {record.route ? <Link to={record.route}>{record.title}</Link> : record.title}
                      <small className="academic-secondary">{record.id}</small>
                    </th>
                    <td>{record.team}</td>
                    <td>{record.stage}</td>
                    <td>
                      <progress
                        aria-label={record.title + " " + copy.columns[3]}
                        max={100}
                        value={record.progress}
                      />{" "}
                      <span>{record.progress}%</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="academic-empty">
            <p role="status">{workspaceCopy.empty}</p>
            <button
              type="button"
              onClick={() => {
                setStage("");
                setSearch("");
              }}
            >
              {workspaceCopy.clear}
            </button>
          </div>
        )}
      </div>
    </section>
  );
}

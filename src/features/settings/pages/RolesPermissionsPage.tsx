import { useState } from "react";
import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { workspaceCopy } from "@/components/data-table/workspace.content";
import { settingsPermissionsContent as copy } from "../content/settingsPermissions.content";

export function RolesPermissionsPage() {
  const [area, setArea] = useState("");
  const [search, setSearch] = useState("");
  const areas = [...new Set(copy.entries.map((entry) => entry.area))];
  const entries = copy.entries.filter(
    (entry) =>
      (!area || area === entry.area) &&
      entry.capability.toLowerCase().includes(search.trim().toLowerCase()),
  );
  return (
    <section className="academic-workspace">
      <WorkspaceHeading title={copy.title} description={copy.description} parent={copy.parent} />
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
            {copy.area}
            <select value={area} onChange={(event) => setArea(event.target.value)}>
              <option value="">{copy.all}</option>
              {areas.map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>
        </div>
        {entries.length ? (
          <div className="academic-scroll">
            <table>
              <thead>
                <tr>
                  <th scope="col">{copy.capability}</th>
                  {copy.roles.map((role) => (
                    <th key={role} scope="col">
                      {role}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {entries.map((entry) => (
                  <tr key={entry.capability}>
                    <th scope="row">
                      {entry.capability}
                      <small className="academic-secondary">{entry.area}</small>
                    </th>
                    {entry.grants.map((grant, index) => (
                      <td key={copy.roles[index]}>
                        <span
                          className={
                            grant === "restricted" ? "academic-secondary" : "academic-status"
                          }
                        >
                          {copy.legend[grant]}
                        </span>
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="academic-empty">
            <p role="status">{workspaceCopy.empty}</p>
            <button
              onClick={() => {
                setArea("");
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

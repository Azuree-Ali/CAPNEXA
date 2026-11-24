import { useRef, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { settingsContent as copy, type AcademicSection } from "../content/settings.content";

const records = new Map(copy.sections.map((section) => [section.id, [...section.rows]]));
type Row = AcademicSection["rows"][number];

export function AcademicConfigurationPage() {
  const [params] = useSearchParams();
  const section =
    copy.sections.find((item) => item.id === params.get("section")) ?? copy.sections[0];
  return (
    <section className="academic-workspace">
      <WorkspaceHeading title={copy.title} description={copy.description}>
        <div>
          <Link to={copy.permissions.route}>{copy.permissions.label}</Link>
        </div>
      </WorkspaceHeading>
      <nav className="academic-views" aria-label={copy.navigation}>
        {copy.sections.map((item) => (
          <Link
            key={item.id}
            to={`?section=${item.id}`}
            aria-current={item.id === section.id ? "page" : undefined}
          >
            {item.title}
          </Link>
        ))}
      </nav>
      <ConfigurationRegister key={section.id} section={section} />
    </section>
  );
}
function ConfigurationRegister({ section }: { section: AcademicSection }) {
  const [rows, setRows] = useState(() => records.get(section.id) ?? []);
  const [selected, setSelected] = useState<Row>();
  const [values, setValues] = useState<string[]>([]);
  const [error, setError] = useState(false);
  const [saved, setSaved] = useState(false);
  const dialog = useRef<HTMLDialogElement>(null);
  function open(row?: Row) {
    setSaved(false);
    setError(false);
    setSelected(row);
    setValues(row ? [...row.values] : section.fields.map(() => ""));
    dialog.current?.showModal();
  }
  function save() {
    const data = values.map((value) => value.trim());
    const start = section.fields.findIndex((field) => field.id === "start");
    const end = section.fields.findIndex((field) => field.id === "end");
    if (
      data.some((value) => !value) ||
      rows.some(
        (row) => row.id !== selected?.id && row.values[0].toLowerCase() === data[0].toLowerCase(),
      ) ||
      (start >= 0 && end >= 0 && data[end] <= data[start])
    ) {
      setError(true);
      return;
    }
    const entry = { id: selected?.id ?? crypto.randomUUID(), values: data };
    const next = selected
      ? rows.map((row) => (row.id === selected.id ? entry : row))
      : [...rows, entry];
    records.set(section.id, next);
    setRows(next);
    dialog.current?.close();
    setSaved(true);
  }
  return (
    <div className="academic-records">
      <div className="academic-tools">
        <div>
          <h2>{section.title}</h2>
          <p>{section.description}</p>
        </div>
        <button onClick={() => open()}>{copy.add}</button>
      </div>
      {rows.length ? (
        <div className="academic-scroll">
          <table>
            <thead>
              <tr>
                {section.fields.map((field) => (
                  <th key={field.id} scope="col">
                    {field.label}
                  </th>
                ))}
                <th scope="col">{copy.edit}</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((row) => (
                <tr key={row.id}>
                  {row.values.map((value, index) =>
                    index === 0 ? (
                      <th key={section.fields[index].id} scope="row">
                        {value}
                      </th>
                    ) : (
                      <td key={section.fields[index].id}>{value}</td>
                    ),
                  )}
                  <td>
                    <button onClick={() => open(row)} aria-label={copy.edit + " " + row.values[0]}>
                      {copy.edit}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <p className="academic-panel" role="status">
          {copy.empty}
        </p>
      )}
      {saved && (
        <p className="academic-panel" role="status">
          {copy.saved}
        </p>
      )}
      <dialog className="info-dialog" ref={dialog} aria-labelledby="configuration-dialog-title">
        <form
          onSubmit={(event) => {
            event.preventDefault();
            save();
          }}
        >
          <h2 id="configuration-dialog-title">
            {selected ? copy.edit : copy.add} · {section.title}
          </h2>
          <p>{copy.notice}</p>
          {section.fields.map((field, index) => (
            <label key={field.id}>
              {field.label}
              <input
                autoFocus={index === 0}
                type={field.type}
                required
                maxLength={200}
                min={field.min}
                max={field.max}
                value={values[index] ?? ""}
                onChange={(event) => {
                  const value = event.target.value;
                  setValues((previous) =>
                    previous.map((item, itemIndex) => (itemIndex === index ? value : item)),
                  );
                  setError(false);
                }}
              />
            </label>
          ))}
          {error && <p role="alert">{copy.invalid}</p>}
          <div className="academic-inline-actions">
            <button type="button" onClick={() => dialog.current?.close()}>
              {copy.cancel}
            </button>
            <button type="submit">{copy.save}</button>
          </div>
        </form>
      </dialog>
    </div>
  );
}

export interface WorkspaceMetric {
  label: string;
  value: string | number;
}
export function WorkspaceMetrics({ items }: { items: readonly WorkspaceMetric[] }) {
  return (
    <dl className="academic-metrics">
      {items.map(({ label, value }) => (
        <div key={label}>
          <dt>{label}</dt>
          <dd>{value}</dd>
        </div>
      ))}
    </dl>
  );
}

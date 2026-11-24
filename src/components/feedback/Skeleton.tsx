import type { CSSProperties } from "react";

/** Decorative placeholder shown while data loads. The real status text stays outside it. */
export function TableSkeleton({ rows = 5, columns = 4 }: { rows?: number; columns?: number }) {
  return (
    <div className="skeleton-table" aria-hidden="true">
      {Array.from({ length: rows }, (_, row) => (
        <div className="skeleton-row" key={row} style={{ "--row": row } as CSSProperties}>
          {Array.from({ length: columns }, (_, col) => (
            <span
              className="skeleton"
              key={col}
              style={{ inlineSize: col === 0 ? "88%" : `${52 + ((row * 7 + col * 13) % 36)}%` }}
            />
          ))}
        </div>
      ))}
    </div>
  );
}

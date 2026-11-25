import { Link } from "react-router-dom";
import { workspaceCopy } from "@/components/data-table/workspace.content";
import { useEffect, type ReactNode } from "react";

interface Props {
  title: string;
  description: string;
  parent?: { label: string; route: string };
  children?: ReactNode;
}

export function WorkspaceHeading({ title, description, parent, children }: Props) {
  useEffect(() => {
    document.title = title + " | CAPNEXA";
  }, [title]);
  return (
    <header className="academic-heading">
      {parent && <Link to={parent.route}>{parent.label}</Link>}
      <h1>{title}</h1>
      <p>{description}</p>
      <small>{workspaceCopy.preview}</small>
      {children}
    </header>
  );
}

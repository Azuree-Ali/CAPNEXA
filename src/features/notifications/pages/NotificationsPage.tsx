import { useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { notificationsContent as copy } from "../content/notifications.content";

const readEvents = new Set<string>();

export function NotificationsPage() {
  const [params] = useSearchParams();
  const [read, setRead] = useState(() => new Set(readEvents));
  const unreadOnly = params.get("view") === "unread";
  const events = unreadOnly ? copy.events.filter((event) => !read.has(event.id)) : copy.events;
  function toggle(id: string) {
    if (readEvents.has(id)) readEvents.delete(id);
    else readEvents.add(id);
    setRead(new Set(readEvents));
  }
  return (
    <section className="academic-workspace">
      <WorkspaceHeading title={copy.title} description={copy.description} />
      <nav className="academic-views" aria-label={copy.navigation}>
        <Link to="." aria-current={!unreadOnly ? "page" : undefined}>
          {copy.all}
        </Link>
        <Link to="?view=unread" aria-current={unreadOnly ? "page" : undefined}>
          {copy.unread} ({copy.events.length - read.size})
        </Link>
      </nav>
      {events.length ? (
        <ul className="academic-timeline academic-plain-list">
          {events.map((event) => (
            <li key={event.id} className="academic-records academic-panel">
              <div className="academic-card-heading">
                <h2>{event.title}</h2>
                <span className="academic-status">
                  {read.has(event.id) ? copy.read : copy.unread}
                </span>
              </div>
              <p className="academic-secondary">
                {event.project} · {event.time}
              </p>
              <div className="academic-inline-actions">
                <Link to={event.route}>{event.action}</Link>
                <button onClick={() => toggle(event.id)}>
                  {read.has(event.id) ? copy.markUnread : copy.markRead}
                </button>
              </div>
            </li>
          ))}
        </ul>
      ) : (
        <p role="status" className="academic-records academic-panel">
          {copy.empty}
        </p>
      )}
    </section>
  );
}

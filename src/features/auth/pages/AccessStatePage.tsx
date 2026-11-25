import { Link } from "react-router-dom";
import { accessStates } from "../content/accessState.content";

export function AccessStatePage({ state }: { state: keyof typeof accessStates }) {
  const copy = accessStates[state];
  return (
    <section className="not-found">
      <h1>{copy.title}</h1>
      <p>{copy.description}</p>
      <Link className="access-state-action" to={copy.route}>
        {copy.action}
      </Link>
    </section>
  );
}

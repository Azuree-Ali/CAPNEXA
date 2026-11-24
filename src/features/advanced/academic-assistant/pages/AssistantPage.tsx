import { useState } from "react";
import { Link } from "react-router-dom";
import { WorkspaceHeading } from "@/components/layout/WorkspaceHeading";
import { assistantContent as copy } from "../content/assistant.content";

export function AssistantPage() {
  const [query, setQuery] = useState("");
  const [submitted, setSubmitted] = useState("");
  const normalized = submitted.toLowerCase();
  const exact = copy.entries.find((entry) => entry.question.toLowerCase() === normalized);
  const matches = exact
    ? [exact]
    : copy.entries.filter((entry) => entry.terms.some((term) => normalized.includes(term)));
  const match = matches.length === 1 ? matches[0] : undefined;
  return (
    <section className="academic-workspace">
      <WorkspaceHeading title={copy.title} description={copy.description} />
      <div className="academic-records academic-panel">
        <form
          className="academic-query"
          onSubmit={(event) => {
            event.preventDefault();
            setSubmitted(query.trim());
          }}
        >
          <label>
            {copy.question}
            <input
              type="search"
              required
              maxLength={240}
              value={query}
              onChange={(event) => setQuery(event.target.value)}
            />
          </label>
          <button disabled={!query.trim()}>{copy.ask}</button>
        </form>
        <h2>{copy.suggestions}</h2>
        <ul className="academic-list">
          {copy.entries.map((entry) => (
            <li key={entry.question}>
              <button
                onClick={() => {
                  setQuery(entry.question);
                  setSubmitted(entry.question);
                }}
              >
                {entry.question}
              </button>
            </li>
          ))}
        </ul>
      </div>
      {submitted && (
        <section className="academic-records academic-panel academic-result" aria-live="polite">
          {match ? (
            <>
              <h2>{copy.answerHeading}</h2>
              <p>{match.answer}</p>
              <Link to={match.source.route}>{match.source.label}</Link>
            </>
          ) : matches.length ? (
            <>
              <p>{copy.ambiguous}</p>
              <ul className="academic-list">
                {matches.map((entry) => (
                  <li key={entry.question}>
                    <button
                      onClick={() => {
                        setQuery(entry.question);
                        setSubmitted(entry.question);
                      }}
                    >
                      {entry.question}
                    </button>
                  </li>
                ))}
              </ul>
            </>
          ) : (
            <p>{copy.noAnswer}</p>
          )}
        </section>
      )}
    </section>
  );
}

import { profileCopy as copy } from "@/features/settings/content/profile.content";
import { useState } from "react";
import { X, Plus } from "lucide-react";
export function ResearchInterests({
  topics,
  disabled,
  onChange,
}: {
  topics: string[];
  disabled: boolean;
  onChange: (topics: string[]) => void;
}) {
  const [topic, setTopic] = useState("");
  const [error, setError] = useState("");
  function add() {
    const value = topic.trim().replace(/\s+/g, " ");
    if (!value) {
      setError("Enter a research topic.");
      return;
    }
    if (topics.some((t) => t.toLowerCase() === value.toLowerCase())) {
      setError("This topic is already listed.");
      return;
    }
    if (topics.length >= 20) {
      setError("You can add up to 20 topics.");
      return;
    }
    onChange([...topics, value]);
    setTopic("");
    setError("");
  }
  return (
    <section
      className="profile-card"
      id="research-interests"
      tabIndex={-1}
      aria-labelledby="research-title"
    >
      <div className="profile-card-heading">
        <div>
          <h2 id="research-title">{copy.researchInterests}</h2>
          <p>{copy.academicDomainTopicsUsedForAutomaticStudentProject}</p>
        </div>
      </div>
      <div className="profile-topics">
        {topics.length ? (
          topics.map((t) => (
            <span className="profile-topic" key={t}>
              {t}
              <button
                type="button"
                disabled={disabled}
                onClick={() => onChange(topics.filter((v) => v !== t))}
                aria-label={`Remove ${t}`}
              >
                <X size={13} />
              </button>
            </span>
          ))
        ) : (
          <p>{copy.noResearchInterestsAddedYet}</p>
        )}
      </div>
      <div className="profile-topic-add">
        <input
          aria-label={copy.newResearchTopic}
          maxLength={80}
          disabled={disabled}
          placeholder={copy.addANewResearchTopicEGComputer}
          value={topic}
          aria-invalid={!!error}
          aria-describedby={error ? "topic-error" : undefined}
          onChange={(e) => {
            setTopic(e.target.value);
            setError("");
          }}
          onKeyDown={(e) => {
            if (e.key === "Enter") {
              e.preventDefault();
              add();
            }
          }}
        />
        <button className="profile-button" type="button" disabled={disabled} onClick={add}>
          <Plus size={16} />
          {copy.addTopic}
        </button>
      </div>
      {error ? (
        <p className="profile-error" id="topic-error" role="alert">
          {error}
        </p>
      ) : null}
    </section>
  );
}

import { useRef } from "react";
import { UserRound, LockKeyhole, SlidersHorizontal } from "lucide-react";
import type { ProfileTab } from "../types/profile.types";
const tabs = [
  { id: "personal", label: "Personal Information", icon: UserRound },
  { id: "security", label: "Security", icon: LockKeyhole },
  { id: "preferences", label: "Preferences", icon: SlidersHorizontal },
] as const;
export function ProfileTabs({
  selected,
  onChange,
}: {
  selected: ProfileTab;
  onChange: (tab: ProfileTab) => void;
}) {
  const refs = useRef<(HTMLButtonElement | null)[]>([]);
  return (
    <div className="profile-tabs" role="tablist" aria-label="Profile settings">
      {tabs.map(({ id, label, icon: Icon }, index) => (
        <button
          key={id}
          ref={(el) => {
            refs.current[index] = el;
          }}
          type="button"
          role="tab"
          id={`tab-${id}`}
          aria-controls={`panel-${id}`}
          aria-selected={selected === id}
          tabIndex={selected === id ? 0 : -1}
          onClick={() => onChange(id)}
          onKeyDown={(event) => {
            const next =
              event.key === "ArrowRight"
                ? (index + 1) % tabs.length
                : event.key === "ArrowLeft"
                  ? (index + tabs.length - 1) % tabs.length
                  : event.key === "Home"
                    ? 0
                    : event.key === "End"
                      ? tabs.length - 1
                      : -1;
            if (next >= 0) {
              event.preventDefault();
              onChange(tabs[next].id);
              refs.current[next]?.focus();
            }
          }}
        >
          <Icon size={16} />
          {label}
        </button>
      ))}
    </div>
  );
}

import { connected, currentUser } from "@/lib/api/api-client";
import { profileCopy as copy } from "@/features/settings/content/profile.content";
import { useEffect, useRef, useState } from "react";
import { useLocation, useSearchParams } from "react-router-dom";
import { X } from "lucide-react";
import type { PortalRole } from "../../../config/navigation";
import type { Profile, ProfileEditable, ProfileTab } from "../types/profile.types";
import { useProfile } from "../hooks/useProfile";
import { ProfileHeader } from "../components/ProfileHeader";
import { ProfileTabs } from "../components/ProfileTabs";
import { InstitutionDetails } from "../components/InstitutionDetails";
import { RoleAccessCard } from "../components/RoleAccessCard";
import { AvailabilityForm } from "../components/AvailabilityForm";
import { ResearchInterests } from "../components/ResearchInterests";
import { SavedPreferences } from "../components/SavedPreferences";
import { ProfileSupport } from "../components/ProfileSupport";
import { SecuritySettings } from "../components/SecuritySettings";
import { PreferencesForm } from "../components/PreferencesForm";
function editable(p: Profile): ProfileEditable {
  return {
    officeLocation: p.officeLocation,
    officeHours: p.officeHours,
    interests: [...p.interests],
    preferences: { ...p.preferences },
  };
}
function PreviewProfilePage({ role }: { role: PortalRole }) {
  const { profile, error, saving, save, retry } = useProfile(role);
  useEffect(() => {
    document.title = `${copy.profile} — ${role} | CAPNEXA`;
  }, [role]);
  if (!profile)
    return (
      <div className="profile-load" role={error ? "alert" : "status"}>
        {error ? (
          <>
            <p>{error}</p>
            <button className="profile-button" onClick={retry}>
              {copy.tryAgain}
            </button>
          </>
        ) : (
          copy.loading
        )}
      </div>
    );
  return <ProfileContent key={role} profile={profile} saving={saving} error={error} save={save} />;
}
function ProfileContent({
  profile,
  saving,
  error,
  save,
}: {
  profile: Profile;
  saving: boolean;
  error: string;
  save: (changes: ProfileEditable) => Promise<Profile | undefined>;
}) {
  const [draft, setDraft] = useState(() => editable(profile));
  const [message, setMessage] = useState("");
  const [params, setParams] = useSearchParams();
  const tab: ProfileTab =
    params.get("tab") === "security"
      ? "security"
      : params.get("tab") === "preferences"
        ? "preferences"
        : "personal";
  const location = useLocation();
  const dialog = useRef<HTMLDialogElement>(null);
  const [dialogType, setDialogType] = useState<keyof typeof copy.dialogs>("support");
  const dirty = JSON.stringify(draft) !== JSON.stringify(editable(profile));
  function selectTab(next: ProfileTab) {
    const copy = new URLSearchParams(params);
    if (next === "personal") copy.delete("tab");
    else copy.set("tab", next);
    setParams(copy, { replace: true });
    setMessage("");
  }
  useEffect(() => {
    const allowed = [
      "institution-details",
      "availability",
      "research-interests",
      "role-access",
      "security-settings",
      "preferences-settings",
    ];
    const id = location.hash.slice(1);
    if (allowed.includes(id)) {
      const target = document.getElementById(id);
      target?.scrollIntoView({ block: "center" });
      target?.focus({ preventScroll: true });
    }
  }, [location.hash, tab]);
  function openDialog(type: keyof typeof copy.dialogs) {
    setDialogType(type);
    dialog.current?.showModal();
  }
  async function submit() {
    const result = await save(draft);
    if (result) {
      setDraft(editable(result));
      setMessage(copy.saved);
    }
  }
  return (
    <>
      <p className="portal-breadcrumb">
        {profile.role}
        {copy.workspace}
        <span>{copy.profile}</span>
      </p>
      <h1 className="portal-title">{copy.profile2}</h1>
      <p className="portal-subtitle">
        {copy.manageYourDetailsAvailabilityAndNotificationPreferences}
      </p>
      <ProfileHeader profile={profile} />
      <ProfileTabs selected={tab} onChange={selectTab} />
      <div className="profile-grid">
        <div className="profile-primary">
          <div role="tabpanel" id={`panel-${tab}`} aria-labelledby={`tab-${tab}`} tabIndex={0}>
            <form
              onSubmit={(event) => {
                event.preventDefault();
                void submit();
              }}
              aria-busy={saving}
              className="profile-form tw:min-w-0 tw:text-heading tw:font-portal"
            >
              {tab === "personal" ? (
                <>
                  <InstitutionDetails profile={profile} />
                  {profile.role === "Admin" ? <RoleAccessCard profile={profile} /> : null}
                  <AvailabilityForm
                    role={profile.role}
                    location={draft.officeLocation}
                    hours={draft.officeHours}
                    disabled={saving}
                    onChange={(key, value) => {
                      setDraft((old) => ({ ...old, [key]: value }));
                      setMessage("");
                    }}
                  />
                  {profile.role === "Supervisor" ? (
                    <ResearchInterests
                      topics={draft.interests}
                      disabled={saving}
                      onChange={(interests) => {
                        setDraft((old) => ({ ...old, interests }));
                        setMessage("");
                      }}
                    />
                  ) : null}
                  <div className="profile-form-actions">
                    <button
                      type="button"
                      className="profile-button"
                      disabled={saving || !dirty}
                      onClick={() => {
                        setDraft(editable(profile));
                        setMessage(copy.discarded);
                      }}
                    >
                      {copy.cancel}
                    </button>
                    <button
                      type="submit"
                      className="profile-button profile-button-primary"
                      disabled={saving || !dirty}
                    >
                      {saving ? copy.saving : copy.saveChanges}
                    </button>
                  </div>
                </>
              ) : tab === "security" ? (
                <SecuritySettings onOptions={openDialog} />
              ) : (
                <PreferencesForm
                  value={draft.preferences}
                  disabled={saving}
                  dirty={dirty}
                  onChange={(preferences) => {
                    setDraft((old) => ({ ...old, preferences }));
                    setMessage("");
                  }}
                />
              )}
            </form>
          </div>
          <div className="profile-feedback" aria-live="polite" role="status">
            {message}
          </div>
          {error ? (
            <p className="profile-error" role="alert">
              {error}
            </p>
          ) : null}
        </div>
        <aside className="profile-secondary" aria-label={copy.profileInformation}>
          <SavedPreferences preferences={profile.preferences} />
          <ProfileSupport onContact={() => openDialog("support")} />
        </aside>
      </div>
      <dialog className="portal-help" ref={dialog} aria-labelledby="profile-dialog-title">
        <button
          type="button"
          className="portal-icon"
          autoFocus
          onClick={() => dialog.current?.close()}
          aria-label={copy.closeDialog}
        >
          <X size={20} />
        </button>
        <h2 id="profile-dialog-title">{copy.dialogs[dialogType].title}</h2>
        <p>{copy.dialogs[dialogType].text}</p>
        <button className="profile-button" onClick={() => dialog.current?.close()}>
          {copy.close}
        </button>
      </dialog>
    </>
  );
}

export function ProfilePage({ role }: { role: PortalRole }) {
  const user = currentUser();
  if (connected && user)
    return (
      <section className="academic-workspace">
        <header>
          <h1>{copy.accountProfile}</h1>
          <p>{copy.staffProfileNotice}</p>
        </header>
        <div className="academic-records">
          <dl className="record-fields">
            <div>
              <dt>{copy.nameLabel}</dt>
              <dd>
                {user.firstName} {user.lastName}
              </dd>
            </div>
            <div>
              <dt>{copy.emailLabel}</dt>
              <dd>{user.email}</dd>
            </div>
            <div>
              <dt>{copy.rolesLabel}</dt>
              <dd>{user.roles.join(", ")}</dd>
            </div>
          </dl>
        </div>
      </section>
    );
  return <PreviewProfilePage role={role} />;
}

import type { PortalRole } from "@/config/navigation";
import type { Profile } from "../types/profile.types";

export interface ProfileContent {
  fixtures: Record<PortalRole, Profile>;
  dialogs: Record<"password" | "verification" | "support", { title: string; text: string }>;
  loading: string;
  saved: string;
  discarded: string;
  saving: string;
  saveChanges: string;

  accountProfile: string;
  staffProfileNotice: string;
  nameLabel: string;
  emailLabel: string;
  rolesLabel: string;
  tryAgain: string;
  workspace: string;
  profile: string;
  profile2: string;
  manageYourDetailsAvailabilityAndNotificationPreferences: string;
  cancel: string;
  profileInformation: string;
  closeDialog: string;
  close: string;
  officeLocation: string;
  institutionManagedDetails: string;
  readOnly: string;
  notificationWorkspacePreferences: string;
  chooseHowYouReceiveProjectUpdates: string;
  emailDigestSchedule: string;
  daily0800ESTMorningBriefing: string;
  weeklyMonday0800EST: string;
  off: string;
  aggregatesSubmissionUploadsAndStudentMilestoneProgressInto: string;
  deliverableSubmissionAlerts: string;
  immediateNotification: string;
  includeInDigestOnly: string;
  interfaceLanguage: string;
  interfaceTheme: string;
  profileSummary: string;
  separator: string;
  managedByInstitution: string;
  profileSupport: string;
  needToUpdateInstitutionManagedDetailsContactYour: string;
  contactDepartment: string;
  profileSettings: string;
  researchInterests: string;
  academicDomainTopicsUsedForAutomaticStudentProject: string;
  noResearchInterestsAddedYet: string;
  newResearchTopic: string;
  addANewResearchTopicEGComputer: string;
  addTopic: string;
  roleAccess: string;
  permissionsGrantedToYourAdministratorAccount: string;
  savedPreferences: string;
  configuredSystemLocaleAndNotificationRules: string;
  updatesAppearHereAfterYouSave: string;
  securitySignIn: string;
  exploreAccountSecurityOptionsInThisDemo: string;
  passwordOptions: string;
  noPasswordChangesAreMadeInThisPreview: string;
  viewOptions: string;
  twoStepVerification: string;
  noVerificationMethodIsConfiguredByThisDemo: string;
  previewSession: string;
  thisPageDoesNotManageRealAccountSessions: string;
  previewOnly: string;
}
export const profileCopy: ProfileContent = {
  fixtures: {
    Admin: {
      role: "Admin",
      name: "Dr. Omar Hassan",
      initials: "OH",
      title: "System Administrator",
      firstName: "Omar",
      lastName: "Hassan",
      email: "o.hassan@example.edu",
      department: "Office of Academic Affairs",
      institutionId: "ADM-2026-0007",
      accessScope: "All cohorts and workspaces",
      officeLocation: "Admin Building, Room 210",
      officeHours: "Sun–Thu 09:00–15:00 EET",
      interests: [],
      preferences: {
        digest: "daily",
        alerts: "immediate",
        locale: "English (US)",
        theme: "Light Mode",
      },
    },
    Supervisor: {
      role: "Supervisor",
      name: "Dr. Olivia Bennett",
      initials: "OB",
      title: "Faculty Supervisor",
      firstName: "Olivia",
      lastName: "Bennett",
      email: "o.bennett@example.edu",
      department: "Computer Science & Engineering",
      institutionId: "FAC-2026-0491",
      officeLocation: "CS-314 (Tech Hall)",
      officeHours: "Tue/Thu 14:00–16:00 EST",
      interests: ["Artificial Intelligence", "Distributed Systems", "Natural Language Processing"],
      preferences: {
        digest: "daily",
        alerts: "immediate",
        locale: "English (US)",
        theme: "Light Mode",
      },
    },
  },
  dialogs: {
    password: {
      title: "Password options",
      text: "Password changes are not available in this preview. Your institution will provide the account password policy and recovery options.",
    },
    verification: {
      title: "Two-step verification",
      text: "Verification setup is not available yet. No verification method has been enabled or changed.",
    },
    support: {
      title: "Contact your department",
      text: "Contact your department through your university directory to update institution-managed details. A department contact has not been provided yet.",
    },
  },
  loading: "Loading profile...",
  saved: "Changes saved in this browser preview. No server data was changed.",
  discarded: "Unsaved changes discarded.",
  saving: "Saving...",
  saveChanges: "Save Changes",

  accountProfile: "Account profile",
  staffProfileNotice: "Your verified sign-in details. Staff preference updates are unavailable.",
  nameLabel: "Name",
  emailLabel: "Email",
  rolesLabel: "Roles",
  tryAgain: "Try again",
  workspace: "Workspace",
  profile: "/ Profile",
  profile2: "Profile",
  manageYourDetailsAvailabilityAndNotificationPreferences:
    "Manage your details, availability and notification preferences.",
  cancel: "Cancel",
  profileInformation: "Profile information",
  closeDialog: "Close dialog",
  close: "Close",
  officeLocation: "Office Location",
  institutionManagedDetails: "Institution-managed Details",
  readOnly: "Read-only",
  notificationWorkspacePreferences: "Notification & Workspace Preferences",
  chooseHowYouReceiveProjectUpdates: "Choose how you receive project updates.",
  emailDigestSchedule: "Email digest schedule",
  daily0800ESTMorningBriefing: "Daily 08:00 EST (Morning Briefing)",
  weeklyMonday0800EST: "Weekly Monday 08:00 EST",
  off: "Off",
  aggregatesSubmissionUploadsAndStudentMilestoneProgressInto:
    "Aggregates submission uploads and student milestone progress into one email.",
  deliverableSubmissionAlerts: "Deliverable submission alerts",
  immediateNotification: "Immediate notification",
  includeInDigestOnly: "Include in digest only",
  interfaceLanguage: "Interface language",
  interfaceTheme: "Interface theme",
  profileSummary: "Profile summary",
  separator: "·",
  managedByInstitution: "Managed by institution",
  profileSupport: "Profile Support",
  needToUpdateInstitutionManagedDetailsContactYour:
    "Need to update institution-managed details? Contact your department.",
  contactDepartment: "Contact Department",
  profileSettings: "Profile settings",
  researchInterests: "Research Interests",
  academicDomainTopicsUsedForAutomaticStudentProject:
    "Academic domain topics used for automatic student project matching and supervisor allocation.",
  noResearchInterestsAddedYet: "No research interests added yet.",
  newResearchTopic: "New research topic",
  addANewResearchTopicEGComputer: "Add a new research topic (e.g. Computer Vision)...",
  addTopic: "Add topic",
  roleAccess: "Role & Access",
  permissionsGrantedToYourAdministratorAccount:
    "Permissions granted to your administrator account.",
  savedPreferences: "Saved Preferences",
  configuredSystemLocaleAndNotificationRules: "Configured system locale and notification rules.",
  updatesAppearHereAfterYouSave: "Updates appear here after you save.",
  securitySignIn: "Security & sign-in",
  exploreAccountSecurityOptionsInThisDemo: "Explore account security options in this demo.",
  passwordOptions: "Password options",
  noPasswordChangesAreMadeInThisPreview: "No password changes are made in this preview.",
  viewOptions: "View options",
  twoStepVerification: "Two-step verification",
  noVerificationMethodIsConfiguredByThisDemo: "No verification method is configured by this demo.",
  previewSession: "Preview session",
  thisPageDoesNotManageRealAccountSessions: "This page does not manage real account sessions.",
  previewOnly: "Preview only",
};

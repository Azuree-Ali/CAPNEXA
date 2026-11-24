export interface PortalContent {
  profileSections: readonly { label: string; tab: string; id: string; terms: string }[];
  menu: string;
  closeNavigation: string;
  logout: string;
  planned: string;
  searchLabel: string;
  searchHeading: string;
  searchResults: string;
  noDestinations: string;
  searchHint: string;
  notifications: string;
  helpLabel: string;
  preferencesLabel: string;
  searchPlaceholder: string;
  accountProfile: string;
  accountTerms: string;
  skip: string;
  navigation: string;
  closeHelp: string;
  helpTitle: string;
  liveHelp: string;
  previewHelp: string;
  notificationHelp: string;
}

export const portalCopy: PortalContent = {
  profileSections: [
    {
      label: "Personal Information",
      tab: "personal",
      id: "institution-details",
      terms: "name email department office profile",
    },
    {
      label: "Office & Availability",
      tab: "personal",
      id: "availability",
      terms: "contact location hours schedule",
    },
    {
      label: "Research Interests",
      tab: "personal",
      id: "research-interests",
      terms: "topics artificial intelligence distributed systems",
    },
    {
      label: "Role & Access",
      tab: "personal",
      id: "role-access",
      terms: "permissions administrator scope",
    },
    {
      label: "Security & sign-in",
      tab: "security",
      id: "security-settings",
      terms: "password verification session security",
    },
    {
      label: "Notification & Workspace Preferences",
      tab: "preferences",
      id: "preferences-settings",
      terms: "digest alerts notifications language theme",
    },
  ],
  menu: "Open navigation",
  closeNavigation: "Close navigation",
  logout: "Log out",
  planned: "Planned destination",
  searchLabel: "Search workspace",
  searchHeading: "Profile sections",
  searchResults: "Search results",
  noDestinations: "No matching destinations.",
  searchHint: "Search available workspace destinations.",
  notifications: "Notifications",
  helpLabel: "Help & Documentation",
  preferencesLabel: "Open preferences",
  searchPlaceholder: "Search destinations…",
  accountProfile: "Account profile",
  accountTerms: "name email identity role",
  skip: "Skip to content",
  navigation: "navigation",
  closeHelp: "Close help",
  helpTitle: "CAPNEXA workspace help",
  liveHelp:
    "The account profile shows identity and roles. Profile preferences are unavailable in connected mode.",
  previewHelp:
    "Use search to find workspace destinations and profile sections. Profile changes are stored in this browser preview.",
  notificationHelp: "The Admin notification center contains demonstration records.",
};

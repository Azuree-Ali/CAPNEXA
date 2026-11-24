export interface NotificationsContent {
  title: string;
  description: string;
  navigation: string;
  all: string;
  unread: string;
  read: string;
  markRead: string;
  markUnread: string;
  empty: string;
  events: readonly {
    id: string;
    title: string;
    project: string;
    time: string;
    action: string;
    route: string;
  }[];
}
export const notificationsContent: NotificationsContent = {
  title: "Notifications",
  description: "Review updates from the demonstration cohort.",
  navigation: "Notification views",
  all: "All updates",
  unread: "Unread",
  read: "Read",
  markRead: "Mark as read",
  markUnread: "Mark as unread",
  empty: "You have no unread updates.",
  events: [
    {
      id: "design-brief",
      title: "Design brief submitted",
      project: "Campus Atlas",
      time: "12 December",
      action: "Review submission",
      route: "/admin/submissions",
    },
    {
      id: "proposal",
      title: "Proposal awaiting review",
      project: "Library Lens",
      time: "10 December",
      action: "Review proposal",
      route: "/admin/proposals/review",
    },
  ],
};

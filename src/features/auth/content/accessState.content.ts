export interface AccessStateContent {
  title: string;
  description: string;
  action: string;
  route: string;
}
export const accessStates: Record<"expired" | "forbidden" | "missing", AccessStateContent> = {
  expired: {
    title: "Session expired",
    description: "Sign in again to return to your CAPNEXA workspace.",
    action: "Sign in again",
    route: "/login",
  },
  forbidden: {
    title: "Access denied",
    description: "Your role does not have permission to open this workspace.",
    action: "Back to sign-in",
    route: "/login",
  },
  missing: {
    title: "Page not found",
    description: "Check the address or return to the sign-in page.",
    action: "Back to sign-in",
    route: "/login",
  },
};

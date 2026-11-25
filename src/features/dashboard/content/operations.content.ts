import { Check, CircleSlash, Gauge, Link2, UserCheck, UserX, type LucideIcon } from "lucide-react";
import type {
  Change,
  OperationDomain,
  OperationListContent,
  ProposalStatus,
} from "../types/operations.types";
import { studentsContent } from "@/features/students/content/students.content";
import { supervisorsContent } from "@/features/supervisors/content/supervisors.content";
import { proposalsContent } from "@/features/proposals/content/proposals.content";
import { assignmentsContent } from "@/features/assignments/content/assignments.content";

export type { ProposalStatus };

export const operationPages: Record<OperationDomain, OperationListContent> = {
  students: studentsContent.live,
  supervisors: supervisorsContent.live,
  proposals: proposalsContent.live,
  assignments: assignmentsContent.live,
};

export interface OperationsContent {
  proposalStatuses: Record<ProposalStatus, string>;
  confirmation: Record<Change, string>;
  actionIcons: Record<Change, LucideIcon>;
  success: Record<Change, string>;
  text: Record<
    | "preview"
    | "previewMissing"
    | "previewUnavailable"
    | "previewTools"
    | "proposalDemo"
    | "supervisorDemo"
    | "search"
    | "retry"
    | "loading"
    | "empty"
    | "next"
    | "previous"
    | "open"
    | "confirm"
    | "cancel"
    | "back"
    | "field"
    | "value"
    | "activate"
    | "deactivate"
    | "capacity"
    | "reason"
    | "reasonRequired"
    | "teamCode"
    | "members"
    | "submitted"
    | "decision"
    | "reviewedBy"
    | "reviewedOn"
    | "leader"
    | "specialization"
    | "saving"
    | "approve"
    | "reject"
    | "assign"
    | "reassign"
    | "reassignSuccess"
    | "supervisor"
    | "inspect"
    | "error"
    | "active"
    | "inactive"
    | "missing"
    | "account"
    | "accountFilter"
    | "capacityFilter"
    | "all"
    | "availableSlots"
    | "atCapacity"
    | "clearFilters"
    | "closeDialog"
    | "department"
    | "team"
    | "status"
    | "project"
    | "description"
    | "available"
    | "assigned"
    | "remainingSlots"
    | "again"
    | "number"
    | "live"
    | "email"
    | "year"
    | "level"
    | "emailStatus"
    | "profileStatus"
    | "confirmed"
    | "unconfirmed"
    | "complete"
    | "incomplete"
    | "joined"
    | "role"
    | "creator"
    | "member"
    | "teamMembership"
    | "noTeams"
    | "assignedProjects"
    | "noProjects"
    | "noMembers"
    | "reviewHistory"
    | "noReview"
    | "reviewed"
    | "research"
    | "recordOverview"
    | "total"
    | "page"
    | "capacityError"
    | "capacityHint"
    | "refresh"
    | "recordsNavigation"
    | "requiredSupervisor",
    string
  >;
}
export const operations: OperationsContent = {
  proposalStatuses: { PendingReview: "Pending review", Approved: "Approved", Rejected: "Rejected" },
  actionIcons: {
    activate: UserCheck,
    deactivate: UserX,
    capacity: Gauge,
    approve: Check,
    reject: CircleSlash,
    assign: Link2,
  },
  confirmation: {
    activate: "Restore this student's account access.",
    deactivate: "Disable this student's account access.",
    capacity: "Set the maximum number of projects this supervisor may oversee.",
    approve: "Approve this proposal and make it available for supervisor assignment.",
    reject: "Reject this proposal and record the reason for the decision.",
    assign: "Assign an eligible supervisor to this approved project.",
  },
  success: {
    activate: "Student account activated.",
    deactivate: "Student account deactivated.",
    capacity: "Supervisor capacity updated.",
    approve: "Proposal approved. It is eligible for supervisor assignment.",
    reject: "Proposal rejected.",
    assign: "Supervisor assigned.",
  },
  text: {
    preview: "Demonstration data · Changes stay in this tab until reload.",
    previewMissing: "This demonstration record is unavailable.",
    previewUnavailable: "This demonstration change is no longer available. Refresh and try again.",
    previewTools: "Add or edit demonstration students",
    proposalDemo: "Demonstration proposal dossier",
    supervisorDemo: "Demonstration faculty profile",
    search: "Search records",
    retry: "Retry",
    loading: "Loading records…",
    empty: "No matching records.",
    next: "Next",
    previous: "Previous",
    open: "Open",
    confirm: "Confirm change",
    cancel: "Cancel",
    back: "Back to directory",
    field: "Record",
    value: "Details",
    activate: "Activate account",
    deactivate: "Deactivate account",
    capacity: "Maximum projects",
    reason: "Decision reason",
    teamCode: "Team code",
    members: "Team members",
    submitted: "Submitted",
    decision: "Decision",
    reviewedBy: "Reviewed by",
    reviewedOn: "Reviewed on",
    leader: "Team leader",
    specialization: "Specialization",
    reasonRequired: "Enter a reason for rejecting this proposal.",
    saving: "Saving change…",
    approve: "Approve proposal",
    reject: "Reject proposal",
    assign: "Assign supervisor",
    reassign: "Reassign supervisor",
    reassignSuccess: "Supervisor reassigned.",
    supervisor: "Supervisor ID",
    inspect: "Review change",
    error: "Unable to complete this request.",
    active: "Active",
    inactive: "Inactive",
    missing: "Unavailable",
    account: "Account",
    accountFilter: "Account status",
    capacityFilter: "Capacity",
    all: "All",
    availableSlots: "With available capacity",
    atCapacity: "At capacity",
    clearFilters: "Clear filters",
    closeDialog: "Close dialog",
    department: "Department",
    team: "Team",
    status: "Status",
    project: "Project",
    description: "Description",
    available: "Available slots",
    assigned: "Assigned projects",
    remainingSlots: "Remaining slots",
    again: "Sign in again",
    number: "Student number",
    live: "Connected to the configured university API",
    email: "Institutional email",
    year: "Academic year",
    level: "Academic level",
    emailStatus: "Email status",
    profileStatus: "Academic profile",
    confirmed: "Confirmed",
    unconfirmed: "Unconfirmed",
    complete: "Complete",
    incomplete: "Incomplete",
    joined: "Joined",
    role: "Role",
    creator: "Team creator",
    member: "Member",
    teamMembership: "Team memberships",
    noTeams: "No team memberships are recorded.",
    assignedProjects: "Assigned projects",
    noProjects: "No assigned projects are recorded.",
    noMembers: "No team members are recorded.",
    reviewHistory: "Review history",
    noReview: "No administrative decision has been recorded.",
    reviewed: "Reviewed on",
    research: "Research interests",
    recordOverview: "Record overview",
    total: "Matching records",
    page: "Page",
    capacityError: "Enter a whole number between the assigned workload and 20.",
    capacityHint: "The new maximum cannot be lower than the assigned workload.",
    refresh: "Refresh records",
    recordsNavigation: "Record pages",
    requiredSupervisor: "Select an available supervisor.",
  },
};

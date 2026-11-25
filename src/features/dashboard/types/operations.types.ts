export type OperationDomain = "students" | "supervisors" | "proposals" | "assignments";
export type ProposalStatus = "PendingReview" | "Approved" | "Rejected";
export type Change = "activate" | "deactivate" | "capacity" | "approve" | "reject" | "assign";
export interface OperationFilters {
  status?: ProposalStatus;
  isActive?: boolean;
  hasCapacity?: boolean;
}
export interface OperationListContent {
  title: string;
  description: string;
  columns: readonly [string, ...string[]];
}
export interface Entry {
  id: string;
  title: string;
  cells: readonly string[];
  fields?: RecordField[];
}
export interface Listing {
  items: Entry[];
  totalCount: number;
  totalPages: number;
}
export type RecordField = [label: string, value: string];
export interface RecordAction {
  change: Change;
  targetId: string;
  targetTitle: string;
  label: string;
}
export interface RecordSectionItem {
  id: string;
  title: string;
  fields: RecordField[];
  action?: RecordAction;
}
export interface RecordSection {
  title: string;
  records: RecordSectionItem[];
  empty: string;
}
export interface Detail {
  title: string;
  fields: RecordField[];
  actions: Change[];
  sections?: RecordSection[];
  capacity?: { assigned: number; maximum: number };
}

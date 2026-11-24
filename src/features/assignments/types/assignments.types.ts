export interface AdminUnassignedProjectsQueryDto {
  search?: string | null;
  page?: number;
  pageSize?: number;
}
export interface AdminUnassignedProjectListDto {
  items: AdminUnassignedProjectListItemDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
export interface AdminUnassignedProjectListItemDto {
  projectId: number;
  title: string;
  reviewedAt: string | null;
  team: AdminProposalTeamSummaryDto;
  leaderName: string;
}
export interface AdminProposalTeamSummaryDto {
  teamId: number;
  name: string;
  teamCode: string;
  memberCount: number;
}
export interface AssignSupervisorDto {
  supervisorId: number | null;
}
export interface AdminProjectAssignmentDto {
  projectId: number;
  title: string;
  status: string;
  team: AdminProposalTeamSummaryDto;
  supervisor: AdminProjectSupervisorDto;
}
export interface AdminProjectSupervisorDto {
  supervisorId: number;
  userId: string;
  firstName: string;
  lastName: string;
  email: string | null;
  maxProjects: number;
  assignedProjectsCount: number;
  availableCapacity: number;
}

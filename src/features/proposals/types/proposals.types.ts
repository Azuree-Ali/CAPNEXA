export interface AdminProposalQueryDto {
  status?: string | null;
  search?: string | null;
  page?: number;
  pageSize?: number;
}
export interface AdminProposalListDto {
  items: AdminProposalListItemDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
export interface AdminProposalListItemDto {
  projectId: number;
  title: string;
  status: string;
  submittedAt: string | null;
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
export interface AdminProposalDetailDto {
  projectId: number;
  title: string;
  description: string | null;
  status: string;
  createdAt: string;
  submittedAt: string | null;
  team: AdminProposalTeamDto;
  review: AdminProposalReviewDto | null;
}
export interface AdminProposalTeamDto {
  teamId: number;
  name: string;
  teamCode: string;
  maxMembers: number;
  memberCount: number;
  members: AdminProposalMemberDto[];
}
export interface AdminProposalMemberDto {
  userId: string;
  firstName: string;
  lastName: string;
  email: string | null;
  studentNumber: string;
  department: string | null;
  academicYear: string | null;
  isLeader: boolean;
  joinedAt: string;
}
export interface AdminProposalReviewDto {
  decision: string;
  reason: string | null;
  reviewedByUserId: string | null;
  reviewedByName: string | null;
  reviewedAt: string | null;
}
export interface ReviewProjectProposalDto {
  decision: string;
  reason: string | null;
}

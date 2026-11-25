import { isObject } from "@/lib/api/api-client";
import type {
  AdminProposalDetailDto,
  AdminProposalListDto,
  AdminProposalListItemDto,
  AdminProposalMemberDto,
  AdminProposalReviewDto,
  AdminProposalTeamDto,
  AdminProposalTeamSummaryDto,
} from "../types/proposals.types";
export function isAdminProposalListDto(value: unknown): value is AdminProposalListDto {
  return (
    isObject(value) &&
    Array.isArray(value.items) &&
    value.items.every((item: unknown) => isAdminProposalListItemDto(item)) &&
    typeof value.page === "number" &&
    Number.isSafeInteger(value.page) &&
    typeof value.pageSize === "number" &&
    Number.isSafeInteger(value.pageSize) &&
    typeof value.totalCount === "number" &&
    Number.isSafeInteger(value.totalCount) &&
    typeof value.totalPages === "number" &&
    Number.isSafeInteger(value.totalPages)
  );
}
export function isAdminProposalListItemDto(value: unknown): value is AdminProposalListItemDto {
  return (
    isObject(value) &&
    typeof value.projectId === "number" &&
    Number.isSafeInteger(value.projectId) &&
    typeof value.title === "string" &&
    typeof value.status === "string" &&
    (value.submittedAt === null || typeof value.submittedAt === "string") &&
    (value.reviewedAt === null || typeof value.reviewedAt === "string") &&
    isAdminProposalTeamSummaryDto(value.team) &&
    typeof value.leaderName === "string"
  );
}
export function isAdminProposalTeamSummaryDto(
  value: unknown,
): value is AdminProposalTeamSummaryDto {
  return (
    isObject(value) &&
    typeof value.teamId === "number" &&
    Number.isSafeInteger(value.teamId) &&
    typeof value.name === "string" &&
    typeof value.teamCode === "string" &&
    typeof value.memberCount === "number" &&
    Number.isSafeInteger(value.memberCount)
  );
}
export function isAdminProposalDetailDto(value: unknown): value is AdminProposalDetailDto {
  return (
    isObject(value) &&
    typeof value.projectId === "number" &&
    Number.isSafeInteger(value.projectId) &&
    typeof value.title === "string" &&
    (value.description === null || typeof value.description === "string") &&
    typeof value.status === "string" &&
    typeof value.createdAt === "string" &&
    (value.submittedAt === null || typeof value.submittedAt === "string") &&
    isAdminProposalTeamDto(value.team) &&
    (value.review === null || isAdminProposalReviewDto(value.review))
  );
}
export function isAdminProposalTeamDto(value: unknown): value is AdminProposalTeamDto {
  return (
    isObject(value) &&
    typeof value.teamId === "number" &&
    Number.isSafeInteger(value.teamId) &&
    typeof value.name === "string" &&
    typeof value.teamCode === "string" &&
    typeof value.maxMembers === "number" &&
    Number.isSafeInteger(value.maxMembers) &&
    typeof value.memberCount === "number" &&
    Number.isSafeInteger(value.memberCount) &&
    Array.isArray(value.members) &&
    value.members.every((item: unknown) => isAdminProposalMemberDto(item))
  );
}
export function isAdminProposalMemberDto(value: unknown): value is AdminProposalMemberDto {
  return (
    isObject(value) &&
    typeof value.userId === "string" &&
    typeof value.firstName === "string" &&
    typeof value.lastName === "string" &&
    (value.email === null || typeof value.email === "string") &&
    typeof value.studentNumber === "string" &&
    (value.department === null || typeof value.department === "string") &&
    (value.academicYear === null || typeof value.academicYear === "string") &&
    typeof value.isLeader === "boolean" &&
    typeof value.joinedAt === "string"
  );
}
export function isAdminProposalReviewDto(value: unknown): value is AdminProposalReviewDto {
  return (
    isObject(value) &&
    typeof value.decision === "string" &&
    (value.reason === null || typeof value.reason === "string") &&
    (value.reviewedByUserId === null || typeof value.reviewedByUserId === "string") &&
    (value.reviewedByName === null || typeof value.reviewedByName === "string") &&
    (value.reviewedAt === null || typeof value.reviewedAt === "string")
  );
}

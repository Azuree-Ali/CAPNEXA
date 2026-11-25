import { isObject } from "@/lib/api/api-client";
import type {
  AdminProjectAssignmentDto,
  AdminProjectSupervisorDto,
  AdminProposalTeamSummaryDto,
  AdminUnassignedProjectListDto,
  AdminUnassignedProjectListItemDto,
} from "../types/assignments.types";
export function isAdminUnassignedProjectListDto(
  value: unknown,
): value is AdminUnassignedProjectListDto {
  return (
    isObject(value) &&
    Array.isArray(value.items) &&
    value.items.every((item: unknown) => isAdminUnassignedProjectListItemDto(item)) &&
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
export function isAdminUnassignedProjectListItemDto(
  value: unknown,
): value is AdminUnassignedProjectListItemDto {
  return (
    isObject(value) &&
    typeof value.projectId === "number" &&
    Number.isSafeInteger(value.projectId) &&
    typeof value.title === "string" &&
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
export function isAdminProjectAssignmentDto(value: unknown): value is AdminProjectAssignmentDto {
  return (
    isObject(value) &&
    typeof value.projectId === "number" &&
    Number.isSafeInteger(value.projectId) &&
    typeof value.title === "string" &&
    typeof value.status === "string" &&
    isAdminProposalTeamSummaryDto(value.team) &&
    isAdminProjectSupervisorDto(value.supervisor)
  );
}
export function isAdminProjectSupervisorDto(value: unknown): value is AdminProjectSupervisorDto {
  return (
    isObject(value) &&
    typeof value.supervisorId === "number" &&
    Number.isSafeInteger(value.supervisorId) &&
    typeof value.userId === "string" &&
    typeof value.firstName === "string" &&
    typeof value.lastName === "string" &&
    (value.email === null || typeof value.email === "string") &&
    typeof value.maxProjects === "number" &&
    Number.isSafeInteger(value.maxProjects) &&
    typeof value.assignedProjectsCount === "number" &&
    Number.isSafeInteger(value.assignedProjectsCount) &&
    typeof value.availableCapacity === "number" &&
    Number.isSafeInteger(value.availableCapacity)
  );
}

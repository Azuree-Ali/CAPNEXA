import { isObject } from "@/lib/api/api-client";
import type {
  AdminSupervisorDetailDto,
  AdminSupervisorListDto,
  AdminSupervisorListItemDto,
  AdminSupervisorProjectDto,
} from "../types/supervisors.types";
export function isAdminSupervisorListDto(value: unknown): value is AdminSupervisorListDto {
  return (
    isObject(value) &&
    Array.isArray(value.items) &&
    value.items.every((item: unknown) => isAdminSupervisorListItemDto(item)) &&
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
export function isAdminSupervisorListItemDto(value: unknown): value is AdminSupervisorListItemDto {
  return (
    isObject(value) &&
    typeof value.supervisorId === "number" &&
    Number.isSafeInteger(value.supervisorId) &&
    typeof value.userId === "string" &&
    typeof value.firstName === "string" &&
    typeof value.lastName === "string" &&
    (value.email === null || typeof value.email === "string") &&
    typeof value.isActive === "boolean" &&
    (value.department === null || typeof value.department === "string") &&
    (value.specialization === null || typeof value.specialization === "string") &&
    (value.researchInterests === null || typeof value.researchInterests === "string") &&
    typeof value.maxProjects === "number" &&
    Number.isSafeInteger(value.maxProjects) &&
    typeof value.assignedProjectsCount === "number" &&
    Number.isSafeInteger(value.assignedProjectsCount) &&
    typeof value.availableCapacity === "number" &&
    Number.isSafeInteger(value.availableCapacity)
  );
}
export function isAdminSupervisorDetailDto(value: unknown): value is AdminSupervisorDetailDto {
  return (
    isObject(value) &&
    typeof value.supervisorId === "number" &&
    Number.isSafeInteger(value.supervisorId) &&
    typeof value.userId === "string" &&
    typeof value.firstName === "string" &&
    typeof value.lastName === "string" &&
    (value.email === null || typeof value.email === "string") &&
    typeof value.isActive === "boolean" &&
    (value.department === null || typeof value.department === "string") &&
    (value.specialization === null || typeof value.specialization === "string") &&
    (value.researchInterests === null || typeof value.researchInterests === "string") &&
    typeof value.maxProjects === "number" &&
    Number.isSafeInteger(value.maxProjects) &&
    typeof value.assignedProjectsCount === "number" &&
    Number.isSafeInteger(value.assignedProjectsCount) &&
    typeof value.availableCapacity === "number" &&
    Number.isSafeInteger(value.availableCapacity) &&
    Array.isArray(value.assignedProjects) &&
    value.assignedProjects.every((item: unknown) => isAdminSupervisorProjectDto(item))
  );
}
export function isAdminSupervisorProjectDto(value: unknown): value is AdminSupervisorProjectDto {
  return (
    isObject(value) &&
    typeof value.projectId === "number" &&
    Number.isSafeInteger(value.projectId) &&
    typeof value.title === "string" &&
    typeof value.status === "string" &&
    typeof value.teamId === "number" &&
    Number.isSafeInteger(value.teamId) &&
    typeof value.teamName === "string" &&
    typeof value.teamCode === "string"
  );
}

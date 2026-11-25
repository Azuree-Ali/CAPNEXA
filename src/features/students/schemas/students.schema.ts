import { isObject } from "@/lib/api/api-client";
import type {
  AdminStudentListDto,
  AdminStudentListItemDto,
  AdminStudentStatusDto,
  StudentProfileDto,
  StudentProjectDto,
  StudentTeamDto,
} from "../types/students.types";
export function isAdminStudentListDto(value: unknown): value is AdminStudentListDto {
  return (
    isObject(value) &&
    Array.isArray(value.items) &&
    value.items.every((item: unknown) => isAdminStudentListItemDto(item)) &&
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
export function isAdminStudentListItemDto(value: unknown): value is AdminStudentListItemDto {
  return (
    isObject(value) &&
    typeof value.userId === "string" &&
    (value.email === null || typeof value.email === "string") &&
    typeof value.firstName === "string" &&
    typeof value.lastName === "string" &&
    (value.studentNumber === null || typeof value.studentNumber === "string") &&
    (value.department === null || typeof value.department === "string") &&
    (value.academicYear === null || typeof value.academicYear === "string") &&
    typeof value.academicProfileComplete === "boolean" &&
    typeof value.emailConfirmed === "boolean" &&
    typeof value.isActive === "boolean" &&
    typeof value.createdAt === "string"
  );
}
export function isStudentProfileDto(value: unknown): value is StudentProfileDto {
  return (
    isObject(value) &&
    typeof value.userId === "string" &&
    (value.email === null || typeof value.email === "string") &&
    typeof value.emailConfirmed === "boolean" &&
    typeof value.firstName === "string" &&
    typeof value.lastName === "string" &&
    typeof value.isActive === "boolean" &&
    typeof value.createdAt === "string" &&
    typeof value.academicProfileComplete === "boolean" &&
    (value.studentNumber === null || typeof value.studentNumber === "string") &&
    (value.department === null || typeof value.department === "string") &&
    (value.academicYear === null || typeof value.academicYear === "string") &&
    (value.level === null || typeof value.level === "string") &&
    Array.isArray(value.teams) &&
    value.teams.every((item: unknown) => isStudentTeamDto(item))
  );
}
export function isStudentTeamDto(value: unknown): value is StudentTeamDto {
  return (
    isObject(value) &&
    typeof value.teamId === "number" &&
    Number.isSafeInteger(value.teamId) &&
    typeof value.name === "string" &&
    typeof value.teamCode === "string" &&
    typeof value.isLeader === "boolean" &&
    typeof value.isCreator === "boolean" &&
    (value.joinedAt === null || typeof value.joinedAt === "string") &&
    Array.isArray(value.projects) &&
    value.projects.every((item: unknown) => isStudentProjectDto(item))
  );
}
export function isStudentProjectDto(value: unknown): value is StudentProjectDto {
  return (
    isObject(value) &&
    typeof value.projectId === "number" &&
    Number.isSafeInteger(value.projectId) &&
    typeof value.title === "string" &&
    typeof value.status === "string" &&
    typeof value.createdAt === "string"
  );
}
export function isAdminStudentStatusDto(value: unknown): value is AdminStudentStatusDto {
  return isObject(value) && typeof value.userId === "string" && typeof value.isActive === "boolean";
}

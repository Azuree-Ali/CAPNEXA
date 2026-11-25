import { apiRequest } from "@/lib/api/api-client";
import type {
  AdminProjectAssignmentDto,
  AdminUnassignedProjectListDto,
  AdminUnassignedProjectsQueryDto,
  AssignSupervisorDto,
} from "../types/assignments.types";
import {
  isAdminProjectAssignmentDto,
  isAdminUnassignedProjectListDto,
} from "../schemas/assignments.schema";

// [BACKEND-API] listUnassigned: GET /api/Admin/ProjectAssignments/unassigned. Request: request: AdminUnassignedProjectsQueryDto. Response: AdminUnassignedProjectListDto.
export function listUnassigned(
  request: AdminUnassignedProjectsQueryDto,
  signal?: AbortSignal,
): Promise<AdminUnassignedProjectListDto> {
  const query = new URLSearchParams();
  for (const [key, value] of Object.entries(request)) {
    if (value !== undefined && value !== null) query.set(key, String(value));
  }
  return apiRequest(
    `/api/Admin/ProjectAssignments/unassigned${query.size ? "?" + query : ""}`,
    "GET",
    isAdminUnassignedProjectListDto,
    undefined,
    signal,
  );
}

// [BACKEND-API] assignSupervisor: PUT /api/Admin/ProjectAssignments/{projectId}. Request: projectId: number, request: AssignSupervisorDto. Response: AdminProjectAssignmentDto.
export function assignSupervisor(
  projectId: number,
  request: AssignSupervisorDto,
  signal?: AbortSignal,
): Promise<AdminProjectAssignmentDto> {
  return apiRequest(
    `/api/Admin/ProjectAssignments/${encodeURIComponent(projectId)}`,
    "PUT",
    isAdminProjectAssignmentDto,
    request,
    signal,
  );
}

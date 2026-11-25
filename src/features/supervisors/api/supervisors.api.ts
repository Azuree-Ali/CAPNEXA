import { apiRequest } from "@/lib/api/api-client";
import type {
  AdminSupervisorDetailDto,
  AdminSupervisorListDto,
  AdminSupervisorQueryDto,
  UpdateSupervisorCapacityDto,
} from "../types/supervisors.types";
import {
  isAdminSupervisorDetailDto,
  isAdminSupervisorListDto,
} from "../schemas/supervisors.schema";

// [BACKEND-API] listSupervisors: GET /api/Admin/Supervisors. Request: request: AdminSupervisorQueryDto. Response: AdminSupervisorListDto.
export function listSupervisors(
  request: AdminSupervisorQueryDto,
  signal?: AbortSignal,
): Promise<AdminSupervisorListDto> {
  const query = new URLSearchParams();
  for (const [key, value] of Object.entries(request)) {
    if (value !== undefined && value !== null) query.set(key, String(value));
  }
  return apiRequest(
    `/api/Admin/Supervisors${query.size ? "?" + query : ""}`,
    "GET",
    isAdminSupervisorListDto,
    undefined,
    signal,
  );
}

// [BACKEND-API] supervisorDetails: GET /api/Admin/Supervisors/{supervisorId}. Request: supervisorId: number. Response: AdminSupervisorDetailDto.
export function supervisorDetails(
  supervisorId: number,
  signal?: AbortSignal,
): Promise<AdminSupervisorDetailDto> {
  return apiRequest(
    `/api/Admin/Supervisors/${encodeURIComponent(supervisorId)}`,
    "GET",
    isAdminSupervisorDetailDto,
    undefined,
    signal,
  );
}

// [BACKEND-API] setCapacity: PUT /api/Admin/Supervisors/{supervisorId}/max-projects. Request: supervisorId: number, request: UpdateSupervisorCapacityDto. Response: AdminSupervisorDetailDto.
export function setCapacity(
  supervisorId: number,
  request: UpdateSupervisorCapacityDto,
  signal?: AbortSignal,
): Promise<AdminSupervisorDetailDto> {
  return apiRequest(
    `/api/Admin/Supervisors/${encodeURIComponent(supervisorId)}/max-projects`,
    "PUT",
    isAdminSupervisorDetailDto,
    request,
    signal,
  );
}

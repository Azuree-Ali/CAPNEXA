import { apiRequest } from "@/lib/api/api-client";
import type {
  AdminStudentListDto,
  AdminStudentQueryDto,
  AdminStudentStatusDto,
  StudentProfileDto,
  UpdateStudentStatusDto,
} from "../types/students.types";
import {
  isAdminStudentListDto,
  isAdminStudentStatusDto,
  isStudentProfileDto,
} from "../schemas/students.schema";

// [BACKEND-API] listStudents: GET /api/Admin/Students. Request: request: AdminStudentQueryDto. Response: AdminStudentListDto.
export function listStudents(
  request: AdminStudentQueryDto,
  signal?: AbortSignal,
): Promise<AdminStudentListDto> {
  const query = new URLSearchParams();
  for (const [key, value] of Object.entries(request)) {
    if (value !== undefined && value !== null) query.set(key, String(value));
  }
  return apiRequest(
    `/api/Admin/Students${query.size ? "?" + query : ""}`,
    "GET",
    isAdminStudentListDto,
    undefined,
    signal,
  );
}

// [BACKEND-API] studentProfile: GET /api/Admin/Students/{userId}/profile. Request: userId: string. Response: StudentProfileDto.
export function studentProfile(userId: string, signal?: AbortSignal): Promise<StudentProfileDto> {
  return apiRequest(
    `/api/Admin/Students/${encodeURIComponent(userId)}/profile`,
    "GET",
    isStudentProfileDto,
    undefined,
    signal,
  );
}

// [BACKEND-API] setStudentStatus: PUT /api/Admin/Students/{userId}/status. Request: userId: string, request: UpdateStudentStatusDto. Response: AdminStudentStatusDto.
export function setStudentStatus(
  userId: string,
  request: UpdateStudentStatusDto,
  signal?: AbortSignal,
): Promise<AdminStudentStatusDto> {
  return apiRequest(
    `/api/Admin/Students/${encodeURIComponent(userId)}/status`,
    "PUT",
    isAdminStudentStatusDto,
    request,
    signal,
  );
}

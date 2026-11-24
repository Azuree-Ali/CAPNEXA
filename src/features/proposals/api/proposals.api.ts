import { apiRequest } from "@/lib/api/api-client";
import type {
  AdminProposalDetailDto,
  AdminProposalListDto,
  AdminProposalQueryDto,
  ReviewProjectProposalDto,
} from "../types/proposals.types";
import { isAdminProposalDetailDto, isAdminProposalListDto } from "../schemas/proposals.schema";

// [BACKEND-API] listProposals: GET /api/Admin/ProjectProposals. Request: request: AdminProposalQueryDto. Response: AdminProposalListDto.
export function listProposals(
  request: AdminProposalQueryDto,
  signal?: AbortSignal,
): Promise<AdminProposalListDto> {
  const query = new URLSearchParams();
  for (const [key, value] of Object.entries(request)) {
    if (value !== undefined && value !== null) query.set(key, String(value));
  }
  return apiRequest(
    `/api/Admin/ProjectProposals${query.size ? "?" + query : ""}`,
    "GET",
    isAdminProposalListDto,
    undefined,
    signal,
  );
}

// [BACKEND-API] proposalDetails: GET /api/Admin/ProjectProposals/{projectId}. Request: projectId: number. Response: AdminProposalDetailDto.
export function proposalDetails(
  projectId: number,
  signal?: AbortSignal,
): Promise<AdminProposalDetailDto> {
  return apiRequest(
    `/api/Admin/ProjectProposals/${encodeURIComponent(projectId)}`,
    "GET",
    isAdminProposalDetailDto,
    undefined,
    signal,
  );
}

// [BACKEND-API] reviewProposal: PUT /api/Admin/ProjectProposals/{projectId}/review. Request: projectId: number, request: ReviewProjectProposalDto. Response: AdminProposalDetailDto.
export function reviewProposal(
  projectId: number,
  request: ReviewProjectProposalDto,
  signal?: AbortSignal,
): Promise<AdminProposalDetailDto> {
  return apiRequest(
    `/api/Admin/ProjectProposals/${encodeURIComponent(projectId)}/review`,
    "PUT",
    isAdminProposalDetailDto,
    request,
    signal,
  );
}

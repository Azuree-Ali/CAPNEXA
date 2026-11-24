import { apiRequest } from "@/lib/api/api-client";
import type { AuthResponseDto, LoginDto } from "../types/identity.types";
import { isAuthResponseDto } from "../schemas/auth.schema";

// [BACKEND-API] signIn: POST /api/Identity/Auth/login. Request: request: LoginDto. Response: AuthResponseDto.
export function signIn(request: LoginDto, signal?: AbortSignal): Promise<AuthResponseDto> {
  return apiRequest(`/api/Identity/Auth/login`, "POST", isAuthResponseDto, request, signal);
}

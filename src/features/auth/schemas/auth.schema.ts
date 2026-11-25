import { isObject } from "@/lib/api/api-client";
import type { AuthResponseDto, UserResponseDto } from "../types/identity.types";
export function isAuthResponseDto(value: unknown): value is AuthResponseDto {
  return (
    isObject(value) &&
    typeof value.accessToken === "string" &&
    value.accessToken.length > 0 &&
    value.tokenType === "Bearer" &&
    typeof value.expiresInSeconds === "number" &&
    Number.isSafeInteger(value.expiresInSeconds) &&
    value.expiresInSeconds > 0 &&
    typeof value.refreshToken === "string" &&
    isUserResponseDto(value.user)
  );
}
export function isUserResponseDto(value: unknown): value is UserResponseDto {
  return (
    isObject(value) &&
    typeof value.id === "string" &&
    (value.email === null || typeof value.email === "string") &&
    typeof value.firstName === "string" &&
    typeof value.lastName === "string" &&
    Array.isArray(value.roles) &&
    value.roles.every((item: unknown) => typeof item === "string")
  );
}

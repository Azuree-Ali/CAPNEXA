import { useNavigate } from "react-router-dom";
import { login } from "../api/auth.api";
import { signIn } from "../api/identity.api";
import { useAuthRequest } from "./useAuthRequest";
import { connected, setSession } from "@/lib/api/api-client";
import type { LoginValues, PreviewResult } from "../types/auth.types";
export function useLogin() {
  const navigate = useNavigate();
  return useAuthRequest(async (values: LoginValues, signal): Promise<PreviewResult> => {
    if (!connected) return login(values);
    // [BACKEND-API] Authenticate staff. POST /api/Identity/Auth/login. LoginDto -> AuthResponseDto.
    const session = await signIn({ email: values.email, password: values.password }, signal);
    signal.throwIfAborted();
    if (!session.user.roles.includes("Admin") && !session.user.roles.includes("Supervisor"))
      throw new Error("This portal requires a staff account.");
    setSession(session);
    navigate(session.user.roles.includes("Admin") ? "/admin/students" : "/supervisor/profile", {
      replace: true,
    });
    return { mode: "authenticated", message: "Signed in." };
  });
}

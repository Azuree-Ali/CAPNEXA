import { requestPasswordRecovery } from "../api/auth.api";
import { useAuthRequest } from "./useAuthRequest";
export function usePasswordRecovery() {
  return useAuthRequest(requestPasswordRecovery);
}

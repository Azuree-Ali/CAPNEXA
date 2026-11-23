import { login } from "../api/auth.api";
import { useAuthRequest } from "./useAuthRequest";
export function useLogin() {
  // TODO(BACKEND): بعد وصول AuthResult أضف الجلسة ثم وجّه حسب دور الخادم.
  // MFA لا يعامل كتسجيل دخول مكتمل. لا يوجد تحويل Dashboard وهمي الآن.
  return useAuthRequest(login);
}

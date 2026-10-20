import { validateEmail } from "./login.schema";
import type { FieldErrors, RecoveryValues } from "../types/auth.types";
export function validateRecovery(values: RecoveryValues): FieldErrors {
  return { email: validateEmail(values.email) };
}

import type { FieldErrors, LoginValues } from "../types/auth.types";
export function validateEmail(email: string): string | undefined {
  if (!email.trim()) return "Please enter your university email.";
  if (!email.includes("@")) return "Please include an '@' in the email address.";
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) return "Please enter a valid email address.";
  // BACKEND: لا نفترض نطاق الجامعة. صلاحية الحساب يتحقق منها الخادم.
}
export function validateLogin(values: LoginValues): FieldErrors {
  return {
    email: validateEmail(values.email),
    password: values.password.length === 0 ? "Please enter your password." : undefined,
  };
}
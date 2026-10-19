export interface LoginValues { email: string; password: string; rememberMe: boolean; }
export interface RecoveryValues { email: string; }
export type FieldErrors = Partial<Record<"email" | "password", string>>;
// BACKEND: هذه نتيجة العرض فقط. أضف أنواع user/roles/MFA من العقد الفعلي.
export interface PreviewResult { mode: "preview"; message: string; }
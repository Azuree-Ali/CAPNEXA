import type { LoginValues, PreviewResult, RecoveryValues } from "../types/auth.types";

// ========= TODO(BACKEND): ضع هنا الربط الذي سيرسله زميلك =========
// هذا الملف وحده يستقبل بيانات النماذج ثم يرسلها للـAPI.
// لا تكتب fetch داخل LoginPage أو ForgotPasswordPage.
// استورد postJson من ../../../lib/api/api-client بعد الاتفاق على عقد API.

export async function login(values: LoginValues): Promise<PreviewResult> {
  if (import.meta.env.VITE_AUTH_MODE !== "preview") {
    // TODO(BACKEND): استبدل هذا الخطأ بطلب endpoint الفعلي ثم تحقق من response.
    // مثال إرشادي فقط وليس عنوانًا معتمدًا:
    // const response = await postJson("ACTUAL_LOGIN_ENDPOINT", {
    //   email: values.email, password: values.password, rememberMe: values.rememberMe,
    // });
    // ثم عرّف AuthResult من OpenAPI وحدّث useLogin للجلسة/MFA/الدور.
    throw new Error("Backend login integration is not configured yet.");
  }
  void values; // لا نخزن ولا نسجل كلمة المرور في وضع المعاينة.
  return { mode: "preview", message: "UI preview only. No sign-in request was sent. Backend integration is pending." };
}
export async function requestPasswordRecovery(values: RecoveryValues): Promise<PreviewResult> {
  if (import.meta.env.VITE_AUTH_MODE !== "preview") {
    // TODO(BACKEND): ضع هنا endpoint استعادة كلمة المرور والـbody الفعلي.
    // await postJson("ACTUAL_RECOVERY_ENDPOINT", { email: values.email });
    // بعد استجابة ناجحة فقط، اعرض الرسالة المحايدة التالية لجميع الحسابات:
    // "If an eligible account exists, instructions will be sent."
    throw new Error("Backend password recovery integration is not configured yet.");
  }
  void values;
  return { mode: "preview", message: "UI preview only. No email was sent. Backend integration is pending." };
}
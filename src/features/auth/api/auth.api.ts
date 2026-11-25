import type { LoginValues, PreviewResult, RecoveryValues } from "../types/auth.types";
import { connected } from "@/lib/api/api-client";

export async function login(_values: LoginValues): Promise<PreviewResult> {
  void _values;
  return {
    mode: "preview",
    message: "Preview only. Open the demonstration workspace to explore fictional records.",
  };
}
export async function requestPasswordRecovery(_values: RecoveryValues): Promise<PreviewResult> {
  void _values;
  if (connected)
    throw new Error("Password recovery is unavailable. Contact your university administrator.");
  return { mode: "preview", message: "Preview only. No recovery email was sent." };
}

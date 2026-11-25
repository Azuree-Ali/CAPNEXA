import { recoveryCopy as copy } from "@/features/auth/content/recovery.content";
import { useState, type FormEvent } from "react";
import { ArrowRight, CheckCircle2, LoaderCircle } from "lucide-react";
import { EmailField } from "./EmailField";
import { FormFeedback } from "./FormFeedback";
import { validateRecovery } from "../schemas/password.schema";
import { usePasswordRecovery } from "../hooks/usePasswordRecovery";
export function ForgotPasswordForm() {
  const [email, setEmail] = useState("");
  const [fieldError, setFieldError] = useState<string>();
  const { submit, pending, error, result, clearFeedback } = usePasswordRecovery();
  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const next = validateRecovery({ email });
    setFieldError(next.email);
    if (next.email) {
      event.currentTarget.querySelector<HTMLInputElement>("[name=email]")?.focus();
      return;
    }
    await submit({ email: email.trim() });
  }
  return (
    <form
      onSubmit={handleSubmit}
      noValidate
      aria-busy={pending}
      className="auth-form recovery-form"
    >
      <p className="recovery-notice">
        <CheckCircle2 size={18} aria-hidden="true" />
        <span>{copy.previewNotice}</span>
      </p>
      <EmailField
        value={email}
        error={fieldError}
        disabled={pending}
        onChange={(v) => {
          setEmail(v);
          setFieldError(undefined);
          clearFeedback();
        }}
      />

      <FormFeedback error={error} message={result?.message} />
      <button type="submit" className="primary-button" disabled={pending}>
        {pending ? (
          <>
            <LoaderCircle size={18} className="spin" />
            {copy.sendingRequest}
          </>
        ) : (
          <>
            {copy.sendResetInstructions}
            <ArrowRight size={18} />
          </>
        )}
      </button>
    </form>
  );
}

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
    const next = validateRecovery({ email }); setFieldError(next.email);
    if (next.email) { event.currentTarget.querySelector<HTMLInputElement>("[name=email]")?.focus(); return; }
    await submit({ email: email.trim() });
  }
  return <form
  onSubmit={handleSubmit}
  noValidate
  aria-busy={pending}
  className="auth-form recovery-form"
>
    <EmailField value={email} error={fieldError} disabled={pending}
      onChange={v => { setEmail(v); setFieldError(undefined); clearFeedback(); }} />
    <p className="recovery-notice"><CheckCircle2 size={18} aria-hidden="true" /><span>If an eligible account exists, instructions will be sent.</span></p>
    <FormFeedback error={error} message={result?.message} />
    <button type="submit" className="primary-button" disabled={pending}>
      {pending ? <><LoaderCircle size={18} className="spin" />Sending request...</> : <>Send Reset Instructions<ArrowRight size={18} /></>}
    </button>
  </form>;
}
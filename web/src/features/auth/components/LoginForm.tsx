import { useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import { LoaderCircle } from "lucide-react";
import { StitchIcon } from "./StitchIcon";
import { EmailField } from "./EmailField";
import { PasswordField } from "./PasswordField";
import { FormFeedback } from "./FormFeedback";
import { useLogin } from "../hooks/useLogin";
import { validateLogin } from "../schemas/login.schema";
import type { FieldErrors, LoginValues } from "../types/auth.types";
export function LoginForm() {
  const [values, setValues] = useState<LoginValues>({ email: "", password: "", rememberMe: false });
  const [errors, setErrors] = useState<FieldErrors>({});
  const { submit, pending, error, result, clearFeedback } = useLogin();
  function update<K extends keyof LoginValues>(key: K, value: LoginValues[K]) {
    setValues((old) => ({ ...old, [key]: value }));
    if (key === "email" || key === "password") setErrors((old) => ({ ...old, [key]: undefined }));
    clearFeedback();
  }
  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const next = validateLogin(values);
    setErrors(next);
    const field = next.email ? "email" : next.password ? "password" : undefined;
    if (field) {
      event.currentTarget.querySelector<HTMLInputElement>(`[name="${field}"]`)?.focus();
      return;
    }
    await submit({ ...values, email: values.email.trim() });
    setValues((old) => ({ ...old, password: "" }));
  }
  return (
    <form onSubmit={handleSubmit} noValidate aria-busy={pending} className="auth-form">
      <EmailField
        value={values.email}
        onChange={(v) => update("email", v)}
        error={errors.email}
        disabled={pending}
      />
      <PasswordField
        value={values.password}
        onChange={(v) => update("password", v)}
        error={errors.password}
        disabled={pending}
      />
      <div className="form-options">
        <label className="checkbox-label">
          <input
            type="checkbox"
            checked={values.rememberMe}
            onChange={(e) => update("rememberMe", e.target.checked)}
            disabled={pending}
          />
          Remember me
        </label>
        <Link to="/forgot-password">Forgot password?</Link>
      </div>
      <FormFeedback error={error} message={result?.message} />
      <button type="submit" className="primary-button" disabled={pending}>
        {pending ? (
          <>
            <LoaderCircle size={18} className="spin" />
            Signing in...
          </>
        ) : (
          <>
            Sign In
            <StitchIcon name="arrow_forward" size={18} />
          </>
        )}
      </button>
    </form>
  );
}

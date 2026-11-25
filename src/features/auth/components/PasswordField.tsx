import { loginCopy as copy } from "@/features/auth/content/login.content";
import { useState } from "react";
import { StitchIcon } from "./StitchIcon";
interface Props {
  value: string;
  onChange: (value: string) => void;
  error?: string;
  disabled?: boolean;
}
export function PasswordField({ value, onChange, error, disabled }: Props) {
  const [visible, setVisible] = useState(false);
  return (
    <div className="form-field">
      <label htmlFor="password">{copy.password}</label>
      <div className={`input-wrap ${error ? "invalid" : ""}`}>
        <StitchIcon name="lock" size={18} />
        <input
          id="password"
          name="password"
          type={visible ? "text" : "password"}
          autoComplete="current-password"
          required
          placeholder={copy.separator2}
          value={value}
          onChange={(e) => onChange(e.target.value)}
          disabled={disabled}
          aria-invalid={!!error}
          aria-describedby={error ? "password-error" : undefined}
        />
        <button
          type="button"
          className="reveal-button"
          disabled={disabled}
          onClick={() => setVisible((v) => !v)}
          aria-label={visible ? copy.hidePassword : copy.showPassword}
          aria-pressed={visible}
        >
          <StitchIcon name={visible ? "visibility_off" : "visibility"} size={20} />
        </button>
      </div>
      {error && (
        <p className="field-error" id="password-error">
          {error}
        </p>
      )}
    </div>
  );
}

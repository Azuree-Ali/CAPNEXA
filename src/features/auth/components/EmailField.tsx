import { authFieldsCopy as copy } from "@/features/auth/content/authFields.content";
import { StitchIcon } from "./StitchIcon";
interface Props {
  value: string;
  onChange: (value: string) => void;
  error?: string;
  disabled?: boolean;
  id?: string;
}
export function EmailField({ value, onChange, error, disabled, id = "university-email" }: Props) {
  return (
    <div className="form-field">
      <label htmlFor={id}>{copy.universityEmail}</label>
      <div className={`input-wrap ${error ? "invalid" : ""}`}>
        <StitchIcon name="alternate_email" size={18} />
        <input
          id={id}
          name="email"
          type="email"
          autoComplete="username"
          inputMode="email"
          required
          placeholder={copy.eGJMorrisonInstitutionEdu}
          value={value}
          onChange={(e) => onChange(e.target.value)}
          disabled={disabled}
          aria-invalid={!!error}
          aria-describedby={error ? `${id}-error` : undefined}
        />
      </div>
      {error && (
        <p className="field-error" id={`${id}-error`}>
          {error}
        </p>
      )}
    </div>
  );
}

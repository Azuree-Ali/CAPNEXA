interface Props { error?: string; message?: string; }
export function FormFeedback({ error, message }: Props) {
  return <>{error && <p className="feedback error-feedback" role="alert">{error}</p>}
    {message && <p className="feedback preview-feedback" role="status">{message}</p>}</>;
}
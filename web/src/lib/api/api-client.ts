// TODO(BACKEND): هذا الملف خاص باتصال الـBackend الذي سيرسله زميلك.
// الاتفاق على Cookie أو Bearer وCSRF وCORS مطلوب قبل استدعاء هذه الدالة.
// لا تضع كلمة المرور أو token داخل localStorage ولا مفاتيح سرية في VITE_*.
export async function postJson(
  path: string,
  body: unknown,
  signal?: AbortSignal,
): Promise<unknown> {
  const baseUrl = import.meta.env.VITE_API_BASE_URL?.trim();
  if (!baseUrl) throw new Error("BACKEND: Configure VITE_API_BASE_URL first.");
  const url = new URL(baseUrl.replace(/\/$/, "") + "/" + path.replace(/^\//, ""));
  const response = await fetch(url, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    // TODO(BACKEND): Cookie auth may require credentials: "include" and a CSRF header.
    // TODO(BACKEND): Bearer auth requires the agreed Authorization/session flow.
    body: JSON.stringify(body),
    signal,
  });
  if (!response.ok) {
    if (response.status === 429) throw new Error("Too many requests. Please try again later.");
    if (response.status === 401) throw new Error("Unable to sign in. Check your credentials.");
    throw new Error("The request could not be completed. Please try again.");
  }
  if (response.status === 204) return undefined;
  return response.json();
}

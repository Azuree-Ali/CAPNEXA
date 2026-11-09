# Google and GitHub sign-in

The API owns both provider callbacks. ASP.NET Core validates the OAuth/OIDC state, correlation cookies, authorization response, and provider identity. The API finds or creates the Identity user, issues the normal access/refresh token response, then redirects to the configured Flutter callback with a base64url-encoded `AuthResponse` in the URL fragment as `#auth=...`. There is no separate exchange-code request. Flutter decodes the fragment, stores the tokens securely, and routes to the correct screen.

## Local callback URLs

These values are derived from `AppUrls:ApiBaseUrl`, the two provider callback paths, `AppUrls:FrontendBaseUrl`, and `Frontend:OAuthCallbackPath` in the checked-in appsettings and launch profile:

| Purpose | URL |
| --- | --- |
| API base URL | `https://localhost:7072` |
| Google callback | `https://localhost:7072/api/Identity/Auth/google/callback` |
| GitHub callback | `https://localhost:7072/api/Identity/Auth/github/callback` |
| Frontend callback | `http://localhost:5173/oauth/callback` |

Register the provider callback URLs exactly, including scheme, hostname, port, and path. In production, derive the callback from the deployed `AppUrls:ApiBaseUrl` and the configured `Auth:Google:CallbackPath` / `Auth:GitHub:CallbackPath`; register the resulting HTTPS URLs with the providers. Do not use a wildcard callback.

## Configuration keys

Set provider credentials with .NET User Secrets for local development, or the equivalent deployment secret store/environment variables. The exact environment variable names are:

| Configuration key | Environment variable | Purpose |
| --- | --- | --- |
| `Auth:Google:ClientId` | `Auth__Google__ClientId` | Google OAuth web client ID |
| `Auth:Google:ClientSecret` | `Auth__Google__ClientSecret` | Google OAuth web client secret |
| `Auth:GitHub:ClientId` | `Auth__GitHub__ClientId` | GitHub OAuth App client ID |
| `Auth:GitHub:ClientSecret` | `Auth__GitHub__ClientSecret` | GitHub OAuth App client secret |
| `AppUrls:ApiBaseUrl` | `AppUrls__ApiBaseUrl` | Public HTTPS API origin used for callback URLs |
| `AppUrls:FrontendBaseUrl` | `AppUrls__FrontendBaseUrl` | Frontend origin used for the post-login redirect |
| `Frontend:OAuthCallbackPath` | `Frontend__OAuthCallbackPath` | Relative frontend path, default `/oauth/callback` |
| `Frontend:OAuthCallbackUrl` | `Frontend__OAuthCallbackUrl` | Optional absolute Flutter deep-link callback URL, e.g. `capnexa://oauth/callback` |
| `Auth:Google:CallbackPath` | `Auth__Google__CallbackPath` | API callback path, default `/api/Identity/Auth/google/callback` |
| `Auth:GitHub:CallbackPath` | `Auth__GitHub__CallbackPath` | API callback path, default `/api/Identity/Auth/github/callback` |

The callback paths are optional and normally should stay at their defaults. The API requires an HTTPS `AppUrls:ApiBaseUrl`. It does not enable a provider scheme until both of that provider's credentials are set. No OAuth secret or client ID is committed in `appsettings.json`.

Example local setup from the repository root:

```powershell
$project = '.\CAPNEXAAPI\CAPNEXAAPI\CAPNEXAAPI.csproj'
dotnet user-secrets set 'Auth:Google:ClientId' '<Google client ID>' --project $project
dotnet user-secrets set 'Auth:Google:ClientSecret' '<Google client secret>' --project $project
dotnet user-secrets set 'Auth:GitHub:ClientId' '<GitHub client ID>' --project $project
dotnet user-secrets set 'Auth:GitHub:ClientSecret' '<GitHub client secret>' --project $project
```

Do not place secrets in frontend code, responses, source control, or logs.

## Register the providers

### Google

1. In Google Cloud Console, configure the consent screen and OAuth audience. Add test accounts while the app is in testing mode.
2. Create an OAuth client with application type **Web application**.
3. Add the exact Google callback URL above to **Authorized redirect URIs**.
4. Store the resulting client ID and secret in the API configuration keys above.

See [Google's web server OAuth guide](https://developers.google.com/identity/protocols/oauth2/web-server).

### GitHub

1. In GitHub, open **Settings → Developer settings → OAuth Apps** and create an OAuth App.
2. Set its homepage to the actual frontend base URL.
3. Set **Authorization callback URL** to the exact GitHub callback URL above.
4. Generate a client secret and store the ID and secret in the API configuration keys above.

See [GitHub's OAuth App setup guide](https://docs.github.com/en/apps/oauth-apps/building-oauth-apps/creating-an-oauth-app).

## Frontend flow and account outcomes

Open `/api/Identity/Auth/google/login` or `/api/Identity/Auth/github/login` in the system browser. On success, the API redirects to the configured Flutter callback with `#auth=<base64url-json>`. The decoded JSON matches the response from password login: access token, refresh token, expiry, and user. Parse the fragment in Flutter, remove it from the visible navigation state, and never log the fragment. Provider errors arrive as a safe `error` query value.

The API only creates a new external account when the provider returns a verified email. It does not link an existing password account just because the email matches. The frontend should handle `email_required` and `email_unverified` by guiding the user to the existing registration flow; `email_conflict` should guide them to sign in to the existing account. No external identity or tokens are issued for those cases.

External users receive the application's `Student` role from server-side policy and must complete their academic profile after first sign-in. Provider roles are ignored. Disabled accounts are rejected. Successful external sign-in returns the same auth response shape as password login.

## Manual verification

After configuring credentials and registering the callbacks:

1. Register with email/password, confirm the email link, then log in and verify the auth response.
2. Try password login before confirmation and verify it is rejected.
3. Request a confirmation resend and verify it sends the same confirmation link.
4. Start each provider flow and verify the exact API callback and Flutter callback work.
5. Decode the OAuth callback fragment and verify it contains the normal auth response and no exchange code.
6. Sign in again with the same provider account and confirm it resolves the stored Identity external login.
7. Try a provider email already used by a password account and confirm `email_conflict` without linking accounts.
8. Confirm external users have a Student role and an incomplete academic profile until they complete it through the student profile endpoint.
9. Verify refresh rotates the refresh token and returns a new access token.

Live provider sign-in requires configured credentials and provider-side callback registration; compilation alone does not verify those provider settings.

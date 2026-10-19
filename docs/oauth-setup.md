# Google and GitHub sign-in

The API owns both provider callbacks. ASP.NET Core validates the OAuth/OIDC state, correlation cookies, authorization response, and provider identity. The API finds or creates an ASP.NET Core Identity user by the provider's stable ID, then redirects the browser to the configured frontend callback with a short-lived exchange code. The frontend posts that code to `/api/Auth/exchange` to receive the application's access and refresh tokens.

The code is 256 bits of cryptographic randomness, stored by its SHA-256 hash, expires after two minutes, and can be consumed once. It is held in process memory, so run one API instance or use session-affinity routing for both the callback and `/exchange`. Restarting the API invalidates outstanding codes.

## Local callback URLs

These values are derived from `AppUrls:ApiBaseUrl`, the two provider callback paths, `AppUrls:FrontendBaseUrl`, and `Frontend:OAuthCallbackPath` in the checked-in appsettings and launch profile:

| Purpose | URL |
| --- | --- |
| API base URL | `https://localhost:7072` |
| Google callback | `https://localhost:7072/api/auth/google/callback` |
| GitHub callback | `https://localhost:7072/api/auth/github/callback` |
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
| `Auth:Google:CallbackPath` | `Auth__Google__CallbackPath` | API callback path, default `/api/auth/google/callback` |
| `Auth:GitHub:CallbackPath` | `Auth__GitHub__CallbackPath` | API callback path, default `/api/auth/github/callback` |

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

Navigate the browser to `/api/Auth/google/login` or `/api/Auth/github/login`. On success, the API redirects to `/oauth/callback#code=...`. Read the code from the URL fragment, remove the fragment from browser history, and POST `{ "code": "..." }` to `/api/Auth/exchange`. Keep the code out of analytics and logs. The API never puts its JWT or refresh token in a URL. Provider errors arrive as a safe `error` query value.

The API only creates a new external account when the provider returns a verified email. It does not link an existing password account just because the email matches. The frontend should handle `email_required` and `email_unverified` by guiding the user to the existing registration flow; `email_conflict` should guide them to sign in to the existing account. No external identity or JWT is issued for those cases. A separate authenticated account-linking feature can be added if product requirements call for it.

External users receive the application's `Student` role from server-side policy. Roles and privileges from provider claims are ignored. Disabled or unconfirmed accounts are rejected, and exchange token issuance uses the same JWT and refresh-token services as password login.

## Manual verification

After configuring credentials and registering the callbacks:

1. Sign in with an existing password account and confirm the response shape and refresh/revoke flow are unchanged.
2. Start each provider flow and verify the provider returns to the exact API callback, then to the configured frontend callback with only a fragment exchange code.
3. Exchange the code once and confirm the response contains the normal CAPNEXA access token and refresh token. Re-submit the code and confirm it is rejected.
4. Sign in again with the same provider account and confirm it resolves through the stored Identity external login.
5. Try a provider email already used by a password account and confirm `email_conflict` is returned without linking the accounts.
6. Test a provider account with no verified email and confirm the API redirects with a safe error and creates no external account.
7. Disable an external account or mark its email unconfirmed and confirm it cannot exchange a code for tokens.
8. Confirm provider claims do not change the user's application roles.

Live provider sign-in requires configured credentials and provider-side callback registration; compilation alone does not verify those provider settings.

# CAPNEXA Backend API Reference for Flutter

Flutter and Dio integration guide

This document describes the HTTP endpoints implemented by the CAPNEXA ASP.NET Core backend and compares them with the Flutter UI and API layer on the repository's `flutter` branch. Admin endpoints are for the web dashboard and are excluded from Flutter integration. The API uses JSON for most requests and responses, JWT bearer authentication for protected routes, and redirects for browser based OAuth/email confirmation flows. Replace the base URL below with the host reachable from the device or emulator; `localhost` inside an Android emulator refers to the emulator itself, not the development PC.

### Profile completion behavior

Email/password registration already requires student number, department, and academic year and creates the academic profile. Google/GitHub sign-up does not provide these fields, so after OAuth code exchange the student should call `GET /api/Student/Profile`. If `academicProfileComplete` is false, Flutter should collect the missing fields and call `POST /api/Student/Profile/complete` with `studentNumber`, `department`, `academicYear`, and optional `level`. The endpoint returns the updated profile. A second completion attempt returns `409` with `academic_profile_already_completed`; duplicate student numbers return `409 student_number_already_in_use`.

`Level` is optional; completion is based on student number, department, and academic year. Phone number is not part of the student profile request or response. ASP.NET Identity phone fields and database columns remain unchanged. At this backend version, profile completion is supported, but there are no other student feature endpoints in the original project to gate yet; apply a completion check to each future protected student feature route. The profile read and completion routes remain available during onboarding. Admin dashboard routes are separate.

## Flutter branch compatibility findings

The Flutter branch currently wires `ApiAuthService` to the login, registration, and current-user calls. Other feature screens/services are mock-backed in the service locator. The auth endpoints and DTOs currently disagree with the backend, so the live integration will fail until the Flutter route paths, payloads, and parsing are aligned.

| Flutter implementation | Backend contract | Result and required adjustment |
|---|---|---|
| Login path `/auth/login` | `POST /api/Auth/login` | Path mismatch. Use `/api/Auth/login`. Request fields `email` and `password` already match. |
| Login parser accepts `token` or `accessToken` | Auth response has `accessToken`, `tokenType`, `expiresInSeconds`, `refreshToken`, and nested `user` | Access token key is supported, but Flutter must also persist the refresh token and parse nested `user`; current code calls `/me` after login. |
| Register path `/auth/register` | `POST /api/Auth/register` | Path mismatch. Use `/api/Auth/register`. |
| Registration JSON: `fullName`, `universityId`, `email`, `department`, `academicYear`, `password` | `firstName`, `lastName`, `email`, `studentNumber`, `department`, `academicYear`, `password`, `confirmPassword` | Rename/split `fullName`, map university ID to `studentNumber`, and send `confirmPassword`. Backend registration sends a confirmation email and returns a message; it does not issue a token. Flutter currently tries immediate sign-in and routes to home, but login must wait until email confirmation. |
| Current user path `/auth/me`; model expects `id`, `fullName`, `universityId`, `email`, `department`, `academicYear` | No `/api/Auth/me` route exists. Student profile is `GET /api/Student/Profile`; it returns `userId`, `firstName`, `lastName`, `studentNumber`, `department`, `academicYear`, `email`, etc. | Replace path with `/api/Student/Profile` and map the actual response fields. `fullName` and `universityId` are not backend response fields. |
| Social login throws unavailable | `GET /api/Auth/google/login`, `/api/Auth/github/login`, then one-time code exchange | OAuth is implemented server-side but Flutter UI does not yet integrate it. Use a browser/system authentication flow and `POST /api/Auth/exchange` after receiving the callback code. |
| Flutter sends bearer token on every request when present | Backend requires bearer auth only for protected endpoints; auth login/register/exchange/refresh and OAuth entry are anonymous | Usually harmless for public requests, but ensure access token is attached to student profile and revoke calls. |
| Dio debug interceptor logs request and response bodies | Auth responses contain access and refresh tokens | Disable or redact body logging for auth/token endpoints to avoid writing credentials to logs. |

### Flutter authentication flow that matches this backend

1. `POST /api/Auth/register` with `firstName`, `lastName`, `email`, `studentNumber`, `department`, `academicYear`, `password`, and `confirmPassword`.
2. Show the registration message and ask the user to confirm email. Do not navigate to authenticated home or attempt login until confirmation.
3. `POST /api/Auth/login` with email/password; securely store both `accessToken` and `refreshToken` from the returned `AuthResponse`.
4. Call `GET /api/Student/Profile` with `Authorization: Bearer <accessToken>` and parse `StudentProfileDto`.
5. On access-token expiry, call `POST /api/Auth/refresh` with the current refresh token, then replace both stored tokens with the rotated pair.

## Base URL and conventions

Local API base URL: `https://localhost:7072` (see `AppUrls:ApiBaseUrl` and launch profile). Routes are case-insensitive in ASP.NET Core; examples use the route casing shown in the controller. Send `Content-Type: application/json` for JSON bodies. Authenticated requests use `Authorization: Bearer <accessToken>`. Dates are ISO 8601 values. Most errors are JSON objects containing `message`, or `code` and `message` for profile operations. `[ApiController]` validation failures generally return HTTP 400 with a validation-problem response.

In development, interactive API documentation is available through Scalar/OpenAPI as configured by the host.

## Endpoint index

| Method | Path | Auth | Purpose |
|---|---|---|---|
| POST | `/api/Auth/register` | No | Register a student and send email confirmation |
| GET | `/api/Auth/confirm-email` | No | Confirm email; redirects to frontend login |
| POST | `/api/Auth/login` | No | Password login |
| POST | `/api/Auth/exchange` | No | Exchange one-time OAuth code for tokens |
| POST | `/api/Auth/refresh` | No | Rotate refresh token and issue access token |
| POST | `/api/Auth/revoke` | Bearer | Revoke caller's refresh token |
| GET | `/api/Auth/google/login` | No | Begin Google OAuth in browser |
| GET | `/api/Auth/github/login` | No | Begin GitHub OAuth in browser |
| GET | `/api/Student/Profile` | Student bearer | Read own profile |
| PUT | `/api/Student/Profile` | Student bearer | Update name and phone |
| POST | `/api/Student/Profile/password` | Student bearer | Change or set password |
| POST | `/api/Student/Profile/complete` | Student bearer | Complete academic profile |
| POST | `/api/Student/Profile/email-change` | Student bearer | Request email change confirmation |
| GET | `/api/Student/Profile/confirm-email-change` | No | Display confirmation form from email link |
| POST | `/api/Student/Profile/confirm-email-change` | No | Submit email-change confirmation form |
| GET | `/WeatherForecast` | No | Sample weather endpoint |

## Web dashboard only

The following backend route is protected by the Admin role and is intended for the web dashboard. It is not included in the Flutter/mobile integration contract:

| Method | Path | Auth | Purpose |
|---|---|---|---|
| GET | `/api/Admin/Students/{userId}/profile` | Admin bearer | Read a student's profile |

## Authentication endpoints

### Register
`POST /api/Auth/register`

Request:
```json
{
  "firstName": "Amina",
  "lastName": "Hassan",
  "email": "amina@example.edu",
  "studentNumber": "20260042",
  "department": "Computer Science",
  "academicYear": "Year 3",
  "password": "ExamplePass123!",
  "confirmPassword": "ExamplePass123!"
}
```
First/last name are 2–100 characters; email is required and max 256; student number max 50; department max 100; academic year max 20; password 8–100 characters and must match confirmation. Success: `200 {"message":"Registration completed successfully..."}`. User must confirm via email before password login. `400` validation/Identity password policy failure; `409 {"message":"...already in use."}` for duplicate email or student number.

### Confirm registration email
`GET /api/Auth/confirm-email?userId=...&token=...`

Browser/email-link endpoint. Redirects to `{FrontendBaseUrl}/login?confirmed=true|false`. Do not call from Dio expecting JSON; launch the URL in a browser or handle the redirect as appropriate.

### Password login
`POST /api/Auth/login`
```json
{"email":"amina@example.edu","password":"ExamplePass123!"}
```
Success `200` returns `AuthResponse` (shape below). `401` means invalid credentials or disabled account. `400` includes unconfirmed email or model validation failure.

### Exchange OAuth code
`POST /api/Auth/exchange`
```json
{"code":"<code-from-frontend-url-fragment>"}
```
Success: `200 AuthResponse`. `400` if invalid, expired, already consumed, or user cannot sign in. Code expires after two minutes and is single use. OAuth redirects the browser to `/oauth/callback#code=...`; read the URL fragment and post it here. The API keeps JWTs out of URLs.

### Refresh tokens
`POST /api/Auth/refresh`
```json
{"refreshToken":"<refresh-token>"}
```
Success: `200 AuthResponse` with a rotated refresh token. Replace both stored tokens. `401` for invalid, expired, or reused tokens.

### Revoke refresh token
`POST /api/Auth/revoke` (Bearer required)
```json
{"refreshToken":"<refresh-token>"}
```
Success `204 No Content`; `400` when token is not found or already revoked; `401` if bearer token is missing/invalid. Revoke is scoped to the authenticated user.

### OAuth browser sign-in
Open `GET /api/Auth/google/login` or `GET /api/Auth/github/login` in a browser/system auth session. These routes issue redirects and are not ordinary Dio JSON calls. Provider callbacks are handled by the API. On success, frontend receives the one-time code fragment and exchanges it as above. Safe frontend errors may include `email_conflict`, `email_required`, `email_unverified`, `github_profile_failed`, and `oauth_failed`. `email_conflict` means an account already exists for that email; automatic linking is intentionally disallowed. Provider app credentials and exact callback URLs must be configured server-side.

## Student profile endpoints

All routes below require a valid JWT with the Student role unless noted otherwise.

### Get own profile
`GET /api/Student/Profile`

Success `200 StudentProfile`. Errors: `401` unauthorized; `403` disabled/not permitted; `404` user not found.

### Update own profile
`PUT /api/Student/Profile`
```json
{"firstName":"Amina","lastName":"Hassan","phoneNumber":"+201001234567"}
```
First/last names required, 2–100 chars. Phone optional, max 32. Success `200 StudentProfile`. Changing phone resets `phoneNumberConfirmed` to false. Errors include `400 profile_update_failed`, `401`, `403`, `404`.

### Change or set password
`POST /api/Student/Profile/password`
```json
{"currentPassword":"OldPass123!","newPassword":"NewPass123!","confirmPassword":"NewPass123!"}
```
For accounts that already have a password, currentPassword is required. OAuth-only users may omit it when setting their first password, but their email must be confirmed. New password length 8–100 and confirmation must match. Success `204 No Content`; tokens/refresh tokens are revoked after success, so sign in again. Errors: `400 current_password_required`, `current_password_incorrect`, `password_policy_failed`; `403 email_not_confirmed` or account disabled; `401` unauthorized.

### Complete academic profile
`POST /api/Student/Profile/complete`
```json
{"studentNumber":"20260042","department":"Computer Science","academicYear":"Year 3","level":"Level 3"}
```
Student number required max 50; department required max 100; academicYear required max 20; level optional max 50. Success `200 StudentProfile`. `409 academic_profile_already_completed` or `student_number_already_in_use`; `400` invalid payload; `401` or `403` as applicable.

### Request email change
`POST /api/Student/Profile/email-change`
```json
{"newEmail":"amina.new@example.edu"}
```
Success `200 {"code":"email_change_confirmation_sent","message":"..."}`. If same as current address, success uses `email_unchanged`. The new address is not applied until confirmation. `409 email_already_in_use`; `503 email_service_unavailable`; `401` unauthorized. This route is rate limited.

### Show email-change confirmation page
`GET /api/Student/Profile/confirm-email-change?userId=...&newEmail=...&token=...`

The email link opens a small HTML confirmation page. It is intentionally not a JSON endpoint; the page submits a form to the POST endpoint. Invalid/incomplete query returns `400` JSON `{code,message}`.

### Confirm email change
`POST /api/Student/Profile/confirm-email-change`

The HTML form posts `application/x-www-form-urlencoded` fields `UserId`, `NewEmail`, `Token`. On success: `200 {"code":"email_change_confirmed","message":"..."}`. `400 email_change_token_invalid_or_expired` or `email_change_failed`; `404 user_not_found`; `409 email_already_in_use`; disabled account returns `403`. This is designed for browser form submission, not a typical mobile JSON call. If implementing native confirmation, send form URL-encoded content.

## Sample weather endpoint

`GET /WeatherForecast`

Unauthenticated sample endpoint. Returns an array of forecasts with `date`, `temperatureC`, `temperatureF`, and `summary`. This is scaffold/sample functionality rather than a business endpoint.

## Response models

### AuthResponse
```json
{
  "accessToken":"<jwt>",
  "tokenType":"Bearer",
  "expiresInSeconds":3600,
  "refreshToken":"<opaque-refresh-token>",
  "user":{"id":"<id>","email":"amina@example.edu","firstName":"Amina","lastName":"Hassan","roles":["Student"]}
}
```
Expiry is server-configured; use `expiresInSeconds` rather than hard-coding a lifetime. Store refresh tokens securely (for example, platform secure storage). Never log tokens.

### StudentProfile
Includes `userId`, nullable `email`, `emailConfirmed`, `firstName`, `lastName`, nullable `phoneNumber`, `phoneNumberConfirmed`, `isActive`, `createdAt`, `academicProfileComplete`, nullable `studentNumber`, `department`, `academicYear`, `level`, and `teams` array. A team contains `teamId`, `name`, `teamCode`, `isLeader`, `isCreator`, nullable `joinedAt`, and `projects`. Each project contains `projectId`, `title`, `status`, `createdAt`.

### Error shape
Profile errors use `{ "code": "...", "message": "..." }`. Auth actions usually use `{ "message": "..." }`. Validation errors can use ASP.NET Core's validation-problem format. Check the status code and parse fields defensively.

## Flutter and Dio setup

Add Dio to `pubspec.yaml`, then create one client with the API origin as `baseUrl`. For Android emulator access to a local development API, use the host machine's emulator bridge address (commonly `10.0.2.2`) and ensure the API binds to a reachable interface and the development certificate is trusted. A physical device needs the computer's LAN IP and network access. Production should use HTTPS.

```dart
final dio = Dio(BaseOptions(
  baseUrl: 'https://10.0.2.2:7072', // Android emulator example; adjust to your setup
  connectTimeout: const Duration(seconds: 15),
  receiveTimeout: const Duration(seconds: 15),
  headers: {'Accept': 'application/json'},
));
```

Attach the access token to protected requests:
```dart
dio.options.headers['Authorization'] = 'Bearer $accessToken';
final response = await dio.get('/api/Student/Profile');
```
Prefer a Dio interceptor to add the token and handle a single refresh attempt after `401`. Avoid retry loops; refresh tokens rotate, so persist the newly returned refresh token immediately. Do not disable TLS certificate validation in production. For OAuth, use a system browser/app-auth flow with deep-link return handling rather than embedding provider secrets or attempting the full redirect flow as JSON.

Example login call:
```dart
final response = await dio.post('/api/Auth/login', data: {
  'email': email,
  'password': password,
});
final accessToken = response.data['accessToken'] as String;
final refreshToken = response.data['refreshToken'] as String;
```

Dio reports non-2xx responses as `DioException` by default. Read `e.response?.statusCode` and `e.response?.data`; network/TLS failures may have no response. Do not treat every failure as an authentication error.

## Rate limiting and deployment notes

Authentication controller endpoints are assigned the `auth` rate-limit policy; email-change request is also rate limited. Limits are deployment configuration and should be treated as possible `429 Too Many Requests`. Confirm the server API base URL and provider callback configuration for each environment. Localhost URLs must be adapted for Android emulator, iOS simulator, physical devices, and deployed environments.

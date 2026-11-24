# CAPNEXA Backend API and Flutter Integration Guide

This document is the integration guide for the Flutter developer. It covers the implemented student-facing API and the authentication flows needed by Flutter, with Dio examples. Admin routes are for the web dashboard and are intentionally excluded. Planned backend features are listed separately and must not be called until implemented.

## 1. API basics

- Local API origins from the current launch profile: `https://localhost:7072` and `http://localhost:5001`.
- Android emulator: use `http://10.0.2.2:5001` for local development, or configure a trusted development HTTPS certificate. Do not disable certificate validation in a production build.
- Send JSON with `Content-Type: application/json` and `Accept: application/json`.
- Protected requests use `Authorization: Bearer <accessToken>`.
- ASP.NET Core routes are case-insensitive. Paths below show their controller route shape.
- Authentication endpoints are under `/api/Identity/Auth`, because the controller's area is `Identity`.
- The backend uses SQL Server and EF Core. Identity and academic data are persisted there. OAuth returns the normal auth response directly to the configured Flutter callback fragment; there is no exchange-code endpoint.

## 2. Flutter sign-in and onboarding flow

### Email and password registration

1. Flutter collects first name, last name, email, student number, department, academic year, password, and password confirmation.
2. Flutter calls `POST /api/Identity/Auth/register`.
3. Backend creates the Identity user, Student role, and academic profile, then sends an email-confirmation link.
4. Flutter shows the confirmation instruction. Registration does not return tokens, so do not route to authenticated screens yet.
5. User opens the email link. The API confirms the address and redirects to the frontend login page with `confirmed=true` or `false`.
6. After confirmation, Flutter calls login, securely stores both tokens, then calls `GET /api/Student/Profile`.
7. Since regular registration already includes the required academic data, `academicProfileComplete` should be true. Do not send the student through completion again.

### Google or GitHub sign-in

1. Flutter opens the provider login URL in a system browser or OAuth-capable authentication session; it is a redirect flow, not a JSON Dio call.
2. Provider returns to the API callback. The backend creates or finds the user, assigns the Student role for new external accounts, and issues the normal auth response.
3. API redirects to the configured Flutter callback with `#auth=<base64url-json>`; Flutter decodes it and securely stores `accessToken` and `refreshToken`.
4. Flutter calls `GET /api/Student/Profile`. If `academicProfileComplete` is false, display the academic onboarding form.
5. Submit `studentNumber`, `department`, `academicYear`, and optional `level` to `POST /api/Student/Profile/complete`.
6. Backend returns the updated profile. Flutter can continue to the student app using that response.

Required completion fields are exactly student number, department, and academic year. `Level` is optional. An incomplete student can retrieve and complete their own profile and use the account-maintenance actions in the Profile controller. The profile response is the source of truth for the onboarding decision.

## 3. Current implemented endpoints

### Identity and authentication

| Method | Route | Authentication | Result |
|---|---|---|---|
| POST | `/api/Identity/Auth/register` | Public | Creates password account and academic profile; sends confirmation email |
| POST | `/api/Identity/Auth/resend-confirmation` | Public | Resends the email confirmation link without disclosing whether an account exists |
| GET | `/api/Identity/Auth/confirm-email?userId=...&token=...` | Public/browser | Confirms email and redirects to frontend login |
| POST | `/api/Identity/Auth/login` | Public | Returns access token, refresh token, and user |
| POST | `/api/Identity/Auth/refresh` | Public | Rotates refresh token and returns new auth response |
| POST | `/api/Identity/Auth/forgot-password` | Public | Sends 6-digit OTP to email for password reset |
| POST | `/api/Identity/Auth/reset-password` | Public | Validates OTP and resets password |
| GET | `/api/Identity/Auth/google/login` | Public/browser | Starts Google OAuth |
| GET | `/api/Identity/Auth/github/login` | Public/browser | Starts GitHub OAuth |

### Student profile

All protected routes require a valid bearer token and Student role, except the email-change confirmation routes, which are public browser/form endpoints.

| Method | Route | Authentication | Result |
|---|---|---|---|
| GET | `/api/Student/Profile` | Student bearer | Returns own profile and academic completion state |
| PUT | `/api/Student/Profile` | Student bearer | Updates first and last name |
| POST | `/api/Student/Profile/password` | Student bearer | Changes password or adds one to confirmed external account |
| POST | `/api/Student/Profile/complete` | Student bearer | Creates academic profile for external account |
| POST | `/api/Student/Profile/email-change` | Student bearer | Sends confirmation link to new email |
| GET | `/api/Student/Profile/confirm-email-change?userId=...&newEmail=...&token=...` | Public/browser | Displays confirmation form |
| POST | `/api/Student/Profile/confirm-email-change` | Public form | Confirms email change; expects form URL encoding |
| POST | `/api/Student/Teams` | Student bearer | Create team; creator is leader |
| GET | `/api/Student/Teams/me` | Student bearer | Read own team and members |
| POST | `/api/Student/Teams/join` | Student bearer | Join by invite code |
| POST | `/api/Student/Teams/leave` | Student bearer | Leave team as a non-leader |
| GET | `/api/Student/ProjectProposals` | Student bearer | List proposal attempts for own team |
| POST | `/api/Student/ProjectProposals` | Student bearer, any team member | Create a draft proposal |
| PUT | `/api/Student/ProjectProposals/{projectId}` | Student bearer, any team member | Edit a draft proposal |
| POST | `/api/Student/ProjectProposals/{projectId}/submit` | Student bearer, team leader | Submit a draft for review |
| GET | `/api/Student/Dashboard` | Student bearer | Get student dashboard summary |

### Admin web dashboard only — not for Flutter

All `/api/Admin/*` endpoints are for the web dashboard. Do not call them from the Flutter student application.
### Admin student management

**ADMIN WEB DASHBOARD ONLY — NOT FOR FLUTTER STUDENT FLOWS**

All routes in this section require the Admin role (`Authorization: Bearer <adminAccessToken>`). They are intended for the Admin web dashboard. The Flutter student application must not call these endpoints.

| Method | Route | Authentication | Result |
|---|---|---|---|
| GET | `/api/Admin/Students` | Admin bearer | Lists and searches student accounts with filters and pagination |
| GET | `/api/Admin/Students/{userId}/profile` | Admin bearer | Returns the full profile for one student |
| PUT | `/api/Admin/Students/{userId}/status` | Admin bearer | Activates or deactivates a student account |

#### List students

`GET /api/Admin/Students?search=...&department=...&academicYear=...&isActive=true&page=1&pageSize=20`

Query parameters:

| Name | Type | Required | Rules |
|---|---|---|---|
| `search` | string | No | Case-insensitive partial match on first name, last name, email, or student number |
| `department` | string | No | Exact match on the student's department |
| `academicYear` | string | No | Exact match on the student's academic year |
| `isActive` | boolean | No | Filters active (`true`) or disabled (`false`) accounts |
| `page` | integer | No | 1-based page number; defaults to `1`; must be at least `1` |
| `pageSize` | integer | No | Defaults to `20`; must be between `1` and `100` |

Only accounts in the Student role are returned. Results are ordered newest first (`createdAt` descending, then `userId` ascending) and are paginated in the database. The total count is computed before pagination.

Success is `200`:

```json
{
  "items": [
    {
      "userId": "6f1a...",
      "email": "amina@example.edu",
      "firstName": "Amina",
      "lastName": "Hassan",
      "studentNumber": "20260042",
      "department": "Computer Science",
      "academicYear": "Year 3",
      "academicProfileComplete": true,
      "emailConfirmed": true,
      "isActive": true,
      "createdAt": "2026-10-01T12:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1,
  "totalPages": 1
}
```

Invalid pagination returns `400`:

```json
{"code":"invalid_pagination","message":"Page must be at least 1 and pageSize must be between 1 and 100."}
```

#### Student profile

`GET /api/Admin/Students/{userId}/profile`

Returns the same profile contract as the Flutter student profile endpoint, including teams and projects, for the requested student. Only accounts in the Student role are eligible.

Success is `200`. Unknown or non-student `userId` returns `404`:

```json
{"code":"student_not_found","message":"The student was not found."}
```

An unexpected backend failure returns `500` with `{"code":"profile_operation_failed","message":"..."}`.

#### Activate or deactivate a student

`PUT /api/Admin/Students/{userId}/status`

```json
{"isActive":false}
```

The request is idempotent: setting the current state succeeds without further changes. Only accounts in the Student role can be targeted. Setting `isActive` to `false` also revokes all of that student's refresh tokens, so existing sessions can no longer refresh. Reactivating the account does not restore revoked tokens; the student must sign in again.

Success is `200`:

```json
{"userId":"6f1a...","isActive":false}
```

Errors:

- `400 {"code":"invalid_status_payload","message":"The isActive value is required."}` when `isActive` is missing.
- `404 {"code":"student_not_found","message":"The student was not found."}` when the `userId` is unknown or not a Student.
- `500 {"code":"student_status_update_failed","message":"..."}` when the update cannot be persisted.

A disabled student is rejected from login, refresh, and OAuth token issuance, and protected Student endpoints return `403`:

```json
{"code":"account_disabled","message":"This account has been disabled."}
```

### Sample endpoint

`GET /WeatherForecast` is scaffold/sample functionality, not a CAPNEXA business feature.

## 4. Request and response details

### Register

`POST /api/Identity/Auth/register`

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

Success is `200` with `message` and `emailConfirmationSent`. If SMTP is temporarily unavailable, the account remains created and the response has `emailConfirmationSent: false`; Flutter should show the resend-confirmation option instead of retrying registration. Duplicate email or student number returns `409`. Validation or password-policy errors return `400`.

### Resend registration confirmation

`POST /api/Identity/Auth/resend-confirmation`

```json
{"email":"amina@example.edu"}
```

Returns a generic `200` message whether or not the address belongs to an unconfirmed account. SMTP failure returns `503`; the user can try again later.

### Login

`POST /api/Identity/Auth/login`

```json
{"email":"amina@example.edu","password":"ExamplePass123!"}
```

Success returns:

```json
{
  "accessToken": "<jwt>",
  "tokenType": "Bearer",
  "expiresInSeconds": 900,
  "refreshToken": "<opaque-refresh-token>",
  "user": {
    "id": "<identity-id>",
    "email": "amina@example.edu",
    "firstName": "Amina",
    "lastName": "Hassan",
    "roles": ["Student"]
  }
}
```

Store both tokens securely. Incorrect credentials or an inactive account returns `401`; unconfirmed email returns `400`. These error responses contain a `message` and no tokens.

### OAuth callback and refresh

OAuth is a browser/deep-link redirect, not a Dio JSON request. On success, the configured callback receives `#auth=<base64url-encoded-json>`. Decode it to get the same camelCase `AuthResponse` returned by password login. Remove the fragment from the visible navigation state and never log it. Set `Frontend:OAuthCallbackUrl` to the Flutter app's registered deep link; if omitted, the local web callback is used.

Refresh request: `POST /api/Identity/Auth/refresh` with `{"refreshToken":"<current-refresh-token>"}`. On success, replace both stored tokens; refresh tokens rotate. Invalid, expired, or reused token returns `401`.

### Get student profile

`GET /api/Student/Profile`

Example response:

```json
{
  "userId": "<identity-id>",
  "email": "amina@example.edu",
  "emailConfirmed": true,
  "firstName": "Amina",
  "lastName": "Hassan",
  "isActive": true,
  "createdAt": "2026-11-09T12:00:00Z",
  "academicProfileComplete": false,
  "studentNumber": null,
  "department": null,
  "academicYear": null,
  "level": null,
  "teams": []
}
```

The example shows an external account before onboarding. Completed academic values are returned when available. Phone number is deliberately absent from student profile request and response DTOs. ASP.NET Identity's phone properties and database fields are preserved and are not modified by profile API updates.

### Complete academic profile

`POST /api/Student/Profile/complete`

```json
{
  "studentNumber": "20260042",
  "department": "Computer Science",
  "academicYear": "Year 3",
  "level": "Level 3"
}
```

`level` can be omitted or null. Success is `200` with the updated student profile, including `academicProfileComplete: true`. Duplicate student number returns `409 student_number_already_in_use`. A second completion attempt returns `409 academic_profile_already_completed`; completed data is not edited through this onboarding endpoint. Invalid or missing required fields return `400`.

### Update basic profile

`PUT /api/Student/Profile`

```json
{"firstName":"Amina","lastName":"Hassan"}
```

Only first and last names are editable here. Academic fields are not changed through this endpoint, and phone number is not accepted by this profile API.

### Password maintenance

`POST /api/Student/Profile/password`

```json
{"currentPassword":"OldPass123!","newPassword":"NewPass123!","confirmPassword":"NewPass123!"}
```

For an existing password account, `currentPassword` is required. A confirmed external account can set its first password without `currentPassword`. Success is `204`; changing password revokes refresh tokens, so sign in again. Invalid current password or password policy returns `400`.

### Email change

Request: `POST /api/Student/Profile/email-change` with `{"newEmail":"new@example.edu"}`. Backend emails a confirmation link; the email is not changed until confirmation. The linked page displays a form and posts URL-encoded fields `UserId`, `NewEmail`, `Token` to `/api/Student/Profile/confirm-email-change`.

## 4.5. Password reset (forgot password)

All password reset endpoints are public (no authentication required). Rate limited to 10 requests per minute per IP.

### Request password reset OTP

`POST /api/Identity/Auth/forgot-password`

```json
{
  "email": "student@university.edu"
}
```

**Response (always `200` — generic to prevent email enumeration):**

```json
{
  "message": "If an account with this email exists, a password reset OTP has been sent."
}
```

**Validation errors (`400`):**

```json
{"message": "Email is required."}
{"message": "Invalid email address."}
```

**Service unavailable (`503`):**

```json
{"message": "The OTP email could not be sent. Try again later."}
```

> **Security:** The response is identical whether the email exists or not. A 6-digit OTP is sent via email, valid for 10 minutes, single-use.

### Reset password with OTP

`POST /api/Identity/Auth/reset-password`

```json
{
  "email": "student@university.edu",
  "otp": "123456",
  "newPassword": "NewSecurePass123!",
  "confirmPassword": "NewSecurePass123!"
}
```

**Success (`200`):**

```json
{
  "message": "Your password has been reset successfully. You can now log in with your new password."
}
```

**Errors (`400`):**

| Error | Message |
|-------|---------|
| Invalid/expired OTP | `{"message": "Invalid or expired OTP. Request a new one."}` |
| OTP expired | `{"message": "OTP has expired. Request a new one."}` |
| Wrong OTP | `{"message": "Invalid OTP. Please try again."}` |
| Mismatched passwords | `{"message": "Password and confirmation password do not match."}` |
| Weak password | `{"message": "password_policy_failed"}` |
| OTP format invalid | `{"message": "OTP must be 6 digits."}` |

> **After reset:** All refresh tokens for the user are revoked. The user must log in again with the new password.

## 5. Team management

All team routes require a Student bearer token, a completed academic profile, and an active account. The maximum is five members including the leader. Each student may belong to one team at a time. There is no academic-year restriction. The backend creates the invite code and sets the creator as leader; do not send `teamId`, `maxMembers`, or `isLeader` in create/join requests.

### Create a team

`POST /api/Student/Teams`

```json
{"name":"Team Alpha"}
```

`name` is required (2–150 characters). Success is `200` with a `TeamDto`; the creator is a member with `isLeader: true`.

### Get my team and members

`GET /api/Student/Teams/me`

Success is `200` with this shape:

```json
{
  "teamId": 12,
  "name": "Team Alpha",
  "teamCode": "A1B2C3D4E5",
  "maxMembers": 5,
  "memberCount": 2,
  "members": [
    {"userId":"<identity-id>","firstName":"Amina","lastName":"Hassan","studentNumber":"20260042","isLeader":true,"joinedAt":"2026-11-09T12:00:00Z"},
    {"userId":"<identity-id-2>","firstName":"Omar","lastName":"Ali","studentNumber":"20260043","isLeader":false,"joinedAt":"2026-11-09T12:15:00Z"}
  ]
}
```

The leader is listed first. A student without a team receives `404 team_not_found`.

### Join a team

`POST /api/Student/Teams/join`

```json
{"teamCode":"A1B2C3D4E5"}
```

Success is `200` with the updated `TeamDto`. Possible errors: `403 profile_completion_required`, `403 account_disabled`, `404 team_not_found`, `409 student_already_in_team`, and `409 team_full`.

### Leave a team

`POST /api/Student/Teams/leave`

No request body. A non-leader leaves successfully with `204 No Content`. A leader receives `409 team_leader_cannot_leave`; leadership transfer is not implemented. A student without a team receives `404 team_not_found`.

## 6. Project proposals

All proposal endpoints require a Student bearer token, a completed academic profile, an active account, and team membership. Every team has one active proposal at a time. Any team member can create or edit the team's draft, and all members can view proposal attempts. The leader alone can submit the draft for review. A rejected attempt remains in history with its rejection reason; any team member can create a new draft from scratch. Submitted proposals cannot be edited through the student API.

### List proposal attempts

`GET /api/Student/ProjectProposals`

Returns `200` with an array, newest first. Each proposal contains `projectId`, `teamId`, `title`, `description`, `status`, `rejectionReason`, `createdAt`, and `submittedAt`. If the student has no team, the API returns `409 team_required`.

```json
[
  {"projectId":7,"teamId":12,"title":"Campus Helper","description":"An app for finding campus services and announcements.","status":"Draft","rejectionReason":null,"createdAt":"2026-11-09T12:00:00Z","submittedAt":null}
]
```

Known statuses are `Draft`, `PendingReview`, `Rejected`, and `Approved`. Review endpoints are part of future Admin web dashboard work; the student API does not currently accept or reject proposals.

### Create or edit a draft

Create: `POST /api/Student/ProjectProposals`

Edit: `PUT /api/Student/ProjectProposals/{projectId}`

Both use this body; title is required and 3–250 characters, description is required and 10–4000 characters:

```json
{"title":"Campus Helper","description":"A mobile application that helps students find campus services and announcements."}
```

Success is `200` with the created or updated proposal. Any team member may create/edit a draft belonging to their own team. `409 active_proposal_exists` means the team already has a non-rejected proposal. A proposal belonging to another team returns `404 proposal_not_found`; an existing proposal that is no longer a draft returns `409 proposal_not_editable`.

### Submit a proposal

`POST /api/Student/ProjectProposals/{projectId}/submit`

No request body. Only the team leader can submit a complete draft. Success is `200` with status `PendingReview` and a `submittedAt` value. A non-leader receives `403 team_leader_required`. A rejected proposal cannot be resubmitted; create a new proposal instead.

## 7. Student dashboard summary

All dashboard endpoints require a Student bearer token, an active account, and a completed academic profile. The dashboard aggregates the student's project journey in one response: team membership, proposal status, project details (if approved), milestones, progress, and upcoming deadlines.

### Get dashboard summary

`GET /api/Student/Dashboard`

Success is `200` with this shape:

```json
{
  "team": {
    "teamId": 12,
    "name": "Team Alpha",
    "teamCode": "A1B2C3D4E5",
    "maxMembers": 5,
    "memberCount": 3,
    "leader": {
      "userId": "<identity-id>",
      "firstName": "Amina",
      "lastName": "Hassan",
      "studentNumber": "20260042"
    },
    "members": [
      {"userId":"<identity-id>","firstName":"Amina","lastName":"Hassan","studentNumber":"20260042","isLeader":true,"joinedAt":"2026-11-09T12:00:00Z"},
      {"userId":"<identity-id-2>","firstName":"Omar","lastName":"Ali","studentNumber":"20260043","isLeader":false,"joinedAt":"2026-11-09T12:15:00Z"},
      {"userId":"<identity-id-3>","firstName":"Leila","lastName":"Mohamed","studentNumber":"20260044","isLeader":false,"joinedAt":"2026-11-09T13:00:00Z"}
    ]
  },
  "proposal": {
    "projectId": 7,
    "title": "Campus Helper",
    "status": "Approved",
    "rejectionReason": null,
    "createdAt": "2026-11-09T12:00:00Z",
    "submittedAt": "2026-11-10T10:00:00Z"
  },
  "project": {
    "projectId": 7,
    "title": "Campus Helper",
    "status": "Approved",
    "createdAt": "2026-11-09T12:00:00Z",
    "supervisorId": 3,
    "supervisorName": "Dr. Sarah Ahmed"
  },
  "milestones": [
    {
      "milestoneId": 1,
      "title": "Requirements Analysis",
      "description": "Gather and document requirements",
      "dueDate": "2026-12-01T23:59:59Z",
      "order": 1,
      "status": "Completed",
      "createdAt": "2026-11-11T09:00:00Z"
    },
    {
      "milestoneId": 2,
      "title": "Design Phase",
      "description": "Create system architecture and UI mockups",
      "dueDate": "2026-12-15T23:59:59Z",
      "order": 2,
      "status": "InProgress",
      "createdAt": "2026-11-11T09:00:00Z"
    },
    {
      "milestoneId": 3,
      "title": "Implementation",
      "description": "Build core features",
      "dueDate": "2027-01-15T23:59:59Z",
      "order": 3,
      "status": "Pending",
      "createdAt": "2026-11-11T09:00:00Z"
    }
  ],
  "progressPercentage": 33,
  "upcomingDeadlines": [
    {
      "milestoneId": 2,
      "title": "Design Phase",
      "dueDate": "2026-12-15T23:59:59Z",
      "order": 2
    },
    {
      "milestoneId": 3,
      "title": "Implementation",
      "dueDate": "2027-01-15T23:59:59Z",
      "order": 3
    }
  ]
}
```

**Behavior by student state:**

| Student state | Response |
|---|---|
| No team | `team: null`, `proposal: null`, `project: null`, `milestones: []`, `progressPercentage: 0`, `upcomingDeadlines: []` |
| Has team, no proposal | `team: {...}`, `proposal: null`, `project: null`, `milestones: []`, `progressPercentage: 0`, `upcomingDeadlines: []` |
| Has team, rejected proposal | `team: {...}`, `proposal: {status: "Rejected", rejectionReason: "..."}`, `project: null`, `milestones: []`, `progressPercentage: 0`, `upcomingDeadlines: []` |
| Has team, approved proposal | `team: {...}`, `proposal: {...}`, `project: {...}`, `milestones: [...]`, `progressPercentage: N`, `upcomingDeadlines: [...]` |

**Progress calculation:** `completed milestones / total milestones * 100` (integer division). Returns `0` when there are no milestones. A milestone is considered **completed** when its `Status` field equals `"Completed"`. Other status values (`"Pending"`, `"InProgress"`, etc.) do not count toward progress.

**Upcoming deadlines:** Milestones with a `DueDate` that are not completed, ordered by due date ascending.

### Errors

- `403 profile_completion_required` — academic profile incomplete
- `403 account_disabled` — account is disabled

## 8. Errors Flutter should handle

Business errors use this shape:

```json
{"code":"profile_completion_required","message":"Complete your academic profile before using teams."}
```

- `400`: invalid request or validation failure. With `[ApiController]`, validation failures may use a `ValidationProblemDetails` body containing an `errors` map.
- `401`: missing/invalid access token, incorrect login credentials, or invalid/expired/reused refresh token.
- `403`: incomplete academic profile (`profile_completion_required`), disabled account (`account_disabled`), or leader-only operation (`team_leader_required`).
- `404`: requested team or proposal was not found.
- `409`: conflict such as `team_full`, `student_already_in_team`, `team_required`, `active_proposal_exists`, `proposal_not_editable`, duplicate email/student number, or already-completed profile.
- `503`: confirmation email service unavailable.

Auth endpoints commonly return `{ "message": "..." }`; team and proposal business errors return `{ "code": "...", "message": "..." }`. Check the response `code` as well as the HTTP status.

### Password reset errors (public endpoints)

Password reset endpoints return `{ "message": "..." }` (no `code` field):

| HTTP | Message | Cause |
|------|---------|-------|
| `400` | `Email is required.` | Missing email in forgot-password |
| `400` | `Invalid email address.` | Malformed email |
| `400` | `OTP must be 6 digits.` | OTP format invalid |
| `400` | `Invalid OTP. Please try again.` | Wrong OTP |
| `400` | `OTP has expired. Request a new one.` | OTP > 10 min old |
| `400` | `Invalid or expired OTP. Request a new one.` | OTP used/expired/not found |
| `400` | `Password and confirmation password do not match.` | Mismatched passwords |
| `400` | `password_policy_failed` | Password doesn't meet requirements (min 8 chars, etc.) |
| `503` | `The OTP email could not be sent. Try again later.` | SMTP failure |

> **Security:** Forgot-password **always returns `200`** with generic message — never reveals if email exists.

## 8. Dio integration

Use one Dio instance with the API origin as `baseUrl`. Add the bearer token only to protected calls. Store access and refresh tokens in platform secure storage; do not print token or password values in logs.

```dart
import 'package:dio/dio.dart';

final dio = Dio(BaseOptions(
  baseUrl: 'http://10.0.2.2:5001', // Android emulator, local development
  connectTimeout: const Duration(seconds: 15),
  receiveTimeout: const Duration(seconds: 15),
  headers: {
    'Accept': 'application/json',
    'Content-Type': 'application/json',
  },
));

Options withBearer(String accessToken) => Options(
  headers: {'Authorization': 'Bearer $accessToken'},
);

final response = await dio.get(
  '/api/Student/Profile',
  options: withBearer(accessToken),
);
final profileComplete = response.data['academicProfileComplete'] as bool;
```

For `DioException`, inspect `e.response?.statusCode` and `e.response?.data`. If an authorized request returns `401`, call `POST /api/Identity/Auth/refresh` once with the stored refresh token, securely replace both returned tokens, and retry the original request once. Do not refresh after a failed login or failed refresh, and prevent retry loops.

### Password reset flow (public endpoints)

```dart
// 1. Request OTP
final forgotResponse = await dio.post(
  '/api/Identity/Auth/forgot-password',
  data: {'email': 'student@university.edu'},
);
// Always 200: { "message": "If an account with this email exists, a password reset OTP has been sent." }

// 2. Show OTP input screen in Flutter, then:
final resetResponse = await dio.post(
  '/api/Identity/Auth/reset-password',
  data: {
    'email': 'student@university.edu',
    'otp': otpController.text.trim(),
    'newPassword': newPasswordController.text,
    'confirmPassword': confirmPasswordController.text,
  },
);
// Success: 200 { "message": "Your password has been reset successfully..." }
// Errors: 400 { "message": "Invalid or expired OTP..." } etc.

// 3. After successful reset, navigate to login screen
```

### Student Dashboard Summary (protected)

```dart
final dashboardResponse = await dio.get(
  '/api/Student/Dashboard',
  options: withBearer(accessToken),
);

final dashboard = dashboardResponse.data;
// dashboard['team'] - TeamDashboardDto? (null if no team)
// dashboard['proposal'] - ProposalDashboardDto? (null if no proposal)
// dashboard['project'] - ProjectDashboardDto? (null if proposal not approved)
// dashboard['milestones'] - List<MilestoneDashboardDto>
// dashboard['progressPercentage'] - int (0-100)
// dashboard['upcomingDeadlines'] - List<UpcomingDeadlineDto>

// Handle student states:
if (dashboard['team'] == null) {
  // Student has no team → show "Create/Join Team" screen
} else if (dashboard['proposal'] == null) {
  // Has team, no proposal → show "Create Proposal" screen
} else if (dashboard['proposal']['status'] == 'Rejected') {
  // Rejected → show rejection reason, allow new proposal
} else if (dashboard['proposal']['status'] == 'Approved') {
  // Approved → show project, milestones, progress, deadlines
}
```

For teams and proposals, send JSON maps matching the request examples above. A successful leave returns `204` and has no response body. After creating, joining, or leaving a team, call `GET /api/Student/Teams/me` if the screen needs the latest membership state.

Google/GitHub sign-in and email-confirmation links are browser/deep-link flows, not ordinary Dio requests. On OAuth success Flutter receives `#auth=<base64url-json>` at the configured callback; decode the fragment as JSON, securely store both tokens, then call `GET /api/Student/Profile`. On an OAuth callback containing an `error` query parameter, show an appropriate sign-in error. Never put callback tokens in logs or analytics.

Common OAuth callback errors include `email_conflict` (the email already belongs to a password account), `email_unverified` or `email_required` (provider did not return a verified email), `github_profile_failed`, `account_disabled`, and `oauth_failed`. Show a user-friendly message and let the user return to sign-in; do not expect an auth response on an error callback.

## 9. Backend feature roadmap

These features do not yet have Flutter student endpoints. Do not build Dio calls for them until the backend routes are delivered. Proposal review and supervisor assignment are web-admin workflows, not Flutter student APIs.

1. **Team management**: backend routes for create, join, leave, and member listing are implemented. Flutter must still connect its screens to these routes. Leadership transfer and team deletion are future work.
2. **Project proposal**: draft creation/editing, submission, and listing of proposal attempts are implemented. Review decisions and rejection reasons will be completed with proposal review.
3. **Proposal review**: web dashboard review and decision history.
4. **Supervisor assignment**: assign supervisors to approved projects and enforce supervisor capacity.
5. **Milestones**: create ordered milestones, due dates, status, and project progress.
6. **Submissions and file versions**: upload files to milestones, maintain versions, status, and submission history.
7. **Review and grading**: supervisor comments, grade, revision requests, and review history.
8. **Notifications**: list user notifications and mark read; generate events from project workflow changes.
9. **Student dashboard summary**: aggregate team, proposal, project, milestones, progress, and upcoming deadlines — **implemented**.
10. **Similarity analysis**: process submitted documents, persist results, and return match details.
11. **Audit history**: record and provide authorized access to important administrative/workflow events.

Student team, proposal, and dashboard endpoints require the Student role, an active account, and a completed academic profile; team and proposal endpoints also require team membership. If the academic profile is incomplete, they return `403` with code `profile_completion_required`. The profile read/completion and account-maintenance endpoints remain available during onboarding. Admin dashboard endpoints use the Admin role and must stay in the web application.

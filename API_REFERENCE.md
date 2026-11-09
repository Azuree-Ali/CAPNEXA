# CAPNEXA Backend API and Flutter Integration Guide

This guide describes the backend API as it exists in the original `CAPNEXAAPI` project and the intended Flutter flow. It separates implemented endpoints from planned features so the Flutter app does not depend on routes that do not exist yet.

## 1. API basics

- Local API origin: `https://localhost:7072` (verify the active launch profile and `AppUrls:ApiBaseUrl`).
- Android emulator: use the host machine address reachable from the emulator, commonly `https://10.0.2.2:7072`; the API must be reachable and the development TLS certificate trusted.
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
| POST | `/api/Student/ProjectProposals` | Student bearer, team leader | Create a draft proposal |
| PUT | `/api/Student/ProjectProposals/{projectId}` | Student bearer, team leader | Edit a draft proposal |
| POST | `/api/Student/ProjectProposals/{projectId}/submit` | Student bearer, team leader | Submit a draft for review |

### Admin web dashboard only

`GET /api/Admin/Students/{userId}/profile` requires the Admin role. This endpoint is for the web dashboard, not the Flutter student application.

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

Store both tokens securely. Login rejects incorrect credentials, inactive accounts, or unconfirmed email.

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

## Team management

All team routes require a Student bearer token and a completed academic profile. Any academic year is allowed. The maximum is five members including the leader. A student may belong to one team at a time. The backend generates the invite code and sets the creator as leader; Flutter must not submit `maxMembers`, `teamId` to join, or a leader flag.

### Create a team

`POST /api/Student/Teams`

```json
{"name":"Team Alpha"}
```

Success: `200 TeamDto`; the creator is added as leader. `403 profile_completion_required` if profile completion is missing; `409 student_already_in_team` if the student already belongs to a team.

### Get my team

`GET /api/Student/Teams/me`

Success: `200 TeamDto` containing `teamId`, `name`, `teamCode`, `maxMembers`, `memberCount`, and a `members` array. Each member includes user ID, first/last name, student number, `isLeader`, and `joinedAt`. `404 team_not_found` if the student is not on a team.

### Join a team

`POST /api/Student/Teams/join`

```json
{"teamCode":"A1B2C3D4E5"}
```

Success: `200 TeamDto` with updated member list. Errors: `403 profile_completion_required`; `404 team_not_found`; `409 student_already_in_team` or `team_full` when the team already has five members.

### Leave a team

`POST /api/Student/Teams/leave`

No request body. Success: `204 No Content`. A leader receives `409 team_leader_cannot_leave`; leadership transfer is not in this first version. A student with no team receives `404 team_not_found`.

The database enforces one membership per student, unique invite codes, and unique student/team membership pairs. Team operations use the generic repository and do not open explicit transactions. The five-member limit is checked before each join; simultaneous join requests could race, so strict concurrency-safe capacity enforcement would require a database-level atomic counter or transaction in a later change. Leadership transfer and team deletion are not implemented yet.

## Project proposals

All proposal endpoints require a Student bearer token, a complete academic profile, and team membership. Every team has one active proposal at a time. Any team member can create or edit the team's draft, but only the team leader can submit it for review. All team members can view the team's proposal attempts. A rejected attempt remains in the list with its rejection reason, and any team member can create a new draft from scratch. Submitted proposals cannot be edited through the student API.

### List proposal attempts

`GET /api/Student/ProjectProposals`

Returns `200` with an array of proposals, newest first. Each item contains `projectId`, `teamId`, `title`, `description`, `status`, `rejectionReason`, `createdAt`, and `submittedAt`. Returns `409 team_required` when the student is not on a team.

### Create a proposal draft

`POST /api/Student/ProjectProposals`

```json
{"title":"Campus Helper","description":"A mobile application that helps students find campus services and announcements."}
```

Title is required and must be 3–250 characters. Description is required and must be 10–4000 characters. Success is `200` with the created proposal in `Draft` status. Returns `409 active_proposal_exists` if the team already has a draft, submitted, or approved proposal.

### Edit a draft

`PUT /api/Student/ProjectProposals/{projectId}`

Uses the same JSON body and field limits as draft creation. Success is `200` with the updated proposal. Any team member can edit a draft belonging to their team.

### Submit for review

`POST /api/Student/ProjectProposals/{projectId}/submit`

No request body. Success is `200` with status `PendingReview` and a `submittedAt` timestamp. Only the team leader can submit a complete draft. Other team members receive `403 team_leader_required`. A rejected proposal is not edited or resubmitted; create a new proposal instead. Review decisions and setting `rejectionReason` will be provided by the proposal-review feature.

Common errors: `403 profile_completion_required`, `403 account_disabled`, `403 team_leader_required`, `404 proposal_not_found`, `409 team_required`, `409 active_proposal_exists`, and `409 proposal_not_editable`.

## 5. Errors Flutter should handle

- `400`: invalid request, validation failure, or profile completion issue.
- `401`: missing/invalid access token or failed login/refresh.
- `403`: account disabled or insufficient role. The current original backend does not yet have additional student feature endpoints requiring an incomplete-profile gate.
- `409`: duplicate email/student number or repeated academic-profile completion.
- `503`: email service unavailable for email-change request.

Auth errors generally return `{ "message": "..." }`. Profile business errors generally return `{ "code": "...", "message": "..." }`.

## 6. Dio integration notes

Use one Dio client with the API origin as `baseUrl`; append full route paths above. Add an interceptor for bearer token on protected calls. Do not log auth request/response bodies because they contain credentials and tokens. On an access-token `401`, refresh once, store the rotated pair, and retry the original request once. Avoid refresh retry loops.

```dart
final dio = Dio(BaseOptions(
  baseUrl: 'https://10.0.2.2:7072', // adjust for emulator/device/deployment
  connectTimeout: const Duration(seconds: 15),
  receiveTimeout: const Duration(seconds: 15),
  headers: {'Accept': 'application/json'},
));

final response = await dio.get(
  '/api/Student/Profile',
  options: Options(headers: {'Authorization': 'Bearer $accessToken'}),
);
final complete = response.data['academicProfileComplete'] as bool;
```

Use platform secure storage for access/refresh tokens. OAuth and email confirmation are browser/deep-link flows, not ordinary JSON requests.

## 7. Backend feature roadmap

These are planned backend features, not available endpoints in the current API. Build them in this order because each stage depends on earlier data and permissions.

1. **Team management**: backend routes for create, join, leave, and member listing are implemented. Flutter must still connect its screens to these routes. Leadership transfer and team deletion are future work.
2. **Project proposal**: draft creation/editing, submission, and listing of proposal attempts are implemented. Review decisions and rejection reasons will be completed with proposal review.
3. **Proposal review**: web dashboard review and decision history.
4. **Supervisor assignment**: assign supervisors to approved projects and enforce supervisor capacity.
5. **Milestones**: create ordered milestones, due dates, status, and project progress.
6. **Submissions and file versions**: upload files to milestones, maintain versions, status, and submission history.
7. **Review and grading**: supervisor comments, grade, revision requests, and review history.
8. **Notifications**: list user notifications and mark read; generate events from project workflow changes.
9. **Student dashboard summary**: aggregate team, project, milestones, deadlines, submissions, and feedback.
10. **Similarity analysis**: process submitted documents, persist results, and return match details.
11. **Audit history**: record and provide authorized access to important administrative/workflow events.

Each future student feature endpoint must check that the authenticated user has the Student role and a completed academic profile. If incomplete, return `403` with `{ "code": "profile_completion_required", "message": "Complete your academic profile before using this feature." }`. Keep profile read/completion and account-maintenance endpoints available during onboarding. Admin dashboard endpoints remain governed by Admin authorization and are not blocked by student onboarding.

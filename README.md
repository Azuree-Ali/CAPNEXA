# CapNexa – Student mobile app (Flutter)

Pixel-ported from the Figma export `GradFlow_Student_Mobile_App.zip`
(a React/CSS prototype – every colour, size, radius, spacing value and SVG icon
path was copied from its source). Mock data only; no backend yet.

## Run

```bash
# 1. generate the Android/iOS/web runner folders (lib/ and pubspec.yaml are kept)
flutter create --org com.capnexa --project-name capnexa --platforms android,ios,web .
# 2. fetch packages (google_fonts, flutter_svg)
flutter pub get
# 3. check + run
flutter analyze
flutter test
flutter run
```

Fonts: Manrope is loaded through `google_fonts` (needs internet on first run;
cached afterwards). To ship it offline, download Manrope, add it under
`assets/fonts/` and replace `GoogleFonts.manrope` in `lib/core/theme/app_text.dart`.

The brand name shown in the UI is one constant: `AppConstants.brandName`
(`CapNexa`; the Figma file says "GradFlow").

## Project structure

The code is split by **what it is**, so you always know where to look:

```
lib/
├── main.dart                 app entry point (sets up services, runs the app)
├── app.dart                  MaterialApp: theme, first route, responsive frame
│
├── core/                     shared building blocks – no business logic
│   ├── constants/            brand name, API base URL placeholder, layout caps
│   ├── theme/                colours, shadows, text styles, ThemeData
│   ├── routes/               route names, router, AppNav helpers, journey list
│   ├── utils/                small helpers
│   └── widgets/              reusable UI: buttons, text fields, cards, badges,
│                             progress, timeline, app bar, bottom nav, dialogs,
│                             state cards, icons …
│
├── models/                   ALL data classes (Student, Team, Project, …)
│
├── services/                 ALL data access  ← the .NET API goes here
│   ├── *_service.dart        abstract contracts (what the app needs)
│   ├── service_locator.dart  picks which implementation the app uses
│   └── mock/                 mock_*_service.dart – today's fake data
│       (future: api/        api_*_service.dart – calls to the .NET backend)
│
└── features/                 UI only – one folder per area of the app
    ├── auth/                 splash, onboarding, login, register (+ widgets/)
    ├── home/                 home dashboard (+ widgets/)
    ├── team/                 team choice, team
    ├── projects/             proposal, approval, supervisor, project workspace
    ├── milestones/           milestones, milestone details
    ├── submissions/          upload, analysis (scan + AI), history
    ├── similarity/           similarity result
    ├── feedback/             supervisor feedback / resubmission entry
    ├── notifications/        notifications
    └── profile/              profile, settings, completion, system states
```

Rules of thumb

* A screen lives in `features/<area>/<name>_screen.dart`.
* Widgets used by one area only live in `features/<area>/widgets/`.
* Widgets used by several areas live in `core/widgets/`.
* A screen never builds data itself – it asks `ServiceLocator.<x>` for it.
* All imports are `package:capnexa/...` (no relative `../../` paths).

## Where to add the real API (ASP.NET Core Web API)

1. Every service has an abstract contract in `lib/services/` (`AuthService`,
   `TeamService`, `ProjectService`, `MilestoneService`, `SubmissionService`,
   `FeedbackService`, `NotificationService`, `SimilarityService`,
   `HomeService`). The intended endpoint for each method is written in the
   `TODO(api)` comment above it.
2. Create `lib/services/api/api_<x>_service.dart` (`implements <X>Service`) next
   to the mock (use `http` or `dio`; `AppConstants.apiBaseUrl` is the placeholder
   base URL). Models in `lib/models/` already have `fromJson` factories.
3. In `lib/services/service_locator.dart` replace the `Mock…Service()`
   instances with the API ones. Screens only depend on the abstract types, so
   no screen changes.
4. Loading / error UI is already wired: `AsyncScaffold` shows the design's
   loading state while a future runs and the error state with "Try again"
   when it throws.
5. Add auth token storage + an HTTP interceptor in a new `lib/core/network/` folder.

## Navigation

Named routes (`lib/core/routes`). Bottom bar and the menu button (journey drawer,
as in the Figma prototype's left panel) jump to any screen; in-flow buttons use
`AppNav.push` so Android back works.

Flow: Splash → Onboarding → Register/Login → Home → Team choice → Team →
Proposal → Approval → Supervisor → Project → Milestones → Milestone →
Submission → Analysis (security scan + AI) → Similarity → Feedback →
(resubmit via Submission) → History → Completion.

## Not reproduced exactly / added beyond the Figma frames

* **Not compiled here.** The authoring sandbox had no Flutter SDK, so
  `flutter analyze/build` has not been run – expect to fix a few small
  analyzer messages on first run.
* **Platform status bar**: Figma draws a fake 9:41 status bar; the real
  system one is used (edge-to-edge, safe-area aware).
* **Settings** has no frame in Figma: built only from the existing
  "Account settings" list. Its rows show a placeholder snackbar.
* **Security scan / AI analysis** has no frame: `AnalysisScreen` re-uses the
  design's loading state card for two timed stages, then opens Similarity.
* **Resubmission** has no separate frame: Feedback → "Upload revised
  submission" opens the Submission screen (already "Version 3").
* **Project progress** = the Project workspace ring + Milestones summary.
* **Rejected proposal**: Figma only has a note; a red variant of the status
  hero is composed from the same pieces (toggle via "Preview rejected state").
* **Dialogs**: none in Figma; one reusable dialog (invite code, leave team).
* Tiny type (7–10 px) is kept as designed; OS text scale is clamped 0.9–1.2.
* Content is capped at 480 px wide and centred on tablets/foldables.
* File picking is mocked (tapping "Add another file" adds a sample file).
* Inline-SVG icons are drawn with `flutter_svg` from the exact Figma paths.
* The Figma font (Manrope) loads via `google_fonts`, not bundled.

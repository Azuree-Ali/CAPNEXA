import 'package:flutter/material.dart';

import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/features/auth/login_screen.dart';
import 'package:capnexa/features/auth/onboarding_screen.dart';
import 'package:capnexa/features/auth/register_screen.dart';
import 'package:capnexa/features/auth/splash_screen.dart';
import 'package:capnexa/features/feedback/feedback_screen.dart';
import 'package:capnexa/features/home/home_screen.dart';
import 'package:capnexa/features/milestones/milestone_detail_screen.dart';
import 'package:capnexa/features/milestones/milestones_screen.dart';
import 'package:capnexa/features/notifications/notifications_screen.dart';
import 'package:capnexa/features/profile/completion_screen.dart';
import 'package:capnexa/features/profile/profile_screen.dart';
import 'package:capnexa/features/profile/settings_screen.dart';
import 'package:capnexa/features/profile/states_screen.dart';
import 'package:capnexa/features/projects/approval_screen.dart';
import 'package:capnexa/features/projects/project_screen.dart';
import 'package:capnexa/features/projects/proposal_screen.dart';
import 'package:capnexa/features/projects/supervisor_screen.dart';
import 'package:capnexa/features/similarity/similarity_screen.dart';
import 'package:capnexa/features/submissions/analysis_screen.dart';
import 'package:capnexa/features/submissions/history_screen.dart';
import 'package:capnexa/features/submissions/submission_screen.dart';
import 'package:capnexa/features/team/team_choice_screen.dart';
import 'package:capnexa/features/team/team_screen.dart';

class AppRouter {
  AppRouter._();

  static final Map<String, WidgetBuilder> _routes = {
    Routes.splash: (_) => const SplashScreen(),
    Routes.onboarding: (_) => const OnboardingScreen(),
    Routes.register: (_) => const RegisterScreen(),
    Routes.login: (_) => const LoginScreen(),
    Routes.home: (_) => const HomeScreen(),
    Routes.teamChoice: (_) => const TeamChoiceScreen(),
    Routes.team: (_) => const TeamScreen(),
    Routes.proposal: (_) => const ProposalScreen(),
    Routes.approval: (_) => const ApprovalScreen(),
    Routes.supervisor: (_) => const SupervisorScreen(),
    Routes.project: (_) => const ProjectScreen(),
    Routes.milestones: (_) => const MilestonesScreen(),
    Routes.milestone: (_) => const MilestoneDetailScreen(),
    Routes.submission: (_) => const SubmissionScreen(),
    Routes.analysis: (_) => const AnalysisScreen(),
    Routes.similarity: (_) => const SimilarityScreen(),
    Routes.feedback: (_) => const FeedbackScreen(),
    Routes.history: (_) => const HistoryScreen(),
    Routes.notifications: (_) => const NotificationsScreen(),
    Routes.profile: (_) => const ProfileScreen(),
    Routes.settings: (_) => const SettingsScreen(),
    Routes.completion: (_) => const CompletionScreen(),
    Routes.states: (_) => const StatesScreen(),
  };

  static Route<dynamic> onGenerateRoute(RouteSettings settings) {
    final builder = _routes[settings.name] ?? _routes[Routes.home]!;
    return MaterialPageRoute<void>(settings: settings, builder: builder);
  }
}

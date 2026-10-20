import 'package:capnexa/services/auth_service.dart';
import 'package:capnexa/services/feedback_service.dart';
import 'package:capnexa/services/home_service.dart';
import 'package:capnexa/services/milestone_service.dart';
import 'package:capnexa/services/mock/mock_auth_service.dart';
import 'package:capnexa/services/mock/mock_feedback_service.dart';
import 'package:capnexa/services/mock/mock_home_service.dart';
import 'package:capnexa/services/mock/mock_milestone_service.dart';
import 'package:capnexa/services/mock/mock_notification_service.dart';
import 'package:capnexa/services/mock/mock_project_service.dart';
import 'package:capnexa/services/mock/mock_similarity_service.dart';
import 'package:capnexa/services/mock/mock_submission_service.dart';
import 'package:capnexa/services/mock/mock_team_service.dart';
import 'package:capnexa/services/notification_service.dart';
import 'package:capnexa/services/project_service.dart';
import 'package:capnexa/services/similarity_service.dart';
import 'package:capnexa/services/submission_service.dart';
import 'package:capnexa/services/team_service.dart';

/// Minimal service locator (no extra dependency).
///
/// ► API INTEGRATION POINT ◄
/// Replace the `Mock…Service()` instances below with real implementations
/// (e.g. `ApiProjectService(apiClient)`) once the ASP.NET Core Web API is
/// ready. No screen has to change – they only depend on the abstract services.
class ServiceLocator {
  ServiceLocator._();

  static late AuthService auth;
  static late TeamService team;
  static late ProjectService projects;
  static late MilestoneService milestones;
  static late SubmissionService submissions;
  static late FeedbackService feedback;
  static late NotificationService notifications;
  static late SimilarityService similarity;
  static late HomeService home;

  static void setup() {
    auth = MockAuthService();
    team = MockTeamService();
    projects = MockProjectService();
    milestones = MockMilestoneService();
    submissions = MockSubmissionService();
    feedback = MockFeedbackService();
    notifications = MockNotificationService();
    similarity = MockSimilarityService();
    home = MockHomeService(
      auth: auth,
      projects: projects,
      milestones: milestones,
      feedback: feedback,
    );
  }
}

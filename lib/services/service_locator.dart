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
import 'package:capnexa/core/network/api_client.dart';
import 'package:capnexa/core/network/token_storage.dart';
import 'package:capnexa/services/api/api_auth_service.dart';
import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/services/api/api_home_service.dart';
import 'package:capnexa/services/api/api_milestone_service.dart';
import 'package:capnexa/services/api/api_team_service.dart';
import 'package:capnexa/services/api/api_project_service.dart';

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

  /// true = بيانات وهمية، false = الباك اند الحقيقي
  static const bool _mockAuth = false; 
  static const bool _mockHome = false;
  static const bool _mockMilestones = false;
  static const bool _mockTeam = false;
  static const bool _mockProjects = false;

  static late TokenStorage tokenStorage;
  static late ApiClient apiClient;

  static void setup() {
    //auth = MockAuthService();
    tokenStorage = TokenStorage();
    apiClient = ApiClient(
      tokenStorage: tokenStorage,
      onSessionExpired: AppNav.toLogin,
    );

    auth = _mockAuth ? MockAuthService() : ApiAuthService(apiClient);
    team = _mockTeam ? MockTeamService() : ApiTeamService(apiClient);
    projects = _mockProjects ? MockProjectService() : ApiProjectService(apiClient);
    milestones = _mockMilestones ? MockMilestoneService() : ApiMilestoneService(apiClient);
    
    submissions = MockSubmissionService();
    feedback = MockFeedbackService();
    notifications = MockNotificationService();
    similarity = MockSimilarityService();
    home = _mockHome
        ? MockHomeService(
            auth: auth,
            projects: projects,
            milestones: milestones,
            feedback: feedback,
          )
        : ApiHomeService(apiClient, auth);
  }
}

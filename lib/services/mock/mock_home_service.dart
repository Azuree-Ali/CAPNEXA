import 'package:capnexa/models/app_notification.dart';
import 'package:capnexa/models/home_summary.dart';
import 'package:capnexa/services/auth_service.dart';
import 'package:capnexa/services/feedback_service.dart';
import 'package:capnexa/services/home_service.dart';
import 'package:capnexa/services/milestone_service.dart';
import 'package:capnexa/services/project_service.dart';

/// Composes the dashboard from the other mock services. The real
/// implementation should call one aggregate endpoint instead.
class MockHomeService implements HomeService {
  MockHomeService({
    required this.auth,
    required this.projects,
    required this.milestones,
    required this.feedback,
  });

  final AuthService auth;
  final ProjectService projects;
  final MilestoneService milestones;
  final FeedbackService feedback;

  @override
  Future<HomeSummary> getSummary() async {
    final student = await auth.currentStudent();
    final project = await projects.getCurrentProject();
    final deadline = await milestones.getCurrentMilestoneDetail();
    final latest = await feedback.getLatestFeedback();
    return HomeSummary(
      student: student,
      project: project,
      nextDeadline: deadline,
      latestFeedback: latest,
      updates: const [
        RecentUpdate(
          kind: NotificationKind.similarity,
          title: 'Similarity analysis completed',
          subtitle: 'Version 2 · 27% similarity',
          time: '1h',
        ),
        RecentUpdate(
          kind: NotificationKind.approval,
          title: 'Proposal approved',
          subtitle: 'Your project can now begin',
          time: '2d',
        ),
      ],
    );
  }
}

import 'package:capnexa/models/app_notification.dart';
import 'package:capnexa/models/feedback.dart';
import 'package:capnexa/models/milestone.dart';
import 'package:capnexa/models/project.dart';
import 'package:capnexa/models/student.dart';

class RecentUpdate {
  const RecentUpdate({
    required this.kind,
    required this.title,
    required this.subtitle,
    required this.time,
  });

  final NotificationKind kind;
  final String title;
  final String subtitle;
  final String time;
}

class HomeSummary {
  const HomeSummary({
    required this.student,
    required this.project,
    required this.nextDeadline,
    required this.latestFeedback,
    required this.updates,
  });

  final Student student;
  final Project project;
  final MilestoneDetail nextDeadline;
  final SupervisorFeedback latestFeedback;
  final List<RecentUpdate> updates;
}

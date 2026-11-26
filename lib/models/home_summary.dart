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
    this.teamName,
    this.proposalStatus,
    this.rejectionReason,
    this.project,
    this.nextDeadline,
    this.latestFeedback,
    this.updates = const [],
  });

  final Student student;

  /// null = الطالب لسه ملوش فريق
  final String? teamName;

  /// Draft / PendingReview / Rejected / Approved، أو null لو مفيش proposal
  final String? proposalStatus;
  final String? rejectionReason;

  /// null لحد ما الـ proposal يتوافق عليه
  final Project? project;
  final MilestoneDetail? nextDeadline;
  final SupervisorFeedback? latestFeedback;
  final List<RecentUpdate> updates;
}
import 'package:capnexa/core/utils/date_format.dart';
import 'package:capnexa/models/home_summary.dart';
import 'package:capnexa/models/milestone.dart';
import 'package:capnexa/models/project.dart';
import 'package:capnexa/models/student.dart';

typedef Json = Map<String, dynamic>;

/// بيحوّل رد /api/Student/Dashboard لـ models التطبيق.
class DashboardMapper {
  DashboardMapper._();

  static List<Json> _list(dynamic v) =>
      List<Json>.from((v as List?) ?? const []);

  static List<Json> _sorted(Json d) => _list(d['milestones'])
    ..sort((a, b) => (a['order'] as int).compareTo(b['order'] as int));

  static DateTime? _due(Json m) {
    final raw = m['dueDate'];
    return raw == null ? null : DateTime.parse(raw as String).toLocal();
  }

  static MilestoneStatus _status(Object? s) => switch (s) {
        'Completed' => MilestoneStatus.completed,
        'InProgress' => MilestoneStatus.inProgress,
        _ => MilestoneStatus.upcoming,
      };

  static Milestone _milestone(Json m) {
    final status = _status(m['status']);
    final due = _due(m);
    int? daysLeft;
    if (due != null && status != MilestoneStatus.completed) {
      final days = due.difference(DateTime.now()).inDays;
      daysLeft = days < 0 ? 0 : days;
    }
    return Milestone(
      id: '${m['milestoneId']}',
      name: m['title'] as String,
      dateRange: due == null ? 'No due date' : 'Due ${formatDate(due)}',
      progress: status == MilestoneStatus.completed ? 100 : 0,
      status: status,
      daysLeft: daysLeft,
    );
  }

  static MilestonePlan plan(Json d) {
    final sorted = _sorted(d);
    DateTime? last;
    for (final m in sorted) {
      final due = _due(m);
      if (due != null) last = due;
    }
    return MilestonePlan(
      overallProgress: ((d['progressPercentage'] as num?) ?? 0).toDouble(),
      finalSubmission: last == null ? '—' : formatDate(last),
      milestones: [for (final m in sorted) _milestone(m)],
    );
  }

  static MilestoneDetail? detailOf(Json d, Object? milestoneId) {
    if (milestoneId == null) return null;
    final sorted = _sorted(d);
    final match = sorted.where((m) => '${m['milestoneId']}' == '$milestoneId');
    if (match.isEmpty) return null;
    final m = match.first;
    final milestone = _milestone(m);
    final due = _due(m);
    return MilestoneDetail(
      milestone: milestone,
      number: m['order'] as int,
      total: sorted.length,
      day: due == null ? '--' : due.day.toString().padLeft(2, '0'),
      month: due == null ? '' : monthShort(due).toUpperCase(),
      deadlineLabel: due == null ? 'No due date' : 'Due ${formatDate(due)}',
      remainingLabel: milestone.daysLeft == null
          ? ''
          : '${milestone.daysLeft} days remaining',
      about: (m['description'] ?? '').toString(),
      requirements: const [],
      supervisorInstructions: '',
    );
  }

  static MilestoneDetail? nextDeadline(Json d) {
    final upcoming = _list(d['upcomingDeadlines']);
    return upcoming.isEmpty ? null : detailOf(d, upcoming.first['milestoneId']);
  }

  /// المرحلة الشغالة دلوقتي، وإلا أقرب deadline
  static MilestoneDetail? currentDetail(Json d) {
    for (final m in _sorted(d)) {
      if (_status(m['status']) == MilestoneStatus.inProgress) {
        return detailOf(d, m['milestoneId']);
      }
    }
    return nextDeadline(d);
  }

  static Project? project(Json d) {
    final p = d['project'] as Json?;
    if (p == null) return null;
    final team = d['team'] as Json?;
    final current = currentDetail(d);
    final supervisor = p['supervisorName'] as String?;
    return Project(
      id: '${p['projectId']}',
      title: p['title'] as String,
      tagline: supervisor == null ? '' : 'Supervisor · $supervisor',
      status: ProjectStatus.inProgress,
      progress: ((d['progressPercentage'] as num?) ?? 0).toDouble(),
      currentMilestone: current?.milestone.name ?? 'No active milestone',
      milestoneNumber: current?.number ?? 0,
      milestoneCount: _sorted(d).length,
      supervisorName: supervisor ?? 'Not assigned yet',
      teamSize: (team?['memberCount'] as int?) ?? 0,
      finalDeadline: plan(d).finalSubmission,
      submissionCount: 0,
      dueLabel: current?.deadlineLabel ?? '',
      daysLeft: current?.milestone.daysLeft ?? 0,
    );
  }

  static HomeSummary summary(Student student, Json d) {
    final team = d['team'] as Json?;
    final proposal = d['proposal'] as Json?;
    return HomeSummary(
      student: student,
      teamName: team?['name'] as String?,
      proposalStatus: proposal?['status'] as String?,
      rejectionReason: proposal?['rejectionReason'] as String?,
      project: project(d),
      nextDeadline: nextDeadline(d),
    );
  }
}
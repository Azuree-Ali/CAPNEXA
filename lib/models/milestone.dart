enum MilestoneStatus { completed, inProgress, upcoming }

class Milestone {
  const Milestone({
    required this.id,
    required this.name,
    required this.dateRange,
    required this.progress,
    required this.status,
    this.daysLeft,
  });

  final String id;
  final String name;
  final String dateRange;
  final double progress;
  final MilestoneStatus status;
  final int? daysLeft;

  factory Milestone.fromJson(Map<String, dynamic> json) => Milestone(
        id: json['id'] as String,
        name: json['name'] as String,
        dateRange: json['dateRange'] as String,
        progress: (json['progress'] as num).toDouble(),
        status: MilestoneStatus.values.byName(json['status'] as String),
        daysLeft: json['daysLeft'] as int?,
      );
}

class MilestonePlan {
  const MilestonePlan({
    required this.overallProgress,
    required this.finalSubmission,
    required this.milestones,
  });

  final double overallProgress;
  final String finalSubmission;
  final List<Milestone> milestones;

  int get inProgressNumber =>
      milestones.indexWhere((m) => m.status == MilestoneStatus.inProgress) + 1;
}

class MilestoneDetail {
  const MilestoneDetail({
    required this.milestone,
    required this.number,
    required this.total,
    required this.day,
    required this.month,
    required this.deadlineLabel,
    required this.remainingLabel,
    required this.about,
    required this.requirements,
    required this.supervisorInstructions,
  });

  final Milestone milestone;
  final int number;
  final int total;
  final String day;
  final String month;
  final String deadlineLabel;
  final String remainingLabel;
  final String about;
  final List<String> requirements;
  final String supervisorInstructions;
}

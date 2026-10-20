enum ProjectStatus { draft, pending, inProgress, completed }

class Project {
  const Project({
    required this.id,
    required this.title,
    required this.tagline,
    required this.status,
    required this.progress,
    required this.currentMilestone,
    required this.milestoneNumber,
    required this.milestoneCount,
    required this.supervisorName,
    required this.teamSize,
    required this.finalDeadline,
    required this.submissionCount,
    required this.dueLabel,
    required this.daysLeft,
  });

  final String id;
  final String title;
  final String tagline;
  final ProjectStatus status;

  /// 0 – 100
  final double progress;
  final String currentMilestone;
  final int milestoneNumber;
  final int milestoneCount;
  final String supervisorName;
  final int teamSize;
  final String finalDeadline;
  final int submissionCount;
  final String dueLabel;
  final int daysLeft;

  factory Project.fromJson(Map<String, dynamic> json) => Project(
        id: json['id'] as String,
        title: json['title'] as String,
        tagline: json['tagline'] as String,
        status: ProjectStatus.values.byName(json['status'] as String),
        progress: (json['progress'] as num).toDouble(),
        currentMilestone: json['currentMilestone'] as String,
        milestoneNumber: json['milestoneNumber'] as int,
        milestoneCount: json['milestoneCount'] as int,
        supervisorName: json['supervisorName'] as String,
        teamSize: json['teamSize'] as int,
        finalDeadline: json['finalDeadline'] as String,
        submissionCount: json['submissionCount'] as int,
        dueLabel: json['dueLabel'] as String,
        daysLeft: json['daysLeft'] as int,
      );
}

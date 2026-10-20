class ProposalDraft {
  const ProposalDraft({
    required this.title,
    required this.description,
    required this.problemStatement,
    required this.proposedSolution,
    required this.technologies,
    required this.category,
    required this.lastSaved,
    required this.memberInitials,
  });

  final String title;
  final String description;
  final String problemStatement;
  final String proposedSolution;
  final String technologies;
  final String category;
  final String lastSaved;
  final List<String> memberInitials;

  Map<String, dynamic> toJson() => {
        'title': title,
        'description': description,
        'problemStatement': problemStatement,
        'proposedSolution': proposedSolution,
        'technologies': technologies,
        'category': category,
      };
}

enum ReviewState { complete, current, pending }

class ReviewStep {
  const ReviewStep({required this.title, required this.subtitle, required this.state});

  final String title;
  final String subtitle;
  final ReviewState state;
}

enum ProposalStatus { draft, pending, approved, rejected }

class ProposalStatusInfo {
  const ProposalStatusInfo({
    required this.status,
    required this.projectTitle,
    required this.proposalId,
    required this.reviewer,
    required this.steps,
    this.rejectionReason,
  });

  final ProposalStatus status;
  final String projectTitle;
  final String proposalId;
  final String reviewer;
  final List<ReviewStep> steps;
  final String? rejectionReason;
}

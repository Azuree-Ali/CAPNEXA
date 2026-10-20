class RequiredChange {
  const RequiredChange({required this.title, required this.location, this.done = false});

  final String title;
  final String location;
  final bool done;
}

class SupervisorFeedback {
  const SupervisorFeedback({
    required this.authorInitials,
    required this.authorName,
    required this.date,
    required this.versionLabel,
    required this.message,
    required this.changes,
    required this.homeExcerpt,
    required this.homeTimestamp,
  });

  final String authorInitials;
  final String authorName;
  final String date;
  final String versionLabel;
  final String message;
  final List<RequiredChange> changes;

  /// Short quote + timestamp shown on the Home dashboard card.
  final String homeExcerpt;
  final String homeTimestamp;
}

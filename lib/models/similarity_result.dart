enum MatchSeverity { high, medium, low }

class SimilarityMatch {
  const SimilarityMatch({
    required this.percent,
    required this.title,
    required this.location,
    required this.severity,
  });

  final int percent;
  final String title;
  final String location;
  final MatchSeverity severity;
}

class SimilarityResult {
  const SimilarityResult({
    required this.score,
    required this.fileName,
    required this.analyzedAt,
    required this.pages,
    required this.matches,
  });

  final int score;
  final String fileName;
  final String analyzedAt;
  final int pages;
  final List<SimilarityMatch> matches;
}

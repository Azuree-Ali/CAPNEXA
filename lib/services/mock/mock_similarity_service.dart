import 'package:capnexa/core/constants/app_constants.dart';
import 'package:capnexa/models/similarity_result.dart';
import 'package:capnexa/services/similarity_service.dart';

class MockSimilarityService implements SimilarityService {
  @override
  Future<SimilarityResult> getLatestResult() async {
    await Future<void>.delayed(AppConstants.mockLatency);
    return const SimilarityResult(
      score: 27,
      fileName: 'system-design-v3.pdf',
      analyzedAt: 'Analyzed May 20, 2:38 PM',
      pages: 42,
      matches: [
        SimilarityMatch(percent: 11, title: 'Mobile service discovery systems', location: 'Pages 8–10 · Academic publication', severity: MatchSeverity.high),
        SimilarityMatch(percent: 9, title: 'University service architecture', location: 'Pages 17–19 · Student repository', severity: MatchSeverity.medium),
        SimilarityMatch(percent: 7, title: 'Common technical terminology', location: 'Across 6 pages · Multiple sources', severity: MatchSeverity.low),
      ],
    );
  }
}

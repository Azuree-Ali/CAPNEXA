import 'package:capnexa/models/similarity_result.dart';

/// Security-scan + AI similarity contract.
/// TODO(api): `GET /submissions/{id}/scan`, `GET /submissions/{id}/similarity`.
abstract class SimilarityService {
  Future<SimilarityResult> getLatestResult();
}

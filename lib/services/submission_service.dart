import 'package:capnexa/models/submission.dart';

/// Submission contract.
/// TODO(api): `GET /milestones/current/submissions`, multipart
/// `POST /milestones/{id}/submissions` (triggers security scan + AI analysis).
abstract class SubmissionService {
  Future<SubmissionDraft> getDraft();
  Future<List<SubmissionVersion>> getVersions();
  Future<SubmissionFile> addFile();
  Future<void> submit({required String note, required List<SubmissionFile> files});
}

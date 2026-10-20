import 'package:capnexa/core/constants/app_constants.dart';
import 'package:capnexa/models/submission.dart';
import 'package:capnexa/services/submission_service.dart';

class MockSubmissionService implements SubmissionService {
  @override
  Future<SubmissionDraft> getDraft() async {
    await Future<void>.delayed(AppConstants.mockLatency);
    return const SubmissionDraft(
      milestoneName: 'System Design',
      version: 3,
      submittedBy: 'Lina Hassan',
      files: [
        SubmissionFile(name: 'system-design-v3.pdf', kind: FileKind.pdf, sizeMb: 8.4),
        SubmissionFile(name: 'architecture-source.zip', kind: FileKind.zip, sizeMb: 14.2),
      ],
    );
  }

  @override
  Future<List<SubmissionVersion>> getVersions() async {
    await Future<void>.delayed(AppConstants.mockLatency);
    return const [
      SubmissionVersion(
        number: 3,
        date: 'May 20, 2025 · 2:15 PM',
        status: VersionStatus.processing,
        statusLabel: 'Processing',
        similarity: '27%',
        review: 'Pending',
        isLatest: true,
      ),
      SubmissionVersion(
        number: 2,
        date: 'May 18, 2025 · 4:42 PM',
        status: VersionStatus.revisionNeeded,
        statusLabel: 'Revision needed',
        similarity: '31%',
        review: 'Reviewed',
      ),
      SubmissionVersion(
        number: 1,
        date: 'May 16, 2025 · 11:08 AM',
        status: VersionStatus.replaced,
        statusLabel: 'Replaced',
        similarity: '34%',
        review: 'Reviewed',
      ),
    ];
  }

  @override
  Future<SubmissionFile> addFile() async =>
      const SubmissionFile(name: 'appendix-notes.docx', kind: FileKind.doc, sizeMb: 1.1);

  @override
  Future<void> submit({required String note, required List<SubmissionFile> files}) =>
      Future<void>.delayed(const Duration(milliseconds: 300));
}

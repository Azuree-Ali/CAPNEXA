enum FileKind { pdf, zip, doc }

class SubmissionFile {
  const SubmissionFile({
    required this.name,
    required this.kind,
    required this.sizeMb,
    this.progress = 100,
    this.added = 'Added just now',
  });

  final String name;
  final FileKind kind;
  final double sizeMb;
  final double progress;
  final String added;

  String get label => switch (kind) {
        FileKind.pdf => 'PDF',
        FileKind.zip => 'ZIP',
        FileKind.doc => 'DOC',
      };

  String get meta => '${sizeMb.toStringAsFixed(1)} MB · $added';
}

/// The submission being prepared (Version N of the current milestone).
class SubmissionDraft {
  const SubmissionDraft({
    required this.milestoneName,
    required this.version,
    required this.submittedBy,
    required this.files,
  });

  final String milestoneName;
  final int version;
  final String submittedBy;
  final List<SubmissionFile> files;
}

enum VersionStatus { processing, revisionNeeded, replaced, approved }

class SubmissionVersion {
  const SubmissionVersion({
    required this.number,
    required this.date,
    required this.status,
    required this.statusLabel,
    required this.similarity,
    required this.review,
    this.isLatest = false,
  });

  final int number;
  final String date;
  final VersionStatus status;
  final String statusLabel;
  final String similarity;
  final String review;
  final bool isLatest;
}

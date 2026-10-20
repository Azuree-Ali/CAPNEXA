import 'package:flutter/material.dart';

import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_progress.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/app_text_field.dart';
import 'package:capnexa/core/widgets/async_scaffold.dart';
import 'package:capnexa/core/widgets/dashed_border.dart';
import 'package:capnexa/core/widgets/info_note.dart';
import 'package:capnexa/core/widgets/section_heading.dart';
import 'package:capnexa/models/submission.dart';
import 'package:capnexa/services/service_locator.dart';

class SubmissionScreen extends StatelessWidget {
  const SubmissionScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AsyncScaffold<SubmissionDraft>(
      title: 'Upload submission',
      tab: AppTab.project,
      loader: ServiceLocator.submissions.getDraft,
      subtitleBuilder: (d) => '${d.milestoneName} · Version ${d.version}',
      builder: (context, draft) => _SubmissionBody(draft: draft),
    );
  }
}

class _SubmissionBody extends StatefulWidget {
  const _SubmissionBody({required this.draft});

  final SubmissionDraft draft;

  @override
  State<_SubmissionBody> createState() => _SubmissionBodyState();
}

class _SubmissionBodyState extends State<_SubmissionBody> {
  late final List<SubmissionFile> _files = [...widget.draft.files];
  final _note = TextEditingController();

  @override
  void dispose() {
    _note.dispose();
    super.dispose();
  }

  Future<void> _addFile() async {
    final file = await ServiceLocator.submissions.addFile();
    if (mounted) setState(() => _files.add(file));
  }

  Future<void> _submit() async {
    await ServiceLocator.submissions.submit(note: _note.text, files: _files);
    if (mounted) AppNav.push(context, Routes.analysis);
  }

  @override
  Widget build(BuildContext context) {
    final total = _files.fold<double>(0, (sum, f) => sum + f.sizeMb);
    return ScreenBody(
      children: [
        const InfoNote(
          icon: AppIcons.shield,
          message:
              'Files are encrypted and visible only to your team, supervisor, and authorized reviewers.',
        ),
        Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            SectionHeading('Required documents', trailing: '${_files.length} of ${_files.length} ready'),
            for (var i = 0; i < _files.length; i++) ...[
              if (i > 0) const SizedBox(height: 8),
              _FileCard(file: _files[i]),
            ],
          ],
        ),
        GestureDetector(
          onTap: _addFile,
          child: DashedBorder(
            color: const Color(0xFFAEB9CA),
            radius: 13,
            fill: const Color(0xFFF9FAFC),
            padding: const EdgeInsets.all(17),
            child: Column(
              children: [
                Container(
                  width: 38,
                  height: 38,
                  alignment: Alignment.center,
                  decoration: BoxDecoration(
                    color: AppColors.blue100,
                    borderRadius: BorderRadius.circular(10),
                  ),
                  child: const AppIcon(AppIcons.upload, size: 25, color: AppColors.blue700),
                ),
                const SizedBox(height: 7),
                Text('Add another file',
                    style: AppText.s(9.5, w: FontWeight.w700, color: AppColors.ink)),
                const SizedBox(height: 2),
                Text('PDF, DOCX, or ZIP · Max 50 MB', style: AppText.s(8, color: AppColors.muted)),
              ],
            ),
          ),
        ),
        AppTextField(
          label: 'Submission note (optional)',
          hint: 'Add context for your supervisor...',
          textarea: true,
          controller: _note,
        ),
        Container(
          padding: const EdgeInsets.all(13),
          decoration: BoxDecoration(
            color: AppColors.softFill,
            borderRadius: BorderRadius.circular(12),
          ),
          child: Column(
            children: [
              _SummaryRow('Submission version', 'Version ${widget.draft.version}'),
              const SizedBox(height: 8),
              _SummaryRow('Files', '${_files.length} files · ${total.toStringAsFixed(1)} MB'),
              const SizedBox(height: 8),
              _SummaryRow('Submitted by', widget.draft.submittedBy),
            ],
          ),
        ),
        Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            AppButton(label: 'Submit for review', icon: AppIcons.upload, full: true, onPressed: _submit),
            const SizedBox(height: 20),
            AppButton(
              label: 'View previous versions',
              variant: ButtonVariant.text,
              full: true,
              onPressed: () => AppNav.push(context, Routes.history),
            ),
          ],
        ),
      ],
    );
  }
}

class _SummaryRow extends StatelessWidget {
  const _SummaryRow(this.label, this.value);

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(label, style: AppText.s(8.5, color: AppColors.muted)),
        Text(value, style: AppText.s(8.5, w: FontWeight.w700, color: AppColors.navy900)),
      ],
    );
  }
}

/// `.file-card`
class _FileCard extends StatelessWidget {
  const _FileCard({required this.file});

  final SubmissionFile file;

  @override
  Widget build(BuildContext context) {
    final (fg, bg) = switch (file.kind) {
      FileKind.pdf => (AppColors.red700, AppColors.red100),
      _ => (AppColors.blue700, AppColors.blue100),
    };
    return Container(
      padding: const EdgeInsets.all(11),
      decoration: BoxDecoration(
        color: Colors.white,
        border: Border.all(color: AppColors.line),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        children: [
          Container(
            width: 38,
            height: 41,
            alignment: Alignment.center,
            decoration: BoxDecoration(color: bg, borderRadius: BorderRadius.circular(8)),
            child: Text(file.label, style: AppText.s(7, w: FontWeight.w800, color: fg)),
          ),
          const SizedBox(width: 9),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(file.name, style: AppText.s(9, w: FontWeight.w700)),
                Padding(
                  padding: const EdgeInsets.fromLTRB(0, 1, 0, 5),
                  child: Text(file.meta, style: AppText.s(7.5, color: AppColors.muted, height: 1.6)),
                ),
                AppProgress(value: file.progress, height: 3),
              ],
            ),
          ),
          const SizedBox(width: 9),
          Container(
            width: 20,
            height: 20,
            alignment: Alignment.center,
            decoration: const BoxDecoration(color: AppColors.teal100, shape: BoxShape.circle),
            child: const AppIcon(AppIcons.check, size: 15, color: AppColors.teal700),
          ),
        ],
      ),
    );
  }
}

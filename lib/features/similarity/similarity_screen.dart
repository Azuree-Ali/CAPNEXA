import 'package:flutter/material.dart';

import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_card.dart';
import 'package:capnexa/core/widgets/app_dialog.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_progress.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/async_scaffold.dart';
import 'package:capnexa/core/widgets/info_note.dart';
import 'package:capnexa/core/widgets/section_heading.dart';
import 'package:capnexa/core/widgets/status_badge.dart';
import 'package:capnexa/models/similarity_result.dart';
import 'package:capnexa/services/service_locator.dart';

class SimilarityScreen extends StatelessWidget {
  const SimilarityScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AsyncScaffold<SimilarityResult>(
      title: 'Similarity analysis',
      tab: AppTab.project,
      loader: ServiceLocator.similarity.getLatestResult,
      subtitleBuilder: (r) => r.fileName,
      actionBuilder: (_, __) => const AppIcon(AppIcons.spark),
      builder: (context, r) => ScreenBody(
        children: [
          Column(
            children: [
              Container(
                width: 132,
                height: 132,
                margin: const EdgeInsets.fromLTRB(0, 4, 0, 12),
                child: CustomPaint(
                  painter: RingPainter(
                    value: r.score.toDouble(),
                    trackColor: const Color(0xFFEDF0F4),
                    fillColor: const Color(0xFFE8953E),
                    radiusRatio: 50 / 120,
                    strokeRatio: 8 / 120,
                  ),
                  child: Center(
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Text('${r.score}%',
                            style: AppText.s(27, w: FontWeight.w700, color: AppColors.navy900)),
                        Text('Similarity', style: AppText.s(8, color: AppColors.muted)),
                      ],
                    ),
                  ),
                ),
              ),
              const StatusBadge('Moderate review recommended', tone: BadgeTone.orange),
              const SizedBox(height: 9),
              Text('Analysis completed', style: AppText.h2),
              const SizedBox(height: 5),
              ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 310),
                child: Text(
                  'This result highlights potentially similar content. It is not proof of plagiarism and should be interpreted in academic context.',
                  textAlign: TextAlign.center,
                  style: AppText.p,
                ),
              ),
            ],
          ),
          Container(
            padding: const EdgeInsets.all(9),
            decoration: const BoxDecoration(
              border: Border.symmetric(horizontal: BorderSide(color: AppColors.line)),
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                const AppIcon(AppIcons.clock, color: AppColors.muted),
                const SizedBox(width: 4),
                Text(r.analyzedAt, style: AppText.s(8, color: AppColors.muted)),
                const SizedBox(width: 15),
                const AppIcon(AppIcons.file, color: AppColors.muted),
                const SizedBox(width: 4),
                Text('${r.pages} pages', style: AppText.s(8, color: AppColors.muted)),
              ],
            ),
          ),
          Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              SectionHeading('Potentially similar content',
                  actionLabel: 'View report',
                  onAction: () =>
                      showAppSnack(context, 'The detailed report will be available with the API.')),
              DividedList(
                radius: 13,
                children: [for (final m in r.matches) _MatchRow(match: m)],
              ),
            ],
          ),
          const InfoNote(
            icon: AppIcons.shield,
            iconSize: 20,
            bordered: true,
            padding: 12,
            radius: 12,
            title: 'Use this insight responsibly',
            message: 'Review citations, paraphrasing, and quoted material before final submission.',
            textColor: AppColors.muted,
            titleColor: AppColors.muted,
          ),
          AppButton(
            label: 'Continue to supervisor review',
            icon: AppIcons.arrow,
            full: true,
            onPressed: () => AppNav.push(context, Routes.feedback),
          ),
        ],
      ),
    );
  }
}

class _MatchRow extends StatelessWidget {
  const _MatchRow({required this.match});

  final SimilarityMatch match;

  @override
  Widget build(BuildContext context) {
    final (fg, bg) = switch (match.severity) {
      MatchSeverity.high => (AppColors.orange700, AppColors.orange100),
      MatchSeverity.medium => (const Color(0xFF8B6711), const Color(0xFFFFF7DC)),
      MatchSeverity.low => (AppColors.blue700, AppColors.blue100),
    };
    return Padding(
      padding: const EdgeInsets.all(11),
      child: Row(
        children: [
          Container(
            width: 36,
            height: 36,
            alignment: Alignment.center,
            decoration: BoxDecoration(color: bg, borderRadius: BorderRadius.circular(9)),
            child: Text('${match.percent}%', style: AppText.s(8, w: FontWeight.w800, color: fg)),
          ),
          const SizedBox(width: 9),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(match.title, style: AppText.s(8.5, w: FontWeight.w700, color: AppColors.navy900)),
                Text(match.location, style: AppText.s(7, color: AppColors.muted, height: 1.6)),
              ],
            ),
          ),
          const AppIcon(AppIcons.chevron),
        ],
      ),
    );
  }
}

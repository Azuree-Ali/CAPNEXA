import 'package:flutter/material.dart';

import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_progress.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/async_scaffold.dart';
import 'package:capnexa/core/widgets/avatar_box.dart';
import 'package:capnexa/core/widgets/status_badge.dart';
import 'package:capnexa/models/milestone.dart';
import 'package:capnexa/services/service_locator.dart';

class MilestoneDetailScreen extends StatelessWidget {
  const MilestoneDetailScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AsyncScaffold<MilestoneDetail>(
      title: 'Milestone',
      tab: AppTab.project,
      loader: ServiceLocator.milestones.getCurrentMilestoneDetail,
      titleBuilder: (d) => d.milestone.name,
      subtitleBuilder: (d) => 'Milestone ${d.number} of ${d.total}',
      actionBuilder: (_, __) => const StatusBadge('In progress', tone: BadgeTone.orange),
      builder: (context, d) => ScreenBody(
        children: [
          Container(
            padding: const EdgeInsets.all(14),
            decoration: BoxDecoration(
              color: const Color(0xFFFFFAF4),
              border: Border.all(color: const Color(0xFFF0D4AF)),
              borderRadius: BorderRadius.circular(14),
            ),
            child: Row(
              children: [
                DateBlock(day: d.day, month: d.month),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('DEADLINE', style: AppText.overline),
                      Padding(
                        padding: const EdgeInsets.fromLTRB(0, 3, 0, 1),
                        child: Text(d.deadlineLabel, style: AppText.h2),
                      ),
                      Text(d.remainingLabel,
                          style: AppText.s(11, w: FontWeight.w700, color: AppColors.orange700, height: 1.6)),
                    ],
                  ),
                ),
              ],
            ),
          ),
          AppProgress(value: d.milestone.progress, label: 'Milestone progress'),
          _Block(
            title: 'About this milestone',
            child: Padding(
              padding: const EdgeInsets.only(top: 7),
              child: Text(d.about, style: AppText.p),
            ),
          ),
          _Block(
            title: 'Submission requirements',
            child: Padding(
              padding: const EdgeInsets.only(top: 10),
              child: Column(
                children: [
                  for (var i = 0; i < d.requirements.length; i++) ...[
                    if (i > 0) const SizedBox(height: 9),
                    Row(
                      children: [
                        Container(
                          width: 18,
                          height: 18,
                          padding: const EdgeInsets.all(3),
                          decoration: const BoxDecoration(
                              color: AppColors.teal100, shape: BoxShape.circle),
                          child: const AppIcon(AppIcons.check, size: 12, color: AppColors.teal700),
                        ),
                        const SizedBox(width: 8),
                        Expanded(child: Text(d.requirements[i], style: AppText.s(9.5))),
                      ],
                    ),
                  ],
                ],
              ),
            ),
          ),
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: AppColors.blue50,
              borderRadius: BorderRadius.circular(13),
            ),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const AppIcon(AppIcons.message, color: AppColors.blue700),
                const SizedBox(width: 10),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('SUPERVISOR INSTRUCTIONS', style: AppText.overline),
                      Padding(
                        padding: const EdgeInsets.symmetric(vertical: 5),
                        child: Text(d.supervisorInstructions, style: AppText.p),
                      ),
                      GestureDetector(
                        onTap: () => AppNav.push(context, Routes.feedback),
                        child: Text('View previous feedback',
                            style: AppText.s(8.5, w: FontWeight.w800, color: AppColors.blue700)),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
          AppButton(
            label: 'Upload submission',
            icon: AppIcons.upload,
            full: true,
            onPressed: () => AppNav.push(context, Routes.submission),
          ),
        ],
      ),
    );
  }
}

/// `.content-block` – heading + body with a bottom hairline.
class _Block extends StatelessWidget {
  const _Block({required this.title, required this.child});

  final String title;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.only(bottom: 17),
      decoration: const BoxDecoration(border: Border(bottom: BorderSide(color: AppColors.line))),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [Text(title, style: AppText.h2), child],
      ),
    );
  }
}

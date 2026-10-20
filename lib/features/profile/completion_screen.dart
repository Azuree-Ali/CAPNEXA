import 'package:flutter/material.dart';

import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_card.dart';
import 'package:capnexa/core/widgets/app_dialog.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/section_heading.dart';
import 'package:capnexa/core/widgets/status_badge.dart';

/// "Project completed" – final evaluation and archive summary.
class CompletionScreen extends StatelessWidget {
  const CompletionScreen({super.key});

  static const _stats = <(String, String)>[
    ('6/6', 'Milestones'),
    ('12', 'Submissions'),
    ('18 Jul', 'Completed'),
  ];

  @override
  Widget build(BuildContext context) {
    Widget summaryRow(String l, String v) => Padding(
          padding: const EdgeInsets.only(top: 8),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(l, style: AppText.s(8.5, color: AppColors.muted)),
              Flexible(
                child: Text(v,
                    textAlign: TextAlign.end,
                    style: AppText.s(8.5, w: FontWeight.w700)),
              ),
            ],
          ),
        );
    return AppScaffold(
      title: 'Project completion',
      subtitle: 'Academic year 2024–2025',
      backgroundColor: AppColors.scaffold,
      body: DecoratedBox(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [Color(0xFFF1FAF7), AppColors.scaffold],
            stops: [0, .32],
          ),
        ),
        child: ScreenBody(
          children: [
            Padding(
              padding: const EdgeInsets.only(top: 7),
              child: Column(
                children: [
                  SizedBox(
                    width: 75,
                    height: 75,
                    child: Stack(
                      clipBehavior: Clip.none,
                      children: [
                        Container(
                          alignment: Alignment.center,
                          decoration: BoxDecoration(
                            color: AppColors.navy900,
                            borderRadius: BorderRadius.circular(22),
                            boxShadow: const [
                              BoxShadow(
                                  color: Color.fromRGBO(11, 31, 58, .17),
                                  blurRadius: 24,
                                  offset: Offset(0, 10)),
                            ],
                          ),
                          child: const AppIcon(AppIcons.school, size: 36, color: Colors.white),
                        ),
                        Positioned(
                          right: -5,
                          bottom: -5,
                          child: Container(
                            width: 27,
                            height: 27,
                            alignment: Alignment.center,
                            decoration: BoxDecoration(
                              color: AppColors.teal700,
                              shape: BoxShape.circle,
                              border: Border.all(color: Colors.white, width: 3),
                            ),
                            child: const AppIcon(AppIcons.check, size: 17, color: Colors.white),
                          ),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 11),
                  const StatusBadge('Project completed', tone: BadgeTone.teal),
                  const SizedBox(height: 9),
                  Text('Campus Connect',
                      style: AppText.s(21, w: FontWeight.w700, color: AppColors.navy900, ls: -.3)),
                  const SizedBox(height: 5),
                  Text(
                    'Your graduation project journey is complete. All milestones, reviews, and final evaluations have been recorded.',
                    textAlign: TextAlign.center,
                    style: AppText.p,
                  ),
                ],
              ),
            ),
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: AppColors.navy900,
                borderRadius: BorderRadius.circular(16),
              ),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('FINAL EVALUATION', style: AppText.s(7, color: const Color(0xFF8FA3C0))),
                      const SizedBox(height: 4),
                      Text('Excellent', style: AppText.s(17, w: FontWeight.w700, color: Colors.white)),
                      const SizedBox(height: 2),
                      Text('92 out of 100',
                          style: AppText.s(8, color: const Color(0xFFB0BFD3), height: 1.6)),
                    ],
                  ),
                  Container(
                    width: 57,
                    height: 57,
                    alignment: Alignment.center,
                    decoration: BoxDecoration(
                      color: const Color(0xFF173E6B),
                      borderRadius: BorderRadius.circular(16),
                    ),
                    child: Text('A',
                        style: AppText.s(27, w: FontWeight.w800, color: const Color(0xFF86E0CE))),
                  ),
                ],
              ),
            ),
            Container(
              decoration: BoxDecoration(
                color: Colors.white,
                border: Border.all(color: AppColors.line),
                borderRadius: BorderRadius.circular(13),
              ),
              child: IntrinsicHeight(
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    for (var i = 0; i < _stats.length; i++) ...[
                      Expanded(
                        child: Padding(
                          padding: const EdgeInsets.all(11),
                          child: Column(
                            children: [
                              Text(_stats[i].$1, style: AppText.s(11, w: FontWeight.w700)),
                              const SizedBox(height: 2),
                              Text(_stats[i].$2, style: AppText.s(7, color: AppColors.muted)),
                            ],
                          ),
                        ),
                      ),
                      if (i < _stats.length - 1)
                        const VerticalDivider(width: 1, color: AppColors.line),
                    ],
                  ],
                ),
              ),
            ),
            AppCard(
              shadow: false,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  const SectionHeading('Project summary', trailingWidget: StatusBadge('Archived', tone: BadgeTone.teal)),
                  Container(
                    padding: const EdgeInsets.only(bottom: 10),
                    decoration: const BoxDecoration(
                        border: Border(bottom: BorderSide(color: AppColors.line))),
                    child: Text(
                      'Campus Connect is an AI-powered mobile platform that unifies campus services and delivers personalized recommendations to students.',
                      style: AppText.p,
                    ),
                  ),
                  summaryRow('Supervisor', 'Dr. Omar Rahman'),
                  summaryRow('Team', 'GradFlow Team 04 · 4 members'),
                  summaryRow('Final similarity', '18% · Reviewed'),
                ],
              ),
            ),
            AppButton(
              label: 'Download final report',
              icon: AppIcons.file,
              full: true,
              onPressed: () => showAppSnack(context, 'The final report will be available with the API.'),
            ),
            AppButton(
              label: 'View project archive',
              variant: ButtonVariant.outline,
              full: true,
              onPressed: () => showAppSnack(context, 'The project archive will be available with the API.'),
            ),
          ],
        ),
      ),
    );
  }
}

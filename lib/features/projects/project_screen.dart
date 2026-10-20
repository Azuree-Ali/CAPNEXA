import 'package:flutter/material.dart';

import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_progress.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/app_top_bar.dart';
import 'package:capnexa/core/widgets/async_scaffold.dart';
import 'package:capnexa/core/widgets/avatar_box.dart';
import 'package:capnexa/core/widgets/section_heading.dart';
import 'package:capnexa/core/widgets/status_badge.dart';
import 'package:capnexa/models/project.dart';
import 'package:capnexa/services/service_locator.dart';

class ProjectScreen extends StatelessWidget {
  const ProjectScreen({super.key});

  static const _tabs = ['Overview', 'Milestones', 'Submissions', 'Feedback'];

  void _onTab(BuildContext context, String tab) {
    switch (tab) {
      case 'Milestones':
        AppNav.push(context, Routes.milestones);
      case 'Submissions':
        AppNav.push(context, Routes.history);
      case 'Feedback':
        AppNav.push(context, Routes.feedback);
    }
  }

  @override
  Widget build(BuildContext context) {
    return AsyncScaffold<Project>(
      title: 'Project workspace',
      tab: AppTab.project,
      loader: ServiceLocator.projects.getCurrentProject,
      subtitleBuilder: (p) => p.title,
      actionBuilder: (_, __) => const IconSquareButton(icon: AppIcons.menu),
      builder: (context, p) => SingleChildScrollView(
        physics: const ClampingScrollPhysics(),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // `.workspace-head`
            Container(
              padding: const EdgeInsets.all(18),
              color: AppColors.navy900,
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const StatusBadge('In progress'),
                        Padding(
                          padding: const EdgeInsets.fromLTRB(0, 7, 0, 3),
                          child: Text(p.title,
                              style: AppText.s(20, w: FontWeight.w700, color: Colors.white, ls: -.3)),
                        ),
                        Text(p.tagline, style: AppText.s(9, color: const Color(0xFF9FB0C8))),
                      ],
                    ),
                  ),
                  const SizedBox(width: 12),
                  SizedBox(
                    width: 60,
                    height: 60,
                    child: CustomPaint(
                      painter: RingPainter(
                        value: p.progress,
                        trackColor: const Color(0xFF31445F),
                        fillColor: const Color(0xFF6793F2),
                        radiusRatio: 18 / 44,
                        strokeRatio: 5 / 44,
                      ),
                      child: Center(
                        child: Text('${p.progress.round()}%',
                            style: AppText.s(11, w: FontWeight.w700, color: Colors.white)),
                      ),
                    ),
                  ),
                ],
              ),
            ),
            // `.tabs`
            Container(
              height: 43,
              padding: const EdgeInsets.symmetric(horizontal: 10),
              decoration: const BoxDecoration(
                color: Colors.white,
                border: Border(bottom: BorderSide(color: AppColors.line)),
              ),
              child: Row(
                children: [
                  for (final t in _tabs)
                    Expanded(
                      child: GestureDetector(
                        behavior: HitTestBehavior.opaque,
                        onTap: () => _onTab(context, t),
                        child: Container(
                          alignment: Alignment.center,
                          decoration: BoxDecoration(
                            border: Border(
                              bottom: BorderSide(
                                color: t == 'Overview' ? AppColors.blue700 : Colors.transparent,
                                width: 2,
                              ),
                            ),
                          ),
                          child: Text(t,
                              style: AppText.s(8.5,
                                  w: FontWeight.w700,
                                  color: t == 'Overview' ? AppColors.blue700 : AppColors.muted)),
                        ),
                      ),
                    ),
                ],
              ),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(17, 17, 17, 26),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  // `.current-milestone`
                  Container(
                    padding: const EdgeInsets.all(14),
                    decoration: BoxDecoration(
                      color: Colors.white,
                      border: Border.all(color: const Color(0xFFD8E0ED)),
                      borderRadius: BorderRadius.circular(15),
                      boxShadow: AppShadows.card,
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        SectionHeading(p.currentMilestone,
                            overline: 'CURRENT MILESTONE',
                            trailingWidget: const StatusBadge('In progress', tone: BadgeTone.orange)),
                        const AppProgress(value: 70),
                        Padding(
                          padding: const EdgeInsets.fromLTRB(0, 10, 0, 13),
                          child: Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Row(
                                children: [
                                  const AppIcon(AppIcons.calendar, size: 17, color: AppColors.muted),
                                  const SizedBox(width: 5),
                                  Text(p.dueLabel, style: AppText.s(8.5, color: AppColors.muted)),
                                ],
                              ),
                              Text('${p.daysLeft} days left',
                                  style: AppText.s(8.5, w: FontWeight.w800, color: AppColors.orange700)),
                            ],
                          ),
                        ),
                        AppButton(
                          label: 'View milestone details',
                          icon: AppIcons.arrow,
                          variant: ButtonVariant.outline,
                          full: true,
                          onPressed: () => AppNav.push(context, Routes.milestone),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 20),
                  const SectionHeading('Project overview'),
                  Row(
                    children: [
                      Expanded(child: _OverviewCell(icon: AppIcons.user, label: 'Supervisor', value: p.supervisorName)),
                      const SizedBox(width: 8),
                      Expanded(child: _OverviewCell(icon: AppIcons.team, label: 'Team', value: '${p.teamSize} members')),
                    ],
                  ),
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      Expanded(child: _OverviewCell(icon: AppIcons.calendar, label: 'Final deadline', value: p.finalDeadline)),
                      const SizedBox(width: 8),
                      Expanded(child: _OverviewCell(icon: AppIcons.file, label: 'Submissions', value: '${p.submissionCount} versions')),
                    ],
                  ),
                  const SizedBox(height: 20),
                  SectionHeading('Latest activity',
                      actionLabel: 'View all',
                      onAction: () => AppNav.go(context, Routes.notifications)),
                  Material(
                    color: Colors.white,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                      side: const BorderSide(color: AppColors.line),
                    ),
                    child: InkWell(
                      borderRadius: BorderRadius.circular(12),
                      onTap: () => AppNav.push(context, Routes.feedback),
                      child: Padding(
                        padding: const EdgeInsets.all(11),
                        child: Row(
                          children: [
                            ListIcon(
                              size: 33,
                              tone: IconTone.orange,
                              child: (c) => AppIcon(AppIcons.message, color: c),
                            ),
                            const SizedBox(width: 9),
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text('New supervisor feedback',
                                      style: AppText.s(9, w: FontWeight.w700)),
                                  Padding(
                                    padding: const EdgeInsets.symmetric(vertical: 1),
                                    child: Text('System Design · Version 2',
                                        style: AppText.s(8, color: AppColors.muted, height: 1.6)),
                                  ),
                                  Text('Today, 9:42 AM',
                                      style: AppText.s(8, color: const Color(0xFF97A1B0))),
                                ],
                              ),
                            ),
                            const AppIcon(AppIcons.chevron),
                          ],
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _OverviewCell extends StatelessWidget {
  const _OverviewCell({required this.icon, required this.label, required this.value});

  final AppIcons icon;
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(10),
      decoration: BoxDecoration(
        color: Colors.white,
        border: Border.all(color: AppColors.line),
        borderRadius: BorderRadius.circular(11),
      ),
      child: Row(
        children: [
          AppIcon(icon, color: AppColors.blue700),
          const SizedBox(width: 7),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(label, style: AppText.s(7.5, color: AppColors.muted, height: 1.6)),
                const SizedBox(height: 2),
                Text(value, style: AppText.s(8.5, w: FontWeight.w700, color: AppColors.navy900)),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

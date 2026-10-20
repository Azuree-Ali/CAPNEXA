import 'package:flutter/material.dart';

import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_progress.dart';
import 'package:capnexa/core/widgets/status_badge.dart';
import 'package:capnexa/models/project.dart';

/// `.project-hero` – navy "current project" card on the Home dashboard.
class ProjectHeroCard extends StatelessWidget {
  const ProjectHeroCard({super.key, required this.project});

  final Project project;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(17),
      decoration: BoxDecoration(
        color: AppColors.navy900,
        borderRadius: BorderRadius.circular(16),
        boxShadow: AppShadows.hero,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text('CURRENT PROJECT', style: AppText.overline),
                    const SizedBox(height: 3),
                    Text(project.title,
                        style: AppText.s(18, w: FontWeight.w700, color: Colors.white, ls: -.3)),
                  ],
                ),
              ),
              const StatusBadge('In progress'),
            ],
          ),
          Padding(
            padding: const EdgeInsets.fromLTRB(0, 4, 0, 16),
            child: Text(project.tagline, style: AppText.s(11, color: AppColors.heroSub, height: 1.6)),
          ),
          AppProgress(
            value: project.progress,
            label: 'Overall project progress',
            trackColor: AppColors.heroTrack,
            fillColor: AppColors.heroFill,
            labelColor: AppColors.heroSub,
            valueColor: Colors.white,
          ),
          const SizedBox(height: 12),
          Container(
            padding: const EdgeInsets.only(top: 12),
            decoration: const BoxDecoration(
              border: Border(top: BorderSide(color: Color(0xFF29415F))),
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Flexible(
                  child: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      const AppIcon(AppIcons.project, size: 17, color: Colors.white),
                      const SizedBox(width: 6),
                      Flexible(
                        child: Text(project.currentMilestone,
                            overflow: TextOverflow.ellipsis,
                            style: AppText.s(9, w: FontWeight.w700, color: Colors.white)),
                      ),
                    ],
                  ),
                ),
                Text('Milestone ${project.milestoneNumber} of ${project.milestoneCount}',
                    style: AppText.s(9, color: const Color(0xFFBAC6D8))),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

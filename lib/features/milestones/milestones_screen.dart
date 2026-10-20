import 'package:flutter/material.dart';

import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_progress.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/async_scaffold.dart';
import 'package:capnexa/features/milestones/widgets/milestone_card.dart';
import 'package:capnexa/models/milestone.dart';
import 'package:capnexa/services/service_locator.dart';

class MilestonesScreen extends StatelessWidget {
  const MilestonesScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AsyncScaffold<MilestonePlan>(
      title: 'Project milestones',
      tab: AppTab.project,
      loader: ServiceLocator.milestones.getPlan,
      subtitleBuilder: (p) => '${p.inProgressNumber} of ${p.milestones.length} in progress',
      builder: (context, plan) => ScreenBody(
        children: [
          Container(
            padding: const EdgeInsets.all(15),
            decoration: BoxDecoration(
              color: AppColors.navy900,
              borderRadius: BorderRadius.circular(15),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    Text('${plan.overallProgress.round()}%',
                        style: AppText.s(24, w: FontWeight.w700, color: Colors.white)),
                    Text('Overall progress', style: AppText.s(8, color: AppColors.heroSub)),
                  ],
                ),
                const SizedBox(height: 11),
                AppProgress(
                  value: plan.overallProgress,
                  trackColor: AppColors.heroTrack,
                  fillColor: AppColors.heroFill,
                ),
                const SizedBox(height: 11),
                Row(
                  children: [
                    const AppIcon(AppIcons.calendar, size: 16, color: Color(0xFFBDC8D8)),
                    const SizedBox(width: 5),
                    Text('Final submission · ${plan.finalSubmission}',
                        style: AppText.s(8.5, color: const Color(0xFFBDC8D8))),
                  ],
                ),
              ],
            ),
          ),
          Padding(
            padding: const EdgeInsets.only(left: 4),
            child: Column(
              children: [
                for (var i = 0; i < plan.milestones.length; i++)
                  MilestoneTimelineCard(
                    milestone: plan.milestones[i],
                    index: i,
                    isLast: i == plan.milestones.length - 1,
                    onTap: plan.milestones[i].status == MilestoneStatus.inProgress
                        ? () => AppNav.push(context, Routes.milestone)
                        : null,
                  ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

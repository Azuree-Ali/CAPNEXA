import 'package:flutter/material.dart';

import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/avatar_box.dart';
import 'package:capnexa/core/widgets/status_badge.dart';
import 'package:capnexa/models/milestone.dart';

/// `.deadline-card` – upcoming milestone deadline.
class DeadlineCard extends StatelessWidget {
  const DeadlineCard({super.key, required this.detail, this.onTap});

  final MilestoneDetail detail;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: const Color(0xFFFFFAF4),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(14),
        side: const BorderSide(color: Color(0xFFF2D8B8)),
      ),
      child: InkWell(
        borderRadius: BorderRadius.circular(14),
        onTap: onTap,
        child: Padding(
          padding: const EdgeInsets.all(13),
          child: Row(
            children: [
              DateBlock(day: detail.day, month: detail.month),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    StatusBadge('Due in ${detail.milestone.daysLeft} days', tone: BadgeTone.orange),
                    const SizedBox(height: 6),
                    Text('${detail.milestone.name} submission', style: AppText.h3),
                    const SizedBox(height: 1),
                    Text(detail.deadlineLabel, style: AppText.s(9, color: AppColors.muted, height: 1.6)),
                  ],
                ),
              ),
              const AppIcon(AppIcons.chevron),
            ],
          ),
        ),
      ),
    );
  }
}

/// One tile of the 2-column "Quick actions" grid.
class QuickActionTile extends StatelessWidget {
  const QuickActionTile({super.key, required this.icon, required this.label, required this.onTap});

  final AppIcons icon;
  final String label;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final radius = BorderRadius.circular(13);
    return Material(
      color: Colors.white,
      shape: RoundedRectangleBorder(borderRadius: radius, side: const BorderSide(color: AppColors.line)),
      child: InkWell(
        borderRadius: radius,
        onTap: onTap,
        child: Container(
          constraints: const BoxConstraints(minHeight: 57),
          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 9),
          child: Row(
            children: [
              Container(
                width: 31,
                height: 31,
                alignment: Alignment.center,
                decoration: BoxDecoration(color: AppColors.blue50, borderRadius: BorderRadius.circular(9)),
                child: AppIcon(icon, color: AppColors.blue700),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: Text(label,
                    style: AppText.s(9.5, w: FontWeight.w700, color: AppColors.navy900)),
              ),
              const AppIcon(AppIcons.chevron, size: 16, color: AppColors.chevron),
            ],
          ),
        ),
      ),
    );
  }
}

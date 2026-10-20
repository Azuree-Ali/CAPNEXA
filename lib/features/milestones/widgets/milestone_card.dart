import 'package:flutter/material.dart';

import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_progress.dart';
import 'package:capnexa/core/widgets/status_badge.dart';
import 'package:capnexa/core/widgets/timeline_item.dart';
import 'package:capnexa/models/milestone.dart';

/// One row of the milestones timeline: marker + `.timeline-card`.
class MilestoneTimelineCard extends StatelessWidget {
  const MilestoneTimelineCard({
    super.key,
    required this.milestone,
    required this.index,
    required this.isLast,
    this.onTap,
  });

  final Milestone milestone;
  final int index; // zero-based
  final bool isLast;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final m = milestone;
    final state = switch (m.status) {
      MilestoneStatus.completed => TimelineState.done,
      MilestoneStatus.inProgress => TimelineState.current,
      MilestoneStatus.upcoming => TimelineState.pending,
    };
    final (label, tone) = switch (m.status) {
      MilestoneStatus.completed => ('Completed', BadgeTone.teal),
      MilestoneStatus.inProgress => ('In Progress', BadgeTone.orange),
      MilestoneStatus.upcoming => ('Upcoming', BadgeTone.gray),
    };
    final active = m.status == MilestoneStatus.inProgress;
    return TimelineItem(
      isLast: isLast,
      marker: TimelineMarker(
        state: state,
        index: index + 1,
        doneColor: active ? AppColors.blue700 : AppColors.teal700,
        doneRing: AppColors.blue100,
      ),
      content: Padding(
        padding: const EdgeInsets.only(bottom: 13),
        child: Material(
          color: Colors.white,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
            side: const BorderSide(color: AppColors.line),
          ),
          child: InkWell(
            borderRadius: BorderRadius.circular(12),
            onTap: onTap,
            child: Padding(
              padding: const EdgeInsets.all(11),
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
                            Text(m.dateRange, style: AppText.overline),
                            const SizedBox(height: 3),
                            Text(m.name, style: AppText.h3),
                          ],
                        ),
                      ),
                      const SizedBox(width: 8),
                      StatusBadge(label, tone: tone),
                    ],
                  ),
                  if (active) ...[
                    const SizedBox(height: 8),
                    AppProgress(value: m.progress),
                    const SizedBox(height: 8),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text('Submission pending', style: AppText.s(8, color: AppColors.muted)),
                        Text('${m.daysLeft} days left',
                            style: AppText.s(8, w: FontWeight.w700, color: AppColors.orange700)),
                      ],
                    ),
                  ],
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

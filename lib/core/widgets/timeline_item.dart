import 'package:flutter/material.dart';

import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_icon.dart';

enum TimelineState { pending, done, current }

/// Round 25px marker used by both timelines
/// (`.timeline-marker` and `.review-timeline > div > span`).
class TimelineMarker extends StatelessWidget {
  const TimelineMarker({
    super.key,
    required this.state,
    required this.index,
    this.doneColor = AppColors.teal700,
    this.doneRing = AppColors.teal100,
    this.pendingBorder = const Color(0xFFD7DDE6),
    this.pendingText = const Color(0xFF8A95A6),
  });

  final TimelineState state;
  final int index;
  final Color doneColor;
  final Color doneRing;
  final Color pendingBorder;
  final Color pendingText;

  @override
  Widget build(BuildContext context) {
    final filled = state != TimelineState.pending;
    return Container(
      width: 25,
      height: 25,
      alignment: Alignment.center,
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        color: filled ? doneColor : Colors.white,
        border: Border.all(color: filled ? doneColor : pendingBorder),
        boxShadow: state == TimelineState.current
            ? [BoxShadow(color: doneRing, spreadRadius: 4)]
            : null,
      ),
      child: filled
          ? const AppIcon(AppIcons.check, size: 14, color: Colors.white)
          : Text('$index', style: AppText.s(8, w: FontWeight.w800, color: pendingText)),
    );
  }
}

/// Vertical timeline row: marker + connector line + content.
class TimelineItem extends StatelessWidget {
  const TimelineItem({
    super.key,
    required this.marker,
    required this.content,
    this.isLast = false,
    this.gap = 10,
  });

  final Widget marker;
  final Widget content;
  final bool isLast;
  final double gap;

  @override
  Widget build(BuildContext context) {
    return IntrinsicHeight(
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          SizedBox(
            width: 25,
            child: Column(
              children: [
                marker,
                if (!isLast)
                  Expanded(
                    child: Container(width: 1, color: const Color(0xFFDCE2EA)),
                  ),
              ],
            ),
          ),
          SizedBox(width: gap),
          Expanded(child: content),
        ],
      ),
    );
  }
}

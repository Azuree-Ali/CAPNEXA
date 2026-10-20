import 'package:flutter/material.dart';

import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';

enum BadgeTone { blue, teal, orange, red, gray }

/// `.badge` – pill with a dot, used for every status in the app.
class StatusBadge extends StatelessWidget {
  const StatusBadge(this.label, {super.key, this.tone = BadgeTone.blue});

  final String label;
  final BadgeTone tone;

  (Color, Color) get _colors => switch (tone) {
        BadgeTone.blue => (AppColors.blue700, AppColors.blue100),
        BadgeTone.teal => (AppColors.teal700, AppColors.teal100),
        BadgeTone.orange => (AppColors.orange700, AppColors.orange100),
        BadgeTone.red => (AppColors.red700, AppColors.red100),
        BadgeTone.gray => (const Color(0xFF5F6B7C), const Color(0xFFEDF0F4)),
      };

  @override
  Widget build(BuildContext context) {
    final (fg, bg) = _colors;
    return Container(
      constraints: const BoxConstraints(minHeight: 21),
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(color: bg, borderRadius: BorderRadius.circular(20)),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Container(
            width: 5,
            height: 5,
            decoration: BoxDecoration(color: fg, shape: BoxShape.circle),
          ),
          const SizedBox(width: 5),
          Flexible(
            child: Text(label, style: AppText.s(8.5, w: FontWeight.w800, color: fg)),
          ),
        ],
      ),
    );
  }
}

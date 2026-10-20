import 'dart:math' as math;

import 'package:flutter/material.dart';

import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';

/// `.progress` – optional label row + rounded track.
class AppProgress extends StatelessWidget {
  const AppProgress({
    super.key,
    required this.value,
    this.label,
    this.height = 6,
    this.trackColor = const Color(0xFFE6EAF0),
    this.fillColor = AppColors.blue600,
    this.labelColor = AppColors.muted,
    this.valueColor = AppColors.navy900,
  });

  /// 0 – 100
  final double value;
  final String? label;
  final double height;
  final Color trackColor;
  final Color fillColor;
  final Color labelColor;
  final Color valueColor;

  @override
  Widget build(BuildContext context) {
    final double factor = math.max(0.0, math.min(1.0, value / 100));
    final track = ClipRRect(
      borderRadius: BorderRadius.circular(6),
      child: Container(
        height: height,
        color: trackColor,
        alignment: Alignment.centerLeft,
        child: TweenAnimationBuilder<double>(
          tween: Tween<double>(begin: 0.0, end: factor),
          duration: const Duration(milliseconds: 500),
          curve: Curves.easeOut,
          builder: (context, f, _) => FractionallySizedBox(
            widthFactor: f,
            child: Container(
              decoration: BoxDecoration(
                color: fillColor,
                borderRadius: BorderRadius.circular(6),
              ),
            ),
          ),
        ),
      ),
    );
    if (label == null) return track;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(label!, style: AppText.s(9.5, color: labelColor)),
            Text('${value.round()}%',
                style: AppText.s(10, w: FontWeight.w700, color: valueColor)),
          ],
        ),
        const SizedBox(height: 7),
        track,
      ],
    );
  }
}

/// Circular progress used by the project workspace header (`.ring`,
/// SVG viewBox 44, r=18, stroke 5) and the similarity score (`.score-ring`,
/// viewBox 120, r=50, stroke 8). Geometry is stored as ratios of the box.
class RingPainter extends CustomPainter {
  RingPainter({
    required this.value,
    required this.trackColor,
    required this.fillColor,
    required this.radiusRatio,
    required this.strokeRatio,
  });

  final double value; // 0-100
  final Color trackColor;
  final Color fillColor;
  final double radiusRatio;
  final double strokeRatio;

  @override
  void paint(Canvas canvas, Size size) {
    final strokeWidth = size.width * strokeRatio;
    final radius = size.width * radiusRatio;
    final rect = Rect.fromCircle(center: size.center(Offset.zero), radius: radius);
    final track = Paint()
      ..color = trackColor
      ..style = PaintingStyle.stroke
      ..strokeWidth = strokeWidth;
    final fill = Paint()
      ..color = fillColor
      ..style = PaintingStyle.stroke
      ..strokeCap = StrokeCap.round
      ..strokeWidth = strokeWidth;
    const full = 6.283185307179586;
    canvas.drawArc(rect, 0, full, false, track);
    canvas.drawArc(rect, -full / 4, full * (value / 100), false, fill);
  }

  @override
  bool shouldRepaint(RingPainter old) =>
      old.value != value ||
      old.trackColor != trackColor ||
      old.fillColor != fillColor ||
      old.radiusRatio != radiusRatio ||
      old.strokeRatio != strokeRatio;
}

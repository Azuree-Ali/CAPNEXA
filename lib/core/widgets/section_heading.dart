import 'package:flutter/material.dart';

import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';

/// `.section-heading` – h2 (+ optional overline) with an action link or hint.
class SectionHeading extends StatelessWidget {
  const SectionHeading(
    this.title, {
    super.key,
    this.overline,
    this.actionLabel,
    this.onAction,
    this.trailing,
    this.trailingWidget,
  });

  final String title;
  final String? overline;
  final String? actionLabel;
  final VoidCallback? onAction;
  final String? trailing;
  final Widget? trailingWidget;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 9),
      child: ConstrainedBox(
        constraints: const BoxConstraints(minHeight: 24),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.center,
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  if (overline != null) Text(overline!, style: AppText.overline),
                  Text(title, style: AppText.h2),
                ],
              ),
            ),
            if (actionLabel != null)
              GestureDetector(
                onTap: onAction,
                behavior: HitTestBehavior.opaque,
                child: Padding(
                  padding: const EdgeInsets.symmetric(vertical: 4),
                  child: Text(actionLabel!,
                      style: AppText.s(10, w: FontWeight.w700, color: AppColors.blue700)),
                ),
              ),
            if (trailing != null)
              Text(trailing!, style: AppText.s(10, color: AppColors.muted)),
            if (trailingWidget != null) trailingWidget!,
          ],
        ),
      ),
    );
  }
}

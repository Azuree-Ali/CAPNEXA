import 'package:flutter/material.dart';

import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_icon.dart';

enum StateKind { loading, empty, error, success }

/// `.state-card` – reusable loading / empty / error / success pattern.
class StateCard extends StatelessWidget {
  const StateCard({
    super.key,
    required this.kind,
    required this.title,
    required this.message,
    this.actionLabel,
    this.actionIcon,
    this.onAction,
  });

  final StateKind kind;
  final String title;
  final String message;
  final String? actionLabel;
  final AppIcons? actionIcon;
  final VoidCallback? onAction;

  @override
  Widget build(BuildContext context) {
    Widget symbol;
    if (kind == StateKind.loading) {
      symbol = const Padding(
        padding: EdgeInsets.all(3),
        child: SizedBox(
          width: 33,
          height: 33,
          child: CircularProgressIndicator(
            strokeWidth: 3,
            color: AppColors.blue700,
            backgroundColor: Color(0xFFDCE5FA),
          ),
        ),
      );
    } else {
      final (icon, fg, bg) = switch (kind) {
        StateKind.error => (AppIcons.warning, AppColors.red700, AppColors.red100),
        StateKind.success => (AppIcons.check, AppColors.teal700, AppColors.teal100),
        _ => (AppIcons.file, AppColors.blue700, AppColors.blue100),
      };
      symbol = Container(
        width: 40,
        height: 40,
        alignment: Alignment.center,
        decoration: BoxDecoration(color: bg, borderRadius: BorderRadius.circular(11)),
        child: AppIcon(icon, color: fg),
      );
    }
    return Container(
      padding: const EdgeInsets.all(15),
      decoration: BoxDecoration(
        color: Colors.white,
        border: Border.all(color: AppColors.line),
        borderRadius: BorderRadius.circular(14),
      ),
      child: Row(
        children: [
          symbol,
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(title, style: AppText.s(11, w: FontWeight.w700, color: AppColors.navy900)),
                Padding(
                  padding: const EdgeInsets.symmetric(vertical: 3),
                  child: Text(message, style: AppText.p),
                ),
                if (actionLabel != null)
                  AppButton(
                    label: actionLabel!,
                    icon: actionIcon,
                    variant: ButtonVariant.text,
                    onPressed: onAction,
                  ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

/// Full-area spinner used while a screen loads its data.
class LoadingView extends StatelessWidget {
  const LoadingView({super.key});

  @override
  Widget build(BuildContext context) {
    return const Center(
      child: SizedBox(
        width: 33,
        height: 33,
        child: CircularProgressIndicator(
          strokeWidth: 3,
          color: AppColors.blue700,
          backgroundColor: Color(0xFFDCE5FA),
        ),
      ),
    );
  }
}

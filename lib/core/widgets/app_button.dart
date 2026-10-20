import 'package:flutter/material.dart';

import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_icon.dart';

enum ButtonVariant { primary, secondary, outline, text, danger }

/// `.btn` – primary / secondary / outline / text / danger.
class AppButton extends StatelessWidget {
  const AppButton({
    super.key,
    required this.label,
    this.onPressed,
    this.icon,
    this.variant = ButtonVariant.primary,
    this.full = false,
  });

  final String label;
  final VoidCallback? onPressed;
  final AppIcons? icon;
  final ButtonVariant variant;
  final bool full;

  @override
  Widget build(BuildContext context) {
    late Color bg;
    late Color fg;
    Color? border;
    var shadow = false;
    var minHeight = 43.0;
    var hPad = 16.0;
    switch (variant) {
      case ButtonVariant.primary:
        bg = AppColors.navy900;
        fg = Colors.white;
        shadow = true;
      case ButtonVariant.secondary:
        bg = Colors.white;
        fg = AppColors.navy900;
      case ButtonVariant.outline:
        bg = Colors.white;
        fg = AppColors.navy800;
        border = const Color(0xFFCBD4E1);
      case ButtonVariant.text:
        bg = Colors.transparent;
        fg = AppColors.blue700;
        minHeight = 32;
        hPad = 7;
      case ButtonVariant.danger:
        bg = Colors.white;
        fg = AppColors.red700;
        border = const Color(0xFFEFC9CC);
    }
    final radius = BorderRadius.circular(11);
    final content = Row(
      mainAxisSize: full ? MainAxisSize.max : MainAxisSize.min,
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Flexible(
          child: Text(
            label,
            textAlign: TextAlign.center,
            style: AppText.s(11, w: FontWeight.w800, color: fg),
          ),
        ),
        if (icon != null) ...[
          const SizedBox(width: 9),
          AppIcon(icon!, size: 18, color: fg),
        ],
      ],
    );
    final button = DecoratedBox(
      decoration: BoxDecoration(
        color: bg,
        borderRadius: radius,
        border: Border.all(color: border ?? Colors.transparent),
        boxShadow: shadow ? AppShadows.button : null,
      ),
      child: Material(
        type: MaterialType.transparency,
        child: InkWell(
          borderRadius: radius,
          onTap: onPressed,
          child: Container(
            constraints: BoxConstraints(minHeight: minHeight),
            padding: EdgeInsets.symmetric(horizontal: hPad),
            alignment: Alignment.center,
            child: content,
          ),
        ),
      ),
    );
    final result = Opacity(opacity: onPressed == null ? .58 : 1, child: button);
    return full ? SizedBox(width: double.infinity, child: result) : result;
  }
}

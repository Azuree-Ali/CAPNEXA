import 'package:flutter/material.dart';

import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_icon.dart';

/// `.info-note` / `.integrity-note` – tinted callout with a leading icon.
class InfoNote extends StatelessWidget {
  const InfoNote({
    super.key,
    required this.icon,
    required this.message,
    this.title,
    this.iconSize = 18,
    this.bordered = false,
    this.padding = 11,
    this.radius = 11,
    this.textColor = const Color(0xFF52698F),
    this.titleColor = AppColors.navy800,
  });

  final AppIcons icon;
  final String message;
  final String? title;
  final double iconSize;
  final bool bordered;
  final double padding;
  final double radius;
  final Color textColor;
  final Color titleColor;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.all(padding),
      decoration: BoxDecoration(
        color: AppColors.blue50,
        borderRadius: BorderRadius.circular(radius),
        border: bordered ? Border.all(color: const Color(0xFFC5D5F7)) : null,
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          AppIcon(icon, size: iconSize, color: AppColors.blue700),
          SizedBox(width: bordered ? 9 : 9),
          Expanded(
            child: Text.rich(
              TextSpan(children: [
                if (title != null)
                  TextSpan(
                      text: '$title\n',
                      style: AppText.s(9, w: FontWeight.w700, color: titleColor, height: 1.5)),
                TextSpan(text: message, style: AppText.s(bordered ? 8.5 : 9, color: textColor, height: 1.5)),
              ]),
            ),
          ),
        ],
      ),
    );
  }
}

import 'package:flutter/material.dart';

import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_card.dart';
import 'package:capnexa/core/widgets/app_dialog.dart';
import 'package:capnexa/core/widgets/app_icon.dart';

/// `.settings-list` – account settings rows (shared by Profile and Settings).
class SettingsList extends StatelessWidget {
  const SettingsList({super.key});

  static const _items = <(AppIcons, String, String)>[
    (AppIcons.edit, 'Edit profile', 'Update your student details'),
    (AppIcons.lock, 'Change password', 'Last changed 3 months ago'),
    (AppIcons.bell, 'Notification settings', 'Email and push preferences'),
    (AppIcons.shield, 'Privacy & security', 'Sessions and data controls'),
  ];

  @override
  Widget build(BuildContext context) {
    return DividedList(
      children: [
        for (final item in _items)
          InkWell(
            onTap: () => showAppSnack(context, '${item.$2} will be available with the API.'),
            child: Padding(
              padding: const EdgeInsets.all(10),
              child: Row(
                children: [
                  Container(
                    width: 31,
                    height: 31,
                    alignment: Alignment.center,
                    decoration: BoxDecoration(
                      color: const Color(0xFFF0F3F7),
                      borderRadius: BorderRadius.circular(9),
                    ),
                    child: AppIcon(item.$1, color: AppColors.navy800),
                  ),
                  const SizedBox(width: 9),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(item.$2,
                            style: AppText.s(9, w: FontWeight.w700, color: AppColors.navy900)),
                        Text(item.$3, style: AppText.s(7.5, color: AppColors.muted, height: 1.6)),
                      ],
                    ),
                  ),
                  const AppIcon(AppIcons.chevron),
                ],
              ),
            ),
          ),
      ],
    );
  }
}

import 'package:flutter/material.dart';

import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/avatar_box.dart';
import 'package:capnexa/models/app_notification.dart';

/// Icon + tint used for each notification type (matches the Figma list).
(AppIcons, IconTone) notificationStyle(NotificationKind kind) => switch (kind) {
      NotificationKind.feedback => (AppIcons.message, IconTone.orange),
      NotificationKind.similarity => (AppIcons.spark, IconTone.blue),
      NotificationKind.deadline => (AppIcons.calendar, IconTone.orange),
      NotificationKind.supervisor => (AppIcons.user, IconTone.teal),
      NotificationKind.approval => (AppIcons.check, IconTone.teal),
    };

/// `.notification-list > button` – full-bleed notification row.
class NotificationCard extends StatelessWidget {
  const NotificationCard({super.key, required this.notification, this.onTap});

  final AppNotification notification;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final (icon, tone) = notificationStyle(notification.kind);
    return Material(
      color: notification.unread ? const Color(0xFFF7F9FF) : Colors.white,
      child: InkWell(
        onTap: onTap,
        child: Container(
          width: double.infinity,
          padding: const EdgeInsets.symmetric(horizontal: 17, vertical: 13),
          decoration: const BoxDecoration(
            border: Border(bottom: BorderSide(color: AppColors.line)),
          ),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              ListIcon(tone: tone, child: (c) => AppIcon(icon, color: c)),
              const SizedBox(width: 10),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(notification.title,
                        style: AppText.s(9.5, w: FontWeight.w700, color: AppColors.navy900)),
                    const SizedBox(height: 2),
                    Text(notification.message,
                        style: AppText.s(8, color: AppColors.muted, height: 1.4)),
                    const SizedBox(height: 4),
                    Text(notification.time, style: AppText.s(7, color: const Color(0xFF9CA5B2))),
                  ],
                ),
              ),
              if (notification.unread)
                Container(
                  width: 6,
                  height: 6,
                  margin: const EdgeInsets.only(top: 5),
                  decoration: const BoxDecoration(color: AppColors.blue600, shape: BoxShape.circle),
                ),
            ],
          ),
        ),
      ),
    );
  }
}

/// `.compact-list > div` – row used by "Recent updates" on Home.
class CompactUpdateRow extends StatelessWidget {
  const CompactUpdateRow({
    super.key,
    required this.icon,
    required this.tone,
    required this.title,
    required this.subtitle,
    required this.time,
  });

  final AppIcons icon;
  final IconTone tone;
  final String title;
  final String subtitle;
  final String time;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(11),
      child: Row(
        children: [
          ListIcon(tone: tone, child: (c) => AppIcon(icon, color: c)),
          const SizedBox(width: 9),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(title, style: AppText.s(9.5, w: FontWeight.w700, color: AppColors.navy900)),
                Text(subtitle, style: AppText.s(8, color: AppColors.muted, height: 1.6)),
              ],
            ),
          ),
          Text(time, style: AppText.s(8, color: const Color(0xFF97A1B0))),
        ],
      ),
    );
  }
}

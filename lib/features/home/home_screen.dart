import 'package:flutter/material.dart';

import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_card.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/async_scaffold.dart';
import 'package:capnexa/core/widgets/avatar_box.dart';
import 'package:capnexa/core/widgets/section_heading.dart';
import 'package:capnexa/core/widgets/status_badge.dart';
import 'package:capnexa/features/home/widgets/home_widgets.dart';
import 'package:capnexa/features/notifications/widgets/notification_card.dart';
import 'package:capnexa/features/projects/widgets/project_hero_card.dart';
import 'package:capnexa/models/app_notification.dart';
import 'package:capnexa/models/home_summary.dart';
import 'package:capnexa/services/service_locator.dart';

class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AsyncScaffold<HomeSummary>(
      title: 'Good morning',
      subtitle: 'Here’s where your project stands',
      tab: AppTab.home,
      loader: ServiceLocator.home.getSummary,
      titleBuilder: (d) => 'Good morning, ${d.student.firstName}',
      actionBuilder: (context, d) =>
          AvatarBox(d.student.initials, onTap: () => AppNav.go(context, Routes.profile)),
      builder: (context, d) {
        void push(String route) => AppNav.push(context, route);
        final quick = <(AppIcons, String, String)>[
          (AppIcons.project, 'View project', Routes.project),
          (AppIcons.upload, 'Submit report', Routes.submission),
          (AppIcons.team, 'View team', Routes.team),
          (AppIcons.message, 'View feedback', Routes.feedback),
        ];
        return ScreenBody(
          children: [
            ProjectHeroCard(project: d.project),
            Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                SectionHeading('Upcoming deadline',
                    overline: 'NEXT UP',
                    actionLabel: 'View timeline',
                    onAction: () => push(Routes.milestones)),
                DeadlineCard(detail: d.nextDeadline, onTap: () => push(Routes.milestone)),
              ],
            ),
            Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const SectionHeading('Quick actions'),
                for (var i = 0; i < quick.length; i += 2)
                  Padding(
                    padding: EdgeInsets.only(bottom: i + 2 < quick.length ? 8 : 0),
                    child: Row(
                      children: [
                        for (var j = i; j < i + 2; j++) ...[
                          Expanded(
                            child: QuickActionTile(
                              icon: quick[j].$1,
                              label: quick[j].$2,
                              onTap: () => push(quick[j].$3),
                            ),
                          ),
                          if (j == i) const SizedBox(width: 8),
                        ],
                      ],
                    ),
                  ),
              ],
            ),
            Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                SectionHeading('Latest supervisor feedback',
                    actionLabel: 'View all', onAction: () => push(Routes.feedback)),
                AppCard(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      Row(
                        children: [
                          AvatarBox(d.latestFeedback.authorInitials,
                              size: 32, radius: 10),
                          const SizedBox(width: 9),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(d.latestFeedback.authorName,
                                    style: AppText.s(10, w: FontWeight.w700)),
                                const SizedBox(height: 2),
                                Text(d.latestFeedback.homeTimestamp,
                                    style: AppText.s(8, color: AppColors.muted)),
                              ],
                            ),
                          ),
                          const StatusBadge('Action needed', tone: BadgeTone.orange),
                        ],
                      ),
                      Container(
                        margin: const EdgeInsets.only(top: 11),
                        padding: const EdgeInsets.all(10),
                        decoration: BoxDecoration(
                          color: const Color(0xFFF7F8FB),
                          borderRadius: BorderRadius.circular(9),
                        ),
                        child: Text('“${d.latestFeedback.homeExcerpt}”',
                            style: AppText.s(11, color: const Color(0xFF4C586A), height: 1.55)),
                      ),
                    ],
                  ),
                ),
              ],
            ),
            Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                SectionHeading('Recent updates',
                    actionLabel: 'All notifications',
                    onAction: () => AppNav.go(context, Routes.notifications)),
                DividedList(
                  children: [
                    for (final u in d.updates)
                      CompactUpdateRow(
                        icon: _updateStyle(u.kind).$1,
                        tone: _updateStyle(u.kind).$2,
                        title: u.title,
                        subtitle: u.subtitle,
                        time: u.time,
                      ),
                  ],
                ),
              ],
            ),
          ],
        );
      },
    );
  }
}

(AppIcons, IconTone) _updateStyle(NotificationKind kind) => switch (kind) {
      NotificationKind.similarity => (AppIcons.spark, IconTone.teal),
      NotificationKind.approval => (AppIcons.check, IconTone.blue),
      NotificationKind.feedback => (AppIcons.message, IconTone.orange),
      NotificationKind.deadline => (AppIcons.calendar, IconTone.orange),
      NotificationKind.supervisor => (AppIcons.user, IconTone.teal),
    };

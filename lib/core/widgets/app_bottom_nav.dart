import 'package:flutter/material.dart';

import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_icon.dart';

enum AppTab { home, project, team, alerts, profile }

/// `.bottom-nav` – five tabs, active pill behind the icon, unread badge.
class AppBottomNav extends StatelessWidget {
  const AppBottomNav({super.key, required this.active, this.unreadCount = 3});

  final AppTab? active;
  final int unreadCount;

  static const _items = <(AppTab, AppIcons, String, String)>[
    (AppTab.home, AppIcons.home, 'Home', Routes.home),
    (AppTab.project, AppIcons.project, 'Project', Routes.project),
    (AppTab.team, AppIcons.team, 'Team', Routes.team),
    (AppTab.alerts, AppIcons.bell, 'Alerts', Routes.notifications),
    (AppTab.profile, AppIcons.user, 'Profile', Routes.profile),
  ];

  @override
  Widget build(BuildContext context) {
    return Material(
      color: const Color.fromRGBO(255, 255, 255, .97),
      child: Container(
        decoration: const BoxDecoration(border: Border(top: BorderSide(color: AppColors.line))),
        child: SafeArea(
          top: false,
          child: Padding(
            padding: const EdgeInsets.fromLTRB(7, 7, 7, 9),
            child: SizedBox(
              height: 49,
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceAround,
                children: [
                  for (final item in _items)
                    _NavButton(
                      icon: item.$2,
                      label: item.$3,
                      active: active == item.$1,
                      badge: item.$1 == AppTab.alerts ? unreadCount : 0,
                      onTap: () => AppNav.go(context, item.$4),
                    ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _NavButton extends StatelessWidget {
  const _NavButton({
    required this.icon,
    required this.label,
    required this.active,
    required this.badge,
    required this.onTap,
  });

  final AppIcons icon;
  final String label;
  final bool active;
  final int badge;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final color = active ? AppColors.blue700 : const Color(0xFF8995A7);
    return Expanded(
      child: GestureDetector(
        behavior: HitTestBehavior.opaque,
        onTap: onTap,
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            SizedBox(
              height: 30,
              child: Stack(
                alignment: Alignment.center,
                clipBehavior: Clip.none,
                children: [
                  if (active)
                    Container(
                      width: 34,
                      height: 27,
                      decoration: BoxDecoration(
                        color: AppColors.blue100,
                        borderRadius: BorderRadius.circular(9),
                      ),
                    ),
                  AppIcon(icon, color: color),
                  if (badge > 0)
                    Positioned(
                      right: -9,
                      top: 0,
                      child: Container(
                        width: 13,
                        height: 13,
                        alignment: Alignment.center,
                        decoration: BoxDecoration(
                          color: AppColors.red700,
                          shape: BoxShape.circle,
                          border: Border.all(color: Colors.white, width: 2),
                        ),
                        child: Text('$badge', style: AppText.s(7, color: Colors.white, height: 1)),
                      ),
                    ),
                ],
              ),
            ),
            const SizedBox(height: 2),
            Text(label, style: AppText.s(8, w: FontWeight.w700, color: color)),
          ],
        ),
      ),
    );
  }
}

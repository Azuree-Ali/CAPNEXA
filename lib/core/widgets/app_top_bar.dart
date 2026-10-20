import 'package:flutter/material.dart';

import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_icon.dart';

/// `.appbar` – 65px sticky header: menu button, title/subtitle, action.
class AppTopBar extends StatelessWidget {
  const AppTopBar({super.key, required this.title, this.subtitle, this.action, this.showMenu = true});

  final String title;
  final String? subtitle;
  final Widget? action;
  final bool showMenu;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: const Color.fromRGBO(255, 255, 255, .95),
      child: Container(
        decoration: const BoxDecoration(
          border: Border(bottom: BorderSide(color: AppColors.line)),
        ),
        child: SafeArea(
          bottom: false,
          child: ConstrainedBox(
            constraints: const BoxConstraints(minHeight: 65),
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 12),
              child: Row(
                children: [
                  if (showMenu) ...[
                    IconSquareButton(
                      icon: AppIcons.menu,
                      onTap: () => Scaffold.of(context).openDrawer(),
                    ),
                    const SizedBox(width: 10),
                  ],
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Text(
                          title,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: AppText.s(16, w: FontWeight.w700, color: AppColors.navy900, ls: -.25),
                        ),
                        if (subtitle != null)
                          Padding(
                            padding: const EdgeInsets.only(top: 2),
                            child: Text(
                              subtitle!,
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                              style: AppText.s(9.5, color: AppColors.muted),
                            ),
                          ),
                      ],
                    ),
                  ),
                  const SizedBox(width: 10),
                  action ??
                      IconSquareButton(
                        icon: AppIcons.bell,
                        onTap: () => AppNav.go(context, Routes.notifications),
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

/// `.icon-btn` – 34px bordered square button.
class IconSquareButton extends StatelessWidget {
  const IconSquareButton({super.key, required this.icon, this.onTap});

  final AppIcons icon;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.white,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(10),
        side: const BorderSide(color: AppColors.line),
      ),
      child: InkWell(
        customBorder: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
        onTap: onTap,
        child: SizedBox(
          width: 34,
          height: 34,
          child: Center(child: AppIcon(icon, color: AppColors.navy800)),
        ),
      ),
    );
  }
}

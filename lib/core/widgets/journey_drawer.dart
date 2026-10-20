import 'dart:math' as math;

import 'package:flutter/material.dart';

import 'package:capnexa/core/constants/app_constants.dart';
import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/brand_logo.dart';

/// "End-to-end journey" panel from the Figma prototype. Opened from the menu
/// button in every app bar; lets reviewers jump to any screen.
class JourneyDrawer extends StatelessWidget {
  const JourneyDrawer({super.key, required this.currentRoute});

  final String? currentRoute;

  @override
  Widget build(BuildContext context) {
    final width = MediaQuery.of(context).size.width;
    final groups = <String>[];
    for (final i in journeyItems) {
      if (!groups.contains(i.group)) groups.add(i.group);
    }
    return Drawer(
      width: math.min(width * .82, 320.0),
      backgroundColor: AppColors.navy950,
      surfaceTintColor: Colors.transparent,
      shape: const RoundedRectangleBorder(),
      child: SafeArea(
        child: Column(
          children: [
            Container(
              height: 72,
              padding: const EdgeInsets.symmetric(horizontal: 28),
              decoration: const BoxDecoration(
                border: Border(bottom: BorderSide(color: Color.fromRGBO(255, 255, 255, .1))),
              ),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  const BrandLogo(inverse: true),
                  GestureDetector(
                    onTap: () => Navigator.of(context).pop(),
                    child: Text('×', style: AppText.s(25, color: Colors.white)),
                  ),
                ],
              ),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(28, 25, 28, 15),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Align(
                    alignment: Alignment.centerLeft,
                    child: Text('STUDENT MOBILE APP', style: AppText.overline),
                  ),
                  Padding(
                    padding: const EdgeInsets.fromLTRB(0, 7, 0, 5),
                    child: Text('End-to-end journey',
                        style: AppText.s(20, w: FontWeight.w700, color: Colors.white, ls: -.5)),
                  ),
                  Text(
                    'Explore every stage of the ${AppConstants.brandName} experience.',
                    style: AppText.s(12, color: const Color(0xFF9AABC3), height: 1.5),
                  ),
                ],
              ),
            ),
            Expanded(
              child: ListView(
                padding: const EdgeInsets.fromLTRB(17, 5, 17, 20),
                children: [
                  for (final group in groups) ...[
                    Padding(
                      padding: const EdgeInsets.fromLTRB(11, 15, 11, 6),
                      child: Text(
                        group.toUpperCase(),
                        style: AppText.s(9, w: FontWeight.w800, color: const Color(0xFF7186A4), ls: 1.2),
                      ),
                    ),
                    for (final item in journeyItems.where((i) => i.group == group))
                      _JourneyButton(
                        index: journeyItems.indexOf(item) + 1,
                        item: item,
                        active: item.route == currentRoute,
                      ),
                  ],
                ],
              ),
            ),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 28, vertical: 17),
              decoration: const BoxDecoration(
                border: Border(top: BorderSide(color: Color.fromRGBO(255, 255, 255, .1))),
              ),
              child: Row(
                children: [
                  const AppIcon(AppIcons.shield, size: 16, color: Color(0xFF8FA2BD)),
                  const SizedBox(width: 8),
                  Text('Secure academic workspace',
                      style: AppText.s(11, color: const Color(0xFF8FA2BD))),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _JourneyButton extends StatelessWidget {
  const _JourneyButton({required this.index, required this.item, required this.active});

  final int index;
  final JourneyItem item;
  final bool active;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: active ? const Color(0xFF193A67) : Colors.transparent,
      borderRadius: BorderRadius.circular(8),
      child: InkWell(
        borderRadius: BorderRadius.circular(8),
        onTap: () {
          Navigator.of(context).pop(); // close drawer
          AppNav.go(context, item.route);
        },
        child: Container(
          height: 36,
          padding: const EdgeInsets.symmetric(horizontal: 11),
          child: Row(
            children: [
              SizedBox(
                width: 18,
                child: Text('$index',
                    style: AppText.s(10, color: active ? const Color(0xFF8CACF3) : const Color(0xFF617795))),
              ),
              const SizedBox(width: 10),
              Expanded(
                child: Text(
                  item.label,
                  overflow: TextOverflow.ellipsis,
                  style: AppText.s(12,
                      w: active ? FontWeight.w700 : FontWeight.w400,
                      color: active ? Colors.white : const Color(0xFFAEBBD0)),
                ),
              ),
              if (active) const AppIcon(AppIcons.chevron, size: 15, color: Colors.white),
            ],
          ),
        ),
      ),
    );
  }
}

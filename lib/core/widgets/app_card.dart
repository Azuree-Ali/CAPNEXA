import 'package:flutter/material.dart';

import 'package:capnexa/core/theme/app_colors.dart';

/// `.card` – white surface, 1px line border, soft shadow.
class AppCard extends StatelessWidget {
  const AppCard({
    super.key,
    required this.child,
    this.padding = const EdgeInsets.all(14),
    this.radius = 14,
    this.color = Colors.white,
    this.borderColor = AppColors.line,
    this.shadow = true,
    this.onTap,
    this.clip = false,
  });

  final Widget child;
  final EdgeInsetsGeometry padding;
  final double radius;
  final Color color;
  final Color borderColor;
  final bool shadow;
  final VoidCallback? onTap;
  final bool clip;

  @override
  Widget build(BuildContext context) {
    final br = BorderRadius.circular(radius);
    final box = Container(
      padding: padding,
      clipBehavior: clip ? Clip.antiAlias : Clip.none,
      decoration: BoxDecoration(
        color: color,
        borderRadius: br,
        border: Border.all(color: borderColor),
        boxShadow: shadow ? AppShadows.card : null,
      ),
      child: child,
    );
    if (onTap == null) return box;
    return Material(
      type: MaterialType.transparency,
      child: InkWell(borderRadius: br, onTap: onTap, child: box),
    );
  }
}

/// Bordered list container whose rows are separated by 1px lines
/// (`.compact-list`, `.member-list`, `.match-list`, `.settings-list`…).
class DividedList extends StatelessWidget {
  const DividedList({super.key, required this.children, this.radius = 14});

  final List<Widget> children;
  final double radius;

  @override
  Widget build(BuildContext context) {
    final rows = <Widget>[];
    for (var i = 0; i < children.length; i++) {
      rows.add(children[i]);
      if (i < children.length - 1) {
        rows.add(const Divider(height: 1, thickness: 1, color: AppColors.line));
      }
    }
    return Material(
      color: Colors.white,
      clipBehavior: Clip.antiAlias,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(radius),
        side: const BorderSide(color: AppColors.line),
      ),
      child: Column(mainAxisSize: MainAxisSize.min, children: rows),
    );
  }
}

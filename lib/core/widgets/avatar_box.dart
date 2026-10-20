import 'package:flutter/material.dart';

import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';

/// `.avatar` – rounded-square initials tile.
class AvatarBox extends StatelessWidget {
  const AvatarBox(
    this.initials, {
    super.key,
    this.size = 35,
    this.radius = 11,
    this.muted = false,
    this.fontSize = 10,
    this.onTap,
  });

  final String initials;
  final double size;
  final double radius;
  final bool muted;
  final double fontSize;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final tile = Container(
      width: size,
      height: size,
      alignment: Alignment.center,
      decoration: BoxDecoration(
        color: muted ? const Color(0xFFEEF1F6) : AppColors.navy900,
        borderRadius: BorderRadius.circular(radius),
      ),
      child: Text(
        initials,
        style: AppText.s(fontSize,
            w: FontWeight.w800, color: muted ? AppColors.navy800 : Colors.white),
      ),
    );
    if (onTap == null) return tile;
    return GestureDetector(onTap: onTap, child: tile);
  }
}

/// `.supervisor-photo` – gradient initials tile used for the supervisor.
class SupervisorPhoto extends StatelessWidget {
  const SupervisorPhoto(this.initials, {super.key, this.size = 74, this.radius = 22, this.fontSize = 21, this.bordered = true});

  final String initials;
  final double size;
  final double radius;
  final double fontSize;
  final bool bordered;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: size,
      height: size,
      alignment: Alignment.center,
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFFD5DDEB), Color(0xFFA7B7CE)],
        ),
        borderRadius: BorderRadius.circular(radius),
        border: bordered ? Border.all(color: Colors.white, width: 3) : null,
        boxShadow: const [
          BoxShadow(color: Color.fromRGBO(20, 38, 68, .14), blurRadius: 20, offset: Offset(0, 7)),
        ],
      ),
      child: Text(initials, style: AppText.s(fontSize, w: FontWeight.w800, color: AppColors.navy800)),
    );
  }
}

/// `.list-icon` – tinted square icon holder.
enum IconTone { blue, teal, orange, red }

class ListIcon extends StatelessWidget {
  const ListIcon({super.key, required this.child, this.tone = IconTone.blue, this.size = 32, this.radius = 9});

  final Widget Function(Color color) child;
  final IconTone tone;
  final double size;
  final double radius;

  static (Color, Color) colors(IconTone t) => switch (t) {
        IconTone.blue => (AppColors.blue700, AppColors.blue100),
        IconTone.teal => (AppColors.teal700, AppColors.teal100),
        IconTone.orange => (AppColors.orange700, AppColors.orange100),
        IconTone.red => (AppColors.red700, AppColors.red100),
      };

  @override
  Widget build(BuildContext context) {
    final (fg, bg) = colors(tone);
    return Container(
      width: size,
      height: size,
      alignment: Alignment.center,
      decoration: BoxDecoration(color: bg, borderRadius: BorderRadius.circular(radius)),
      child: child(fg),
    );
  }
}

/// `.date-block` – calendar tile (day + month).
class DateBlock extends StatelessWidget {
  const DateBlock({super.key, required this.day, required this.month});

  final String day;
  final String month;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 50,
      height: 57,
      decoration: BoxDecoration(
        color: Colors.white,
        border: Border.all(color: const Color(0xFFE2E6ED)),
        borderRadius: BorderRadius.circular(11),
      ),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Text(day, style: AppText.s(18, w: FontWeight.w700, color: AppColors.navy900, height: 1)),
          const SizedBox(height: 4),
          Text(month, style: AppText.s(8, w: FontWeight.w800, color: AppColors.orange700)),
        ],
      ),
    );
  }
}

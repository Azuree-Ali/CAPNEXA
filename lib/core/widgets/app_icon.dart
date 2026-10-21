import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';

import 'package:capnexa/core/theme/app_colors.dart';

/// Icon set copied path-for-path from the Figma export (24x24, stroke 1.8).
enum AppIcons {
  home,
  project,
  team,
  bell,
  user,
  arrow,
  check,
  clock,
  calendar,
  file,
  upload,
  spark,
  shield,
  eye,
  copy,
  plus,
  chevron,
  message,
  logout,
  settings,
  lock,
  mail,
  school,
  chart,
  edit,
  warning,
  menu,
  search,
}

const Map<AppIcons, String> _paths = {
  AppIcons.home: '<path d="m3 11 9-8 9 8"/><path d="M5 10v10h14V10M9 20v-6h6v6"/>',
  AppIcons.project:
      '<rect x="4" y="3" width="16" height="18" rx="2"/><path d="M8 8h8M8 12h8M8 16h5"/>',
  AppIcons.team:
      '<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75"/>',
  AppIcons.bell: '<path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9"/><path d="M10 21h4"/>',
  AppIcons.user: '<circle cx="12" cy="8" r="4"/><path d="M4 21a8 8 0 0 1 16 0"/>',
  AppIcons.arrow: '<path d="M5 12h14M13 6l6 6-6 6"/>',
  AppIcons.check: '<path d="m5 12 4 4L19 6"/>',
  AppIcons.clock: '<circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/>',
  AppIcons.calendar:
      '<rect x="3" y="5" width="18" height="16" rx="2"/><path d="M16 3v4M8 3v4M3 10h18"/>',
  AppIcons.file: '<path d="M6 2h9l4 4v16H6z"/><path d="M14 2v5h5M9 13h6M9 17h6"/>',
  AppIcons.upload: '<path d="M12 16V4M7 9l5-5 5 5"/><path d="M4 17v3h16v-3"/>',
  AppIcons.spark:
      '<path d="m12 3 1.3 4.2L17 9l-3.7 1.8L12 15l-1.3-4.2L7 9l3.7-1.8z"/><path d="m19 15 .7 2.3L22 18.5l-2.3 1.2L19 22l-.7-2.3-2.3-1.2 2.3-1.2z"/>',
  AppIcons.shield: '<path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10"/><path d="m9 12 2 2 4-4"/>',
  AppIcons.eye: '<path d="M2 12s4-7 10-7 10 7 10 7-4 7-10 7S2 12 2 12"/><circle cx="12" cy="12" r="3"/>',
  AppIcons.copy:
      '<rect x="8" y="8" width="12" height="12" rx="2"/><path d="M16 8V5a1 1 0 0 0-1-1H5a1 1 0 0 0-1 1v10a1 1 0 0 0 1 1h3"/>',
  AppIcons.plus: '<path d="M12 5v14M5 12h14"/>',
  AppIcons.chevron: '<path d="m9 18 6-6-6-6"/>',
  AppIcons.message:
      '<path d="M21 15a4 4 0 0 1-4 4H8l-5 3V7a4 4 0 0 1 4-4h10a4 4 0 0 1 4 4z"/><path d="M8 9h8M8 13h5"/>',
  AppIcons.logout:
      '<path d="M10 17l5-5-5-5M15 12H3"/><path d="M14 4h5a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-5"/>',
  AppIcons.settings:
      '<circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.7 1.7 0 0 0 .3 1.9l.1.1-2.8 2.8-.1-.1a1.7 1.7 0 0 0-1.9-.3 1.7 1.7 0 0 0-1 1.6v.2h-4V21a1.7 1.7 0 0 0-1-1.6 1.7 1.7 0 0 0-1.9.3l-.1.1L4.2 17l.1-.1a1.7 1.7 0 0 0 .3-1.9A1.7 1.7 0 0 0 3 14H3v-4h.1a1.7 1.7 0 0 0 1.5-1 1.7 1.7 0 0 0-.3-1.9L4.2 7 7 4.2l.1.1A1.7 1.7 0 0 0 9 4.6a1.7 1.7 0 0 0 1-1.5V3h4v.1a1.7 1.7 0 0 0 1 1.5 1.7 1.7 0 0 0 1.9-.3l.1-.1L19.8 7l-.1.1a1.7 1.7 0 0 0-.3 1.9 1.7 1.7 0 0 0 1.6 1h.2v4H21a1.7 1.7 0 0 0-1.6 1Z"/>',
  AppIcons.lock: '<rect x="4" y="10" width="16" height="11" rx="2"/><path d="M8 10V7a4 4 0 0 1 8 0v3"/>',
  AppIcons.mail: '<rect x="3" y="5" width="18" height="14" rx="2"/><path d="m3 7 9 6 9-6"/>',
  AppIcons.school:
      '<path d="m2 9 10-5 10 5-10 5z"/><path d="M6 11.5V17c3 2 9 2 12 0v-5.5M22 9v7"/>',
  AppIcons.chart: '<path d="M4 20V10M10 20V4M16 20v-7M22 20H2"/>',
  AppIcons.edit: '<path d="m4 20 4.5-1 10-10-3.5-3.5-10 10z"/><path d="m13.5 6.5 3.5 3.5"/>',
  AppIcons.warning: '<path d="M12 3 2 21h20z"/><path d="M12 9v5M12 18h.01"/>',
  AppIcons.menu: '<path d="M4 7h16M4 12h16M4 17h16"/>',
  AppIcons.search: '<circle cx="11" cy="11" r="7"/><path d="m20 20-4-4"/>',
};

class AppIcon extends StatelessWidget {
  const AppIcon(this.icon, {super.key, this.size = 20, this.color = AppColors.ink});

  final AppIcons icon;
  final double size;
  final Color color;

  @override
  Widget build(BuildContext context) {
    final svg =
        '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="#000000" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">${_paths[icon]}</svg>';
    return SvgPicture.string(
      svg,
      width: size,
      height: size,
      colorFilter: ColorFilter.mode(color, BlendMode.srcIn),
    );
  }
}

enum Provider { google, github }

/// Brand marks used by the social sign-in buttons (copied from the export).
class ProviderIcon extends StatelessWidget {
  const ProviderIcon(this.provider, {super.key});

  final Provider provider;

  static const _google =
      '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path fill="#4285F4" d="M21.6 12.23c0-.71-.06-1.4-.18-2.07H12v3.91h5.38a4.6 4.6 0 0 1-2 3.02v2.54h3.24c1.9-1.75 2.98-4.33 2.98-7.4Z"/><path fill="#34A853" d="M12 22c2.7 0 4.97-.9 6.62-2.37l-3.24-2.54c-.9.6-2.05.96-3.38.96-2.61 0-4.82-1.76-5.61-4.13H3.04v2.62A10 10 0 0 0 12 22Z"/><path fill="#FBBC05" d="M6.39 13.92A6.02 6.02 0 0 1 6.07 12c0-.67.12-1.32.32-1.92V7.46H3.04A10 10 0 0 0 2 12c0 1.61.39 3.14 1.04 4.54l3.35-2.62Z"/><path fill="#EA4335" d="M12 5.95c1.47 0 2.79.5 3.83 1.5l2.87-2.87A9.63 9.63 0 0 0 12 2a10 10 0 0 0-8.96 5.46l3.35 2.62C7.18 7.71 9.39 5.95 12 5.95Z"/></svg>';
  static const _microsoft =
      '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path fill="#F35325" d="M2 2h9.5v9.5H2z"/><path fill="#81BC06" d="M12.5 2H22v9.5h-9.5z"/><path fill="#05A6F0" d="M2 12.5h9.5V22H2z"/><path fill="#FFBA08" d="M12.5 12.5H22V22h-9.5z"/></svg>';
  static const _github =
      '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path fill="#17233A" d="M12 2a10 10 0 0 0-3.16 19.49c.5.09.68-.22.68-.48v-1.88c-2.78.6-3.37-1.18-3.37-1.18-.45-1.16-1.11-1.47-1.11-1.47-.91-.62.07-.61.07-.61 1 .07 1.53 1.03 1.53 1.03.9 1.53 2.35 1.09 2.92.83.09-.65.35-1.09.64-1.34-2.22-.25-4.56-1.11-4.56-4.94 0-1.09.39-1.98 1.03-2.68-.1-.25-.45-1.27.1-2.64 0 0 .84-.27 2.75 1.02A9.58 9.58 0 0 1 12 6.82a9.5 9.5 0 0 1 2.5.34c1.91-1.29 2.75-1.02 2.75-1.02.55 1.37.2 2.39.1 2.64.64.7 1.03 1.59 1.03 2.68 0 3.84-2.34 4.68-4.57 4.93.36.31.68.92.68 1.86V21c0 .27.18.58.69.48A10 10 0 0 0 12 2Z"/></svg>';

  @override
  Widget build(BuildContext context) {
    final data = switch (provider) {
      Provider.google => _google,
      Provider.github => _github,
    };
    final size = provider == Provider.github ? 18.0 : 17.0;
    return SvgPicture.string(data, width: size, height: size);
  }
}

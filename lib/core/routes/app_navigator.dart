import 'package:flutter/material.dart';

import 'package:capnexa/core/routes/app_routes.dart';

/// Thin wrapper over [Navigator] so navigation rules live in one place.
class AppNav {
  AppNav._();

  /// Forward navigation inside a flow (Android back returns to the previous screen).
  static Future<T?> push<T>(BuildContext context, String route) =>
      Navigator.of(context).pushNamed<T>(route);

  /// Jump to a top-level destination (bottom bar, journey drawer, auth).
  /// The stack is reset to Home so back always leads Home, then exits.
  static void go(BuildContext context, String route) {
    final nav = Navigator.of(context);
    if (Routes.standalone.contains(route) || route == Routes.home) {
      nav.pushNamedAndRemoveUntil(route, (_) => false);
    } else {
      nav.pushNamedAndRemoveUntil(route, (r) => r.settings.name == Routes.home);
    }
  }

  /// Replace the current screen (splash → onboarding → login …).
  static void replace(BuildContext context, String route) =>
      Navigator.of(context).pushReplacementNamed(route);

  static final navigatorKey = GlobalKey<NavigatorState>();

  /// يوجّه للـ login من أي مكان (مثلًا لما الـ refresh token ينتهي)
  static void toLogin() => navigatorKey.currentState
      ?.pushNamedAndRemoveUntil(Routes.login, (_) => false);
}

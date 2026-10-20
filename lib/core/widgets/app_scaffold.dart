import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/utils/layout.dart';
import 'package:capnexa/core/widgets/app_bottom_nav.dart';
import 'package:capnexa/core/widgets/app_top_bar.dart';
import 'package:capnexa/core/widgets/journey_drawer.dart';

export 'package:capnexa/core/widgets/app_bottom_nav.dart' show AppTab;

/// Standard screen frame: sticky app bar, scrollable body, optional sticky
/// bottom actions, bottom navigation and the journey drawer.
class AppScaffold extends StatelessWidget {
  const AppScaffold({
    super.key,
    required this.title,
    required this.body,
    this.subtitle,
    this.action,
    this.bottom,
    this.tab,
    this.backgroundColor = AppColors.scaffold,
    this.showBottomNav = true,
  });

  final String title;
  final String? subtitle;
  final Widget? action;
  final Widget body;
  final Widget? bottom;
  final AppTab? tab;
  final Color backgroundColor;
  final bool showBottomNav;

  @override
  Widget build(BuildContext context) {
    final route = ModalRoute.of(context)?.settings.name;
    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: SystemUiOverlayStyle.dark.copyWith(
        statusBarColor: Colors.transparent,
        systemNavigationBarColor: Colors.white,
        systemNavigationBarIconBrightness: Brightness.dark,
      ),
      child: Scaffold(
        backgroundColor: backgroundColor,
        drawer: JourneyDrawer(currentRoute: route),
        body: Column(
          children: [
            AppTopBar(title: title, subtitle: subtitle, action: action),
            Expanded(child: body),
            if (bottom != null) bottom!,
            if (showBottomNav && !Routes.standalone.contains(route)) AppBottomNav(active: tab),
          ],
        ),
      ),
    );
  }
}

/// `.screen-content` – scrollable column with 17px side padding and 20px gap.
class ScreenBody extends StatelessWidget {
  const ScreenBody({
    super.key,
    required this.children,
    this.padding = const EdgeInsets.fromLTRB(17, 16, 17, 26),
    this.gap = 20,
  });

  final List<Widget> children;
  final EdgeInsets padding;
  final double gap;

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      physics: const ClampingScrollPhysics(),
      padding: padding,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: spaced(children, gap),
      ),
    );
  }
}

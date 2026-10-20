import 'package:flutter/material.dart';

import 'package:capnexa/core/constants/app_constants.dart';
import 'package:capnexa/core/routes/app_router.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_theme.dart';

class CapNexaApp extends StatelessWidget {
  const CapNexaApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: AppConstants.brandName,
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light,
      initialRoute: Routes.splash,
      onGenerateRoute: AppRouter.onGenerateRoute,
      builder: (context, child) {
        final mq = MediaQuery.of(context);
        // Responsive frame: clamp OS font scaling so the fixed-size design
        // keeps its layout, and centre/cap the content on tablets & foldables.
        return MediaQuery(
          data: mq.copyWith(
            textScaler: mq.textScaler.clamp(minScaleFactor: 0.9, maxScaleFactor: 1.2),
          ),
          child: ColoredBox(
            color: AppColors.canvas,
            child: Center(
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: AppLayout.maxContentWidth),
                child: child,
              ),
            ),
          ),
        );
      },
    );
  }
}

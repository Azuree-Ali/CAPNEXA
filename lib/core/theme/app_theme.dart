import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import 'package:capnexa/core/theme/app_colors.dart';

class AppTheme {
  AppTheme._();

  static ThemeData get light {
    final base = ThemeData(
      useMaterial3: true,
      colorScheme: ColorScheme.fromSeed(
        seedColor: AppColors.navy900,
        primary: AppColors.navy900,
        secondary: AppColors.blue700,
        surface: AppColors.surface,
        error: AppColors.red700,
      ),
      scaffoldBackgroundColor: AppColors.scaffold,
      dividerColor: AppColors.line,
    );
    return base.copyWith(
      textTheme: GoogleFonts.manropeTextTheme(base.textTheme).apply(
        bodyColor: AppColors.ink,
        displayColor: AppColors.ink,
      ),
    );
  }
}

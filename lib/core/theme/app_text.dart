import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import 'package:capnexa/core/theme/app_colors.dart';

/// Typography helpers. The Figma export uses Manrope with fixed pixel sizes.
class AppText {
  AppText._();

  static TextStyle s(
    double size, {
    FontWeight w = FontWeight.w400,
    Color color = AppColors.ink,
    double? height,
    double? ls,
    FontStyle? style,
  }) {
    return GoogleFonts.manrope(
      fontSize: size,
      fontWeight: w,
      color: color,
      height: height,
      letterSpacing: ls,
      fontStyle: style,
    );
  }

  /// `.screen-content h2`
  static TextStyle get h2 => s(15, w: FontWeight.w700, color: AppColors.navy900, ls: -.3);

  /// `.screen-content h3`
  static TextStyle get h3 => s(13, w: FontWeight.w700, color: AppColors.navy900);

  /// `.screen-content p`
  static TextStyle get p => s(11, color: AppColors.muted, height: 1.6);

  /// `.overline`
  static TextStyle get overline =>
      s(10, w: FontWeight.w800, color: AppColors.overline, ls: 1.35);

  /// `.eyebrow`
  static TextStyle get eyebrow =>
      s(10, w: FontWeight.w800, color: AppColors.blue700, ls: 1.35);
}

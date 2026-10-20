import 'package:flutter/material.dart';

/// Colour tokens copied 1:1 from the Figma export (:root CSS variables).
class AppColors {
  AppColors._();

  static const navy950 = Color(0xFF07152D);
  static const navy900 = Color(0xFF0B1F3A);
  static const navy800 = Color(0xFF12355B);
  static const blue700 = Color(0xFF2754C5);
  static const blue600 = Color(0xFF3869DC);
  static const blue100 = Color(0xFFE9EFFF);
  static const blue50 = Color(0xFFF4F7FF);
  static const teal700 = Color(0xFF08766D);
  static const teal100 = Color(0xFFDFF5F0);
  static const orange700 = Color(0xFFA64B00);
  static const orange100 = Color(0xFFFFF0DC);
  static const red700 = Color(0xFFB4232C);
  static const red100 = Color(0xFFFDE8E9);
  static const ink = Color(0xFF17233A);
  static const muted = Color(0xFF69748A);
  static const line = Color(0xFFE3E8F0);
  static const canvas = Color(0xFFF5F7FB);
  static const surface = Color(0xFFFFFFFF);
  static const scaffold = Color(0xFFFBFCFE);

  // Frequently repeated one-off values from the design.
  static const overline = Color(0xFF8DA2C3);
  static const heroTrack = Color(0xFF30445F);
  static const heroFill = Color(0xFF6693F4);
  static const heroSub = Color(0xFFA9B9D0);
  static const fieldBorder = Color(0xFFD8DEE8);
  static const hint = Color(0xFF9AA5B5);
  static const iconMuted = Color(0xFF8390A4);
  static const softFill = Color(0xFFF3F5F8);
  static const chevron = Color(0xFF9AA4B4);
}

class AppShadows {
  AppShadows._();

  static const card = [
    BoxShadow(color: Color.fromRGBO(20, 38, 68, .04), blurRadius: 15, offset: Offset(0, 5)),
  ];
  static const hero = [
    BoxShadow(color: Color.fromRGBO(11, 31, 58, .17), blurRadius: 25, offset: Offset(0, 12)),
  ];
  static const button = [
    BoxShadow(color: Color.fromRGBO(11, 31, 58, .16), blurRadius: 15, offset: Offset(0, 7)),
  ];
}

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/brand_logo.dart';
import 'package:capnexa/core/widgets/full_height_scroll.dart';

/// `.auth-screen` – navy header with logo, rounded light panel below.
class AuthShell extends StatelessWidget {
  const AuthShell({super.key, required this.child, this.step});

  final Widget child;
  final String? step;

  @override
  Widget build(BuildContext context) {
    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: SystemUiOverlayStyle.light.copyWith(
        statusBarColor: Colors.transparent,
        systemNavigationBarColor: AppColors.scaffold,
        systemNavigationBarIconBrightness: Brightness.dark,
      ),
      child: Scaffold(
        backgroundColor: AppColors.navy900,
        resizeToAvoidBottomInset: true,
        body: SafeArea(
          bottom: false,
          child: Padding(
            padding: const EdgeInsets.only(top: 10),
            child: FullHeightScroll(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  SizedBox(
                    height: 54,
                    child: Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 20),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          const BrandLogo(inverse: true),
                          if (step != null)
                            Text(step!, style: AppText.s(9, color: const Color(0xFF9FB0C8))),
                        ],
                      ),
                    ),
                  ),
                  Expanded(
                    child: Container(
                      padding: EdgeInsets.fromLTRB(
                          23, 28, 23, 22 + MediaQuery.of(context).padding.bottom),
                      decoration: const BoxDecoration(
                        color: AppColors.scaffold,
                        borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
                      ),
                      child: child,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

/// `.auth-heading` – eyebrow, h1 and subtitle.
class AuthHeading extends StatelessWidget {
  const AuthHeading({super.key, required this.eyebrow, required this.title, required this.subtitle});

  final String eyebrow;
  final String title;
  final String subtitle;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 22),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(eyebrow, style: AppText.eyebrow),
          Padding(
            padding: const EdgeInsets.fromLTRB(0, 7, 0, 4),
            child: Text(title,
                style: AppText.s(24, w: FontWeight.w700, color: AppColors.navy900, ls: -.8)),
          ),
          Text(subtitle, style: AppText.s(11, color: AppColors.muted)),
        ],
      ),
    );
  }
}

/// `.auth-switch` – "New here? Create account".
class AuthSwitch extends StatelessWidget {
  const AuthSwitch({super.key, required this.prompt, required this.action, required this.onTap});

  final String prompt;
  final String action;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(top: 19),
      child: Wrap(
        alignment: WrapAlignment.center,
        crossAxisAlignment: WrapCrossAlignment.center,
        children: [
          Text('$prompt ', style: AppText.s(10, color: AppColors.muted)),
          GestureDetector(
            onTap: onTap,
            child: Text(action,
                style: AppText.s(10, w: FontWeight.w800, color: AppColors.blue700)),
          ),
        ],
      ),
    );
  }
}

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/brand_logo.dart';
import 'package:capnexa/core/widgets/full_height_scroll.dart';

class SplashScreen extends StatelessWidget {
  const SplashScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: SystemUiOverlayStyle.light.copyWith(
        statusBarColor: Colors.transparent,
        systemNavigationBarColor: AppColors.navy950,
        systemNavigationBarIconBrightness: Brightness.light,
      ),
      child: Scaffold(
        backgroundColor: AppColors.navy950,
        body: Stack(
          children: [
            // `.splash-grid` – 32px grid fading out towards the bottom.
            Positioned.fill(
              child: ShaderMask(
                blendMode: BlendMode.dstIn,
                shaderCallback: (rect) => const LinearGradient(
                  begin: Alignment.topCenter,
                  end: Alignment.bottomCenter,
                  colors: [Colors.black, Colors.transparent],
                  stops: [0, .85],
                ).createShader(rect),
                child: CustomPaint(painter: _GridPainter()),
              ),
            ),
            SafeArea(
              child: FullHeightScroll(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Padding(
                      padding: const EdgeInsets.fromLTRB(27, 38, 27, 0),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const BrandLogo(inverse: true),
                          Container(
                            width: 74,
                            height: 74,
                            margin: const EdgeInsets.only(top: 110, bottom: 24),
                            alignment: Alignment.center,
                            decoration: BoxDecoration(
                              color: const Color(0xFF163966),
                              borderRadius: BorderRadius.circular(20),
                              boxShadow: const [
                                BoxShadow(
                                    color: Color.fromRGBO(0, 0, 0, .2),
                                    blurRadius: 40,
                                    offset: Offset(0, 18)),
                              ],
                            ),
                            child: const AppIcon(AppIcons.school, size: 44, color: Color(0xFF91B4FF)),
                          ),
                          Text(
                            'Build your best work.\nStay on track.',
                            style: AppText.s(30,
                                w: FontWeight.w700, color: Colors.white, height: 1.18, ls: -1.3),
                          ),
                          const SizedBox(height: 12),
                          Text(
                            'Manage. Build. Submit. Succeed.',
                            style: AppText.s(12, color: const Color(0xFF9AACC7), ls: .25),
                          ),
                        ],
                      ),
                    ),
                    const Spacer(),
                    Padding(
                      padding: const EdgeInsets.fromLTRB(27, 24, 27, 31),
                      child: Column(
                        children: [
                          Row(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              const AppIcon(AppIcons.shield, size: 17, color: Color(0xFF8EA2BF)),
                              const SizedBox(width: 7),
                              Text('Secure university workspace',
                                  style: AppText.s(9, color: const Color(0xFF8EA2BF))),
                            ],
                          ),
                          const SizedBox(height: 14),
                          AppButton(
                            label: 'Get started',
                            icon: AppIcons.arrow,
                            variant: ButtonVariant.secondary,
                            full: true,
                            onPressed: () => AppNav.replace(context, Routes.onboarding),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _GridPainter extends CustomPainter {
  const _GridPainter();

  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = const Color.fromRGBO(255, 255, 255, .025)
      ..strokeWidth = 1;
    for (double x = 0; x <= size.width; x += 32) {
      canvas.drawLine(Offset(x, 0), Offset(x, size.height), paint);
    }
    for (double y = 0; y <= size.height; y += 32) {
      canvas.drawLine(Offset(0, y), Offset(size.width, y), paint);
    }
  }

  @override
  bool shouldRepaint(_GridPainter old) => false;
}

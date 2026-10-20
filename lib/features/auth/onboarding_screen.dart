import 'dart:math' as math;

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/brand_logo.dart';
import 'package:capnexa/core/widgets/full_height_scroll.dart';

class _Slide {
  const _Slide(this.number, this.icon, this.title, this.text);

  final String number;
  final AppIcons icon;
  final String title;
  final String text;
}

const _slides = [
  _Slide('01', AppIcons.project, 'One place for your graduation project',
      'Bring your team, proposal, milestones, and submissions into one organized workspace.'),
  _Slide('02', AppIcons.chart, 'Know exactly what comes next',
      'Track progress, deadlines, and supervisor feedback at every stage of your project.'),
  _Slide('03', AppIcons.spark, 'Submit with more confidence',
      'Review academic similarity insights and respond to feedback before final evaluation.'),
];

class OnboardingScreen extends StatefulWidget {
  const OnboardingScreen({super.key});

  @override
  State<OnboardingScreen> createState() => _OnboardingScreenState();
}

class _OnboardingScreenState extends State<OnboardingScreen> {
  int _index = 0;

  void _finish() => AppNav.replace(context, Routes.login);

  void _next() {
    if (_index < _slides.length - 1) {
      setState(() => _index++);
    } else {
      _finish();
    }
  }

  @override
  Widget build(BuildContext context) {
    final slide = _slides[_index];
    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: SystemUiOverlayStyle.dark.copyWith(
        statusBarColor: Colors.transparent,
        systemNavigationBarColor: AppColors.scaffold,
        systemNavigationBarIconBrightness: Brightness.dark,
      ),
      child: Scaffold(
        backgroundColor: AppColors.scaffold,
        body: SafeArea(
          child: FullHeightScroll(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Padding(
                  padding: const EdgeInsets.fromLTRB(22, 24, 22, 0),
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const BrandLogo(compact: true),
                      GestureDetector(
                        onTap: _finish,
                        behavior: HitTestBehavior.opaque,
                        child: Padding(
                          padding: const EdgeInsets.all(6),
                          child: Text('Skip',
                              style: AppText.s(9.5, w: FontWeight.w800, color: AppColors.blue700)),
                        ),
                      ),
                    ],
                  ),
                ),
                Container(
                  height: 350,
                  margin: const EdgeInsets.fromLTRB(22, 34, 22, 0),
                  decoration: BoxDecoration(
                    color: AppColors.blue50,
                    borderRadius: BorderRadius.circular(25),
                  ),
                  clipBehavior: Clip.antiAlias,
                  child: Stack(
                    alignment: Alignment.center,
                    children: [
                      Positioned(
                        right: -10,
                        top: -35,
                        child: Text(
                          slide.number,
                          style: AppText.s(160,
                              w: FontWeight.w800, color: const Color(0xFFE5ECFC), ls: -15, height: 1.2),
                        ),
                      ),
                      Transform.rotate(
                        angle: 8 * math.pi / 180,
                        child: Transform.translate(
                          offset: const Offset(15, 5),
                          child: Container(
                            width: 205,
                            height: 235,
                            decoration: BoxDecoration(
                              color: const Color(0xFFB7C8ED),
                              borderRadius: BorderRadius.circular(20),
                            ),
                          ),
                        ),
                      ),
                      Container(
                        width: 205,
                        height: 235,
                        padding: const EdgeInsets.all(27),
                        decoration: BoxDecoration(
                          color: Colors.white,
                          borderRadius: BorderRadius.circular(20),
                          boxShadow: const [
                            BoxShadow(
                                color: Color.fromRGBO(28, 62, 122, .15),
                                blurRadius: 45,
                                offset: Offset(0, 18)),
                          ],
                        ),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.stretch,
                          children: [
                            Align(
                              alignment: Alignment.centerLeft,
                              child: Container(
                                width: 64,
                                height: 64,
                                alignment: Alignment.center,
                                decoration: BoxDecoration(
                                  color: AppColors.blue100,
                                  borderRadius: BorderRadius.circular(18),
                                ),
                                child: AppIcon(slide.icon, size: 36, color: AppColors.blue700),
                              ),
                            ),
                            const SizedBox(height: 35),
                            _line(1),
                            _line(.62),
                            const SizedBox(height: 19),
                            ClipRRect(
                              borderRadius: BorderRadius.circular(8),
                              child: Container(
                                height: 8,
                                color: const Color(0xFFEDF0F5),
                                alignment: Alignment.centerLeft,
                                child: FractionallySizedBox(
                                  widthFactor: .62,
                                  child: Container(color: AppColors.blue600),
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
                Padding(
                  padding: const EdgeInsets.fromLTRB(24, 28, 24, 20),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('SMART PROJECT MANAGEMENT', style: AppText.eyebrow),
                      Padding(
                        padding: const EdgeInsets.fromLTRB(0, 8, 0, 10),
                        child: Text(
                          slide.title,
                          style: AppText.s(26,
                              w: FontWeight.w700, color: AppColors.navy900, height: 1.2, ls: -.9),
                        ),
                      ),
                      Text(slide.text, style: AppText.s(11, color: AppColors.muted, height: 1.7)),
                      const SizedBox(height: 28),
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Row(
                            children: [
                              for (var i = 0; i < _slides.length; i++)
                                AnimatedContainer(
                                  duration: const Duration(milliseconds: 200),
                                  width: i == _index ? 20 : 6,
                                  height: 6,
                                  margin: const EdgeInsets.only(right: 5),
                                  decoration: BoxDecoration(
                                    color: i == _index ? AppColors.blue600 : const Color(0xFFD1D8E3),
                                    borderRadius: BorderRadius.circular(6),
                                  ),
                                ),
                            ],
                          ),
                          Material(
                            color: AppColors.navy900,
                            shape: const CircleBorder(),
                            child: InkWell(
                              customBorder: const CircleBorder(),
                              onTap: _next,
                              child: const SizedBox(
                                width: 48,
                                height: 48,
                                child: Center(
                                    child: AppIcon(AppIcons.arrow, color: Colors.white)),
                              ),
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _line(double factor) => Padding(
        padding: const EdgeInsets.only(bottom: 10),
        child: FractionallySizedBox(
          widthFactor: factor,
          alignment: Alignment.centerLeft,
          child: Container(
            height: 8,
            decoration: BoxDecoration(
              color: const Color(0xFFD9E1EE),
              borderRadius: BorderRadius.circular(4),
            ),
          ),
        ),
      );
}

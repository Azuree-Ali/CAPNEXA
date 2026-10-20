import 'package:flutter/material.dart';

import 'package:capnexa/core/constants/app_constants.dart';
import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_card.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/app_top_bar.dart';
import 'package:capnexa/core/widgets/async_scaffold.dart';
import 'package:capnexa/core/widgets/section_heading.dart';
import 'package:capnexa/core/widgets/status_badge.dart';
import 'package:capnexa/features/profile/widgets/settings_list.dart';
import 'package:capnexa/models/student.dart';
import 'package:capnexa/services/service_locator.dart';

class ProfileScreen extends StatelessWidget {
  const ProfileScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AsyncScaffold<Student>(
      title: 'Profile',
      tab: AppTab.profile,
      loader: ServiceLocator.auth.currentStudent,
      actionBuilder: (context, _) => IconSquareButton(
        icon: AppIcons.settings,
        onTap: () => AppNav.push(context, Routes.settings),
      ),
      builder: (context, s) => ScreenBody(
        children: [
          Column(
            children: [
              Container(
                width: 77,
                height: 77,
                margin: const EdgeInsets.fromLTRB(0, 3, 0, 10),
                child: Stack(
                  clipBehavior: Clip.none,
                  children: [
                    Container(
                      alignment: Alignment.center,
                      decoration: BoxDecoration(
                        color: AppColors.navy900,
                        borderRadius: BorderRadius.circular(23),
                      ),
                      child: Text(s.initials,
                          style: AppText.s(22, w: FontWeight.w800, color: Colors.white)),
                    ),
                    Positioned(
                      right: -3,
                      bottom: -3,
                      child: Container(
                        width: 23,
                        height: 23,
                        alignment: Alignment.center,
                        decoration: BoxDecoration(
                          color: AppColors.teal700,
                          shape: BoxShape.circle,
                          border: Border.all(color: Colors.white, width: 3),
                        ),
                        child: const AppIcon(AppIcons.check, size: 13, color: Colors.white),
                      ),
                    ),
                  ],
                ),
              ),
              Text(s.fullName,
                  style: AppText.s(19, w: FontWeight.w700, color: AppColors.navy900, ls: -.3)),
              Padding(
                padding: const EdgeInsets.fromLTRB(0, 3, 0, 8),
                child: Text('${s.department} · ${s.academicYear}', style: AppText.p),
              ),
              const StatusBadge('Verified student', tone: BadgeTone.teal),
            ],
          ),
          DividedList(
            radius: 13,
            children: [
              _InfoRow('UNIVERSITY ID', s.universityId),
              _InfoRow('UNIVERSITY EMAIL', s.email),
              _InfoRow('DEPARTMENT', s.department),
            ],
          ),
          Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: const [
              SectionHeading('Account settings'),
              SettingsList(),
            ],
          ),
          AppButton(
            label: 'Sign out',
            icon: AppIcons.logout,
            variant: ButtonVariant.danger,
            full: true,
            onPressed: () async {
              await ServiceLocator.auth.signOut();
              if (context.mounted) AppNav.go(context, Routes.login);
            },
          ),
          Text('${AppConstants.brandName} Student · Version ${AppConstants.appVersion}',
              textAlign: TextAlign.center,
              style: AppText.s(7.5, color: AppColors.muted)),
        ],
      ),
    );
  }
}

class _InfoRow extends StatelessWidget {
  const _InfoRow(this.label, this.value);

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: AppText.s(6.5, color: AppColors.muted, ls: .5)),
          const SizedBox(height: 3),
          Text(value, style: AppText.s(9, w: FontWeight.w700, color: AppColors.navy900)),
        ],
      ),
    );
  }
}

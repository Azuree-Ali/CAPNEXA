import 'package:flutter/material.dart';

import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/section_heading.dart';
import 'package:capnexa/features/profile/widgets/settings_list.dart';

/// Not present in the Figma export as its own frame; composed only from the
/// existing "Account settings" list pattern so it stays visually consistent.
class SettingsScreen extends StatelessWidget {
  const SettingsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: 'Settings',
      subtitle: 'Account and preferences',
      tab: AppTab.profile,
      body: ScreenBody(
        children: [
          Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: const [
              SectionHeading('Account settings'),
              SettingsList(),
            ],
          ),
        ],
      ),
    );
  }
}

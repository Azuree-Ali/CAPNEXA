import 'package:flutter/material.dart';

import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_dialog.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/app_text_field.dart';
import 'package:capnexa/core/widgets/info_note.dart';
import 'package:capnexa/services/service_locator.dart';

class TeamChoiceScreen extends StatefulWidget {
  const TeamChoiceScreen({super.key});

  @override
  State<TeamChoiceScreen> createState() => _TeamChoiceScreenState();
}

class _TeamChoiceScreenState extends State<TeamChoiceScreen> {
  final _code = TextEditingController();
  bool _createSelected = true;

  @override
  void dispose() {
    _code.dispose();
    super.dispose();
  }

  Future<void> _join() async {
    if (_code.text.trim().isEmpty) {
      showAppSnack(context, 'Enter the invitation code from your team leader.');
      return;
    }
    await ServiceLocator.team.joinTeam(_code.text.trim());
    if (!mounted) return;
    AppNav.push(context, Routes.team);
  }

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: 'Set up your team',
      subtitle: 'Choose how you’d like to collaborate',
      tab: AppTab.team,
      body: ScreenBody(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(17, 12, 17, 4),
            child: Column(
              children: [
                Container(
                  width: 58,
                  height: 58,
                  alignment: Alignment.center,
                  decoration: BoxDecoration(
                    color: AppColors.blue100,
                    borderRadius: BorderRadius.circular(17),
                  ),
                  child: const AppIcon(AppIcons.team, size: 32, color: AppColors.blue700),
                ),
                const SizedBox(height: 14),
                Text('Projects are better together',
                    textAlign: TextAlign.center,
                    style: AppText.s(19, w: FontWeight.w700, color: AppColors.navy900, ls: -.3)),
                const SizedBox(height: 6),
                Text(
                  'Create a team and invite classmates, or join an existing team with an invitation code.',
                  textAlign: TextAlign.center,
                  style: AppText.p,
                ),
              ],
            ),
          ),
          _ChoiceCard(
            selected: _createSelected,
            icon: AppIcons.plus,
            title: 'Create a new team',
            text: 'Set a team name, add your idea, and invite members.',
            cta: 'Start a team',
            onTap: () {
              setState(() => _createSelected = true);
              ServiceLocator.team.createTeam('Team 04');
              AppNav.push(context, Routes.team);
            },
          ),
          _ChoiceCard(
            selected: !_createSelected,
            icon: AppIcons.team,
            pale: true,
            title: 'Join an existing team',
            text: 'Ask your team leader for their 8-character code.',
            onTap: () => setState(() => _createSelected = false),
          ),
          Container(
            padding: const EdgeInsets.all(14),
            decoration: BoxDecoration(
              color: const Color(0xFFF4F6FA),
              borderRadius: BorderRadius.circular(14),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                AppTextField(label: 'Invitation code', hint: 'e.g. GF-24X8Q', controller: _code),
                const SizedBox(height: 10),
                AppButton(
                    label: 'Join team', variant: ButtonVariant.outline, full: true, onPressed: _join),
              ],
            ),
          ),
          const InfoNote(
            icon: AppIcons.shield,
            title: 'One team per project',
            message: 'You can only be part of one active graduation team.',
          ),
        ],
      ),
    );
  }
}

class _ChoiceCard extends StatelessWidget {
  const _ChoiceCard({
    required this.selected,
    required this.icon,
    required this.title,
    required this.text,
    required this.onTap,
    this.cta,
    this.pale = false,
  });

  final bool selected;
  final AppIcons icon;
  final String title;
  final String text;
  final String? cta;
  final bool pale;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final radius = BorderRadius.circular(15);
    return Material(
      color: Colors.white,
      shape: RoundedRectangleBorder(
        borderRadius: radius,
        side: BorderSide(color: selected ? const Color(0xFFAEBFEE) : AppColors.line),
      ),
      elevation: 0,
      child: InkWell(
        borderRadius: radius,
        onTap: onTap,
        child: Ink(
          decoration: BoxDecoration(
            borderRadius: radius,
            boxShadow: selected
                ? const [
                    BoxShadow(
                        color: Color.fromRGBO(44, 83, 180, .08), blurRadius: 20, offset: Offset(0, 6)),
                  ]
                : null,
          ),
          padding: const EdgeInsets.all(14),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                width: 40,
                height: 40,
                alignment: Alignment.center,
                decoration: BoxDecoration(
                  color: pale ? const Color(0xFFEEF1F5) : AppColors.blue100,
                  borderRadius: BorderRadius.circular(11),
                ),
                child: AppIcon(icon,
                    size: 25, color: pale ? const Color(0xFF52627A) : AppColors.blue700),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(title, style: AppText.h3),
                    Padding(
                      padding: const EdgeInsets.fromLTRB(0, 4, 0, 8),
                      child: Text(text, style: AppText.s(11, color: AppColors.muted, height: 1.5)),
                    ),
                    if (cta != null)
                      Row(
                        children: [
                          Text(cta!,
                              style: AppText.s(9.5, w: FontWeight.w700, color: AppColors.blue700)),
                          const SizedBox(width: 5),
                          const AppIcon(AppIcons.arrow, size: 16, color: AppColors.blue700),
                        ],
                      ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

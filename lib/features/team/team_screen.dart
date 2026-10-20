import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_card.dart';
import 'package:capnexa/core/widgets/app_dialog.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/app_top_bar.dart';
import 'package:capnexa/core/widgets/async_scaffold.dart';
import 'package:capnexa/core/widgets/avatar_box.dart';
import 'package:capnexa/core/widgets/section_heading.dart';
import 'package:capnexa/core/widgets/status_badge.dart';
import 'package:capnexa/models/team.dart';
import 'package:capnexa/services/service_locator.dart';

class TeamScreen extends StatelessWidget {
  const TeamScreen({super.key});

  Future<void> _invite(BuildContext context, Team team) async {
    await showAppDialog(
      context,
      title: 'Invite a teammate',
      message: 'Share this code with your classmates so they can join ${team.projectTitle}.',
      confirmLabel: 'Copy code',
      cancelLabel: 'Close',
      content: Container(
        padding: const EdgeInsets.symmetric(vertical: 12),
        alignment: Alignment.center,
        decoration: BoxDecoration(
          color: AppColors.blue50,
          borderRadius: BorderRadius.circular(11),
        ),
        child: Text(team.inviteCode,
            style: AppText.s(22, w: FontWeight.w700, color: AppColors.navy900, ls: 1.5)),
      ),
    ).then((copy) async {
      if (copy) {
        await Clipboard.setData(ClipboardData(text: team.inviteCode));
        if (context.mounted) showAppSnack(context, 'Invitation code copied');
      }
    });
  }

  Future<void> _leave(BuildContext context) async {
    final ok = await showAppDialog(
      context,
      title: 'Leave team?',
      message: 'You will lose access to the team workspace and its submissions.',
      confirmLabel: 'Leave team',
      destructive: true,
    );
    if (ok && context.mounted) {
      await ServiceLocator.team.leaveTeam();
      if (context.mounted) AppNav.go(context, Routes.teamChoice);
    }
  }

  @override
  Widget build(BuildContext context) {
    return AsyncScaffold<Team>(
      title: 'Your team',
      tab: AppTab.team,
      loader: ServiceLocator.team.getTeam,
      subtitleBuilder: (t) => t.projectTitle,
      actionBuilder: (context, t) => AppButton(
        label: 'Invite',
        icon: AppIcons.plus,
        variant: ButtonVariant.text,
        onPressed: () => _invite(context, t),
      ),
      builder: (context, team) => ScreenBody(
        children: [
          Container(
            padding: const EdgeInsets.all(17),
            decoration: BoxDecoration(
              color: AppColors.navy900,
              borderRadius: BorderRadius.circular(16),
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('TEAM INVITATION CODE', style: AppText.overline),
                      Padding(
                        padding: const EdgeInsets.symmetric(vertical: 3),
                        child: Text(team.inviteCode,
                            style: AppText.s(22,
                                w: FontWeight.w700, color: Colors.white, ls: 1.5)),
                      ),
                      Text('Share this code with your classmates',
                          style: AppText.s(8.5, color: const Color(0xFF9FB0C8))),
                    ],
                  ),
                ),
                Material(
                  color: const Color(0xFF1B3555),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(9),
                    side: const BorderSide(color: Color(0xFF40546E)),
                  ),
                  child: InkWell(
                    borderRadius: BorderRadius.circular(9),
                    onTap: () async {
                      await Clipboard.setData(ClipboardData(text: team.inviteCode));
                      if (context.mounted) showAppSnack(context, 'Invitation code copied');
                    },
                    child: Padding(
                      padding: const EdgeInsets.all(8),
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          const AppIcon(AppIcons.copy, color: Colors.white),
                          const SizedBox(width: 5),
                          Text('Copy', style: AppText.s(9, color: Colors.white)),
                        ],
                      ),
                    ),
                  ),
                ),
              ],
            ),
          ),
          _StatRow(stats: [
            ('${team.members.length}', 'Members'),
            (team.status, 'Team status'),
            (team.academicYear, 'Academic year'),
          ]),
          Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              SectionHeading('Team members',
                  trailing: '${team.members.length} of ${team.maxMembers} seats'),
              DividedList(
                children: [
                  for (final m in team.members)
                    Padding(
                      padding: const EdgeInsets.all(11),
                      child: Row(
                        children: [
                          AvatarBox(m.initials, muted: !m.isLeader),
                          const SizedBox(width: 9),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(m.name, style: AppText.s(10, w: FontWeight.w700)),
                                const SizedBox(height: 2),
                                Text(m.role, style: AppText.s(8, color: AppColors.muted)),
                              ],
                            ),
                          ),
                          if (m.isLeader)
                            const StatusBadge('Leader')
                          else
                            const IconSquareButton(icon: AppIcons.chevron),
                        ],
                      ),
                    ),
                ],
              ),
            ],
          ),
          AppButton(
            label: 'Invite another member',
            icon: AppIcons.plus,
            variant: ButtonVariant.outline,
            full: true,
            onPressed: () => _invite(context, team),
          ),
          GestureDetector(
            onTap: () => _leave(context),
            behavior: HitTestBehavior.opaque,
            child: Padding(
              padding: const EdgeInsets.symmetric(vertical: 4),
              child: Center(
                child: Text('Leave team',
                    style: AppText.s(9, w: FontWeight.w700, color: AppColors.red700)),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

/// `.stat-row` – three equal stat cells separated by lines.
class _StatRow extends StatelessWidget {
  const _StatRow({required this.stats});

  final List<(String, String)> stats;

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        border: Border.all(color: AppColors.line),
        borderRadius: BorderRadius.circular(13),
      ),
      child: IntrinsicHeight(
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            for (var i = 0; i < stats.length; i++) ...[
              Expanded(
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 11),
                  child: Column(
                    children: [
                      Text(stats[i].$1,
                          style: AppText.s(11, w: FontWeight.w700, color: AppColors.navy900)),
                      const SizedBox(height: 3),
                      Text(stats[i].$2, style: AppText.s(7.5, color: AppColors.muted)),
                    ],
                  ),
                ),
              ),
              if (i < stats.length - 1) const VerticalDivider(width: 1, color: AppColors.line),
            ],
          ],
        ),
      ),
    );
  }
}

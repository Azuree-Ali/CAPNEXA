import 'package:flutter/material.dart';

import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_card.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/async_scaffold.dart';
import 'package:capnexa/core/widgets/avatar_box.dart';
import 'package:capnexa/core/widgets/dashed_border.dart';
import 'package:capnexa/core/widgets/status_badge.dart';
import 'package:capnexa/models/supervisor.dart';
import 'package:capnexa/services/service_locator.dart';

class SupervisorScreen extends StatelessWidget {
  const SupervisorScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AsyncScaffold<Supervisor>(
      title: 'Your supervisor',
      subtitle: 'Assignment confirmed',
      tab: AppTab.project,
      loader: ServiceLocator.projects.getSupervisor,
      builder: (context, s) => ScreenBody(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(10, 10, 10, 4),
            child: Column(
              children: [
                SupervisorPhoto(s.initials),
                const SizedBox(height: 10),
                const StatusBadge('Assigned', tone: BadgeTone.teal),
                const SizedBox(height: 8),
                Text(s.name,
                    style: AppText.s(19, w: FontWeight.w700, color: AppColors.navy900, ls: -.3)),
                Padding(
                  padding: const EdgeInsets.symmetric(vertical: 3),
                  child: Text(s.title, textAlign: TextAlign.center, style: AppText.p),
                ),
                const SizedBox(height: 7),
                Wrap(
                  alignment: WrapAlignment.center,
                  spacing: 5,
                  runSpacing: 5,
                  children: [
                    for (final tag in s.specialties)
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 7, vertical: 4),
                        decoration: BoxDecoration(
                          color: const Color(0xFFEEF1F5),
                          borderRadius: BorderRadius.circular(6),
                        ),
                        child: Text(tag, style: AppText.s(7, color: const Color(0xFF59677A))),
                      ),
                  ],
                ),
              ],
            ),
          ),
          AppCard(
            shadow: false,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Padding(
                  padding: const EdgeInsets.only(bottom: 10),
                  child: Text('Contact & office', style: AppText.h3),
                ),
                const Divider(height: 1, color: AppColors.line),
                _DetailRow(icon: AppIcons.mail, label: 'University email', value: s.email),
                _DetailRow(icon: AppIcons.school, label: 'Office', value: s.office),
                _DetailRow(icon: AppIcons.clock, label: 'Office hours', value: s.officeHours),
              ],
            ),
          ),
          Row(
            children: [
              Expanded(child: _InfoTile(label: 'ASSIGNMENT DATE', value: s.assignmentDate)),
              const SizedBox(width: 8),
              Expanded(child: _InfoTile(label: 'SUPERVISION GROUP', value: s.group)),
            ],
          ),
          AppButton(
            label: 'Open project workspace',
            icon: AppIcons.arrow,
            full: true,
            onPressed: () => AppNav.go(context, Routes.project),
          ),
          DashedBorder(
            color: const Color(0xFFCBD4E1),
            radius: 13,
            padding: const EdgeInsets.all(12),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                ListIcon(
                  size: 34,
                  radius: 10,
                  tone: IconTone.orange,
                  child: (c) => AppIcon(AppIcons.clock, color: c),
                ),
                const SizedBox(width: 11),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const StatusBadge('Alternative state', tone: BadgeTone.orange),
                      const SizedBox(height: 7),
                      Text('Waiting for supervisor assignment', style: AppText.h3),
                      const SizedBox(height: 2),
                      Text('Your approved project is in the assignment queue.',
                          style: AppText.s(8.5, color: AppColors.muted, height: 1.6)),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _DetailRow extends StatelessWidget {
  const _DetailRow({required this.icon, required this.label, required this.value});

  final AppIcons icon;
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(top: 12),
      child: Row(
        children: [
          AppIcon(icon, color: AppColors.blue700),
          const SizedBox(width: 10),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(label, style: AppText.s(7.5, color: AppColors.muted, height: 1.6)),
                const SizedBox(height: 2),
                Text(value, style: AppText.s(9, w: FontWeight.w700, color: AppColors.navy900)),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _InfoTile extends StatelessWidget {
  const _InfoTile({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(11),
      decoration: BoxDecoration(
        color: const Color(0xFFF3F5F8),
        borderRadius: BorderRadius.circular(11),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: AppText.s(7, color: AppColors.muted)),
          const SizedBox(height: 4),
          Text(value, style: AppText.s(9, w: FontWeight.w700, color: AppColors.navy900)),
        ],
      ),
    );
  }
}

import 'package:flutter/material.dart';

import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/async_scaffold.dart';
import 'package:capnexa/core/widgets/status_badge.dart';
import 'package:capnexa/models/submission.dart';
import 'package:capnexa/services/service_locator.dart';

class HistoryScreen extends StatelessWidget {
  const HistoryScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AsyncScaffold<List<SubmissionVersion>>(
      title: 'Submission history',
      subtitle: 'System Design',
      tab: AppTab.project,
      loader: ServiceLocator.submissions.getVersions,
      builder: (context, versions) => ScreenBody(
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 170),
                child: Text('All submitted versions are securely archived.', style: AppText.p),
              ),
              AppButton(
                label: 'New version',
                icon: AppIcons.upload,
                variant: ButtonVariant.outline,
                onPressed: () => AppNav.push(context, Routes.submission),
              ),
            ],
          ),
          Column(
            children: [
              for (var i = 0; i < versions.length; i++) ...[
                if (i > 0) const SizedBox(height: 10),
                _VersionCard(
                  version: versions[i],
                  onTap: () => AppNav.push(
                      context, versions[i].isLatest ? Routes.similarity : Routes.feedback),
                ),
              ],
            ],
          ),
        ],
      ),
    );
  }
}

class _VersionCard extends StatelessWidget {
  const _VersionCard({required this.version, required this.onTap});

  final SubmissionVersion version;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final v = version;
    final tone = switch (v.status) {
      VersionStatus.processing => BadgeTone.orange,
      VersionStatus.revisionNeeded => BadgeTone.red,
      VersionStatus.replaced => BadgeTone.gray,
      VersionStatus.approved => BadgeTone.teal,
    };
    Widget stat(String label, Widget value, {bool first = false, bool last = false}) => Container(
          padding: EdgeInsets.only(left: first ? 0 : 8),
          decoration: BoxDecoration(
            border: last ? null : const Border(right: BorderSide(color: AppColors.line)),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(label, style: AppText.s(6.5, color: const Color(0xFF929DAD))),
              const SizedBox(height: 4),
              value,
            ],
          ),
        );
    return Container(
      clipBehavior: Clip.antiAlias,
      decoration: BoxDecoration(
        color: Colors.white,
        border: Border.all(color: v.isLatest ? const Color(0xFFB8C8ED) : AppColors.line),
        borderRadius: BorderRadius.circular(14),
        boxShadow: v.isLatest
            ? const [
                BoxShadow(color: Color.fromRGBO(39, 84, 197, .07), blurRadius: 18, offset: Offset(0, 6)),
              ]
            : null,
      ),
      child: Material(
        color: Colors.white,
        child: InkWell(
          onTap: onTap,
          child: Column(
            children: [
              Padding(
                padding: const EdgeInsets.all(12),
                child: Row(
                  children: [
                    Container(
                      width: 34,
                      height: 34,
                      alignment: Alignment.center,
                      decoration: BoxDecoration(
                        color: v.isLatest ? AppColors.blue100 : const Color(0xFFEEF1F6),
                        borderRadius: BorderRadius.circular(9),
                      ),
                      child: Text('V${v.number}',
                          style: AppText.s(9,
                              w: FontWeight.w800,
                              color: v.isLatest ? AppColors.blue700 : AppColors.navy800)),
                    ),
                    const SizedBox(width: 9),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            children: [
                              Text('Version ${v.number}',
                                  style: AppText.s(10, w: FontWeight.w700, color: AppColors.navy900)),
                              if (v.isLatest)
                                Container(
                                  margin: const EdgeInsets.only(left: 4),
                                  padding: const EdgeInsets.symmetric(horizontal: 5, vertical: 3),
                                  decoration: BoxDecoration(
                                    color: AppColors.blue100,
                                    borderRadius: BorderRadius.circular(8),
                                  ),
                                  child: Text('Latest', style: AppText.s(7, color: AppColors.blue700)),
                                ),
                            ],
                          ),
                          const SizedBox(height: 2),
                          Text(v.date, style: AppText.s(7.5, color: AppColors.muted)),
                        ],
                      ),
                    ),
                    const AppIcon(AppIcons.chevron),
                  ],
                ),
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 9),
                decoration: const BoxDecoration(
                  color: Color(0xFFFAFBFC),
                  border: Border(top: BorderSide(color: AppColors.line)),
                ),
                child: IntrinsicHeight(
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      Expanded(
                        flex: 14,
                        child: stat('STATUS',
                            Align(alignment: Alignment.centerLeft, child: StatusBadge(v.statusLabel, tone: tone)),
                            first: true),
                      ),
                      Expanded(
                        flex: 8,
                        child: stat('SIMILARITY',
                            Text(v.similarity,
                                style: AppText.s(8, w: FontWeight.w700, color: AppColors.navy900))),
                      ),
                      Expanded(
                        flex: 10,
                        child: stat(
                            'REVIEW',
                            Text(v.review,
                                style: AppText.s(8, w: FontWeight.w700, color: AppColors.navy900)),
                            last: true),
                      ),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

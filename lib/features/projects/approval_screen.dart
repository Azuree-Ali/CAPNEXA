import 'package:flutter/material.dart';

import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/async_scaffold.dart';
import 'package:capnexa/core/widgets/dashed_border.dart';
import 'package:capnexa/core/widgets/info_note.dart';
import 'package:capnexa/core/widgets/section_heading.dart';
import 'package:capnexa/core/widgets/status_badge.dart';
import 'package:capnexa/core/widgets/timeline_item.dart';
import 'package:capnexa/models/proposal.dart';
import 'package:capnexa/services/service_locator.dart';

class ApprovalScreen extends StatefulWidget {
  const ApprovalScreen({super.key});

  @override
  State<ApprovalScreen> createState() => _ApprovalScreenState();
}

class _ApprovalScreenState extends State<ApprovalScreen> {
  /// Lets reviewers preview the "rejected" state described in the design note.
  bool _previewRejected = false;

  @override
  Widget build(BuildContext context) {
    return AsyncScaffold<ProposalStatusInfo>(
      title: 'Proposal status',
      tab: AppTab.project,
      loader: ServiceLocator.projects.getProposalStatus,
      subtitleBuilder: (s) => s.projectTitle,
      builder: (context, info) {
        final rejected = _previewRejected || info.status == ProposalStatus.rejected;
        return ScreenBody(
          children: [
            rejected ? _RejectedHero(info: info) : _ApprovedHero(info: info),
            if (!rejected) ...[
              Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  const SectionHeading('Review progress'),
                  Padding(
                    padding: const EdgeInsets.only(left: 3),
                    child: Column(
                      children: [
                        for (var i = 0; i < info.steps.length; i++)
                          _reviewStep(info.steps[i], i, i == info.steps.length - 1),
                      ],
                    ),
                  ),
                ],
              ),
              const InfoNote(
                icon: AppIcons.bell,
                message: 'We’ll notify every team member when a supervisor has been assigned.',
              ),
              AppButton(
                label: 'Continue',
                icon: AppIcons.arrow,
                full: true,
                onPressed: () => AppNav.push(context, Routes.supervisor),
              ),
            ] else ...[
              AppButton(
                label: 'Edit & resubmit proposal',
                icon: AppIcons.edit,
                full: true,
                onPressed: () => AppNav.push(context, Routes.proposal),
              ),
            ],
            DashedBorder(
              color: const Color(0xFFE1AEB2),
              radius: 12,
              padding: const EdgeInsets.all(12),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const StatusBadge('Rejected state', tone: BadgeTone.red),
                  Padding(
                    padding: const EdgeInsets.fromLTRB(0, 7, 0, 3),
                    child: Text(
                      'When rejected, the reason appears here with clear actions to edit and resubmit the proposal.',
                      style: AppText.p,
                    ),
                  ),
                  AppButton(
                    label: _previewRejected ? 'Back to approved state' : 'Preview rejected state',
                    icon: AppIcons.edit,
                    variant: ButtonVariant.text,
                    onPressed: () => setState(() => _previewRejected = !_previewRejected),
                  ),
                ],
              ),
            ),
          ],
        );
      },
    );
  }

  /// `.review-timeline` row.
  Widget _reviewStep(ReviewStep step, int index, bool isLast) {
    final state = switch (step.state) {
      ReviewState.complete => TimelineState.done,
      ReviewState.current => TimelineState.current,
      ReviewState.pending => TimelineState.pending,
    };
    return TimelineItem(
      isLast: isLast,
      gap: 11,
      marker: TimelineMarker(
        state: state,
        index: index + 1,
        pendingBorder: AppColors.fieldBorder,
        pendingText: const Color(0xFF8793A5),
      ),
      content: Padding(
        padding: const EdgeInsets.only(bottom: 14, top: 2),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(step.title, style: AppText.s(10, w: FontWeight.w700)),
            const SizedBox(height: 1),
            Text(step.subtitle, style: AppText.s(8.5, color: AppColors.muted, height: 1.6)),
          ],
        ),
      ),
    );
  }
}

class _ApprovedHero extends StatelessWidget {
  const _ApprovedHero({required this.info});

  final ProposalStatusInfo info;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.fromLTRB(15, 19, 15, 19),
      decoration: BoxDecoration(
        color: const Color(0xFFF4FBF9),
        border: Border.all(color: const Color(0xFFCAE9E2)),
        borderRadius: BorderRadius.circular(17),
      ),
      child: Column(
        children: [
          Container(
            width: 58,
            height: 58,
            alignment: Alignment.center,
            decoration: BoxDecoration(
              color: AppColors.teal700,
              borderRadius: BorderRadius.circular(17),
            ),
            child: const AppIcon(AppIcons.check, size: 31, color: Colors.white),
          ),
          const SizedBox(height: 11),
          const StatusBadge('Approved', tone: BadgeTone.teal),
          const SizedBox(height: 9),
          Text('Your proposal is approved',
              textAlign: TextAlign.center,
              style: AppText.s(19, w: FontWeight.w700, color: AppColors.navy900, ls: -.3)),
          Padding(
            padding: const EdgeInsets.fromLTRB(0, 6, 0, 14),
            child: Text(
              '${info.projectTitle} has been accepted by the Department Review Committee. You’re ready for supervisor assignment.',
              textAlign: TextAlign.center,
              style: AppText.p,
            ),
          ),
          _Reference(id: info.proposalId, borderColor: const Color(0xFFD5ECE7)),
        ],
      ),
    );
  }
}

/// Rejected variant, composed from the same hero pattern in the red tone
/// (the Figma file only describes this state in a note).
class _RejectedHero extends StatelessWidget {
  const _RejectedHero({required this.info});

  final ProposalStatusInfo info;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.fromLTRB(15, 19, 15, 19),
      decoration: BoxDecoration(
        color: const Color(0xFFFFF7F7),
        border: Border.all(color: const Color(0xFFEFC9CC)),
        borderRadius: BorderRadius.circular(17),
      ),
      child: Column(
        children: [
          Container(
            width: 58,
            height: 58,
            alignment: Alignment.center,
            decoration: BoxDecoration(
              color: AppColors.red700,
              borderRadius: BorderRadius.circular(17),
            ),
            child: const AppIcon(AppIcons.warning, size: 31, color: Colors.white),
          ),
          const SizedBox(height: 11),
          const StatusBadge('Rejected', tone: BadgeTone.red),
          const SizedBox(height: 9),
          Text('Your proposal needs changes',
              textAlign: TextAlign.center,
              style: AppText.s(19, w: FontWeight.w700, color: AppColors.navy900, ls: -.3)),
          Padding(
            padding: const EdgeInsets.fromLTRB(0, 6, 0, 14),
            child: Text(info.rejectionReason ?? 'Please review the feedback and resubmit.',
                textAlign: TextAlign.center, style: AppText.p),
          ),
          _Reference(id: info.proposalId, borderColor: const Color(0xFFF1D5D7)),
        ],
      ),
    );
  }
}

class _Reference extends StatelessWidget {
  const _Reference({required this.id, required this.borderColor});

  final String id;
  final Color borderColor;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.only(top: 11),
      decoration: BoxDecoration(border: Border(top: BorderSide(color: borderColor))),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Text('Proposal ID', style: AppText.s(8.5, color: AppColors.muted)),
          const SizedBox(width: 5),
          Text(id, style: AppText.s(8.5, w: FontWeight.w700, color: AppColors.navy900)),
        ],
      ),
    );
  }
}

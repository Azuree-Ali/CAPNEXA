import 'package:flutter/material.dart';

import 'package:capnexa/core/network/api_exception.dart';
import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_dialog.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/app_text_field.dart';
import 'package:capnexa/core/widgets/async_scaffold.dart';
import 'package:capnexa/core/widgets/state_card.dart';
import 'package:capnexa/core/widgets/status_badge.dart';
import 'package:capnexa/models/proposal.dart';
import 'package:capnexa/services/service_locator.dart';

class ProposalScreen extends StatelessWidget {
  const ProposalScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AsyncScaffold<ProposalDraft>(
      title: 'Project proposal',
      tab: AppTab.project,
      loader: ServiceLocator.projects.getProposalDraft,
      subtitleBuilder: (d) => d.lastSaved,
      actionBuilder: (_, d) =>
          StatusBadge(d.projectId == null ? 'New' : 'Draft', tone: BadgeTone.gray),
      builder: (context, draft) {
        if (draft.status == 'PendingReview' || draft.status == 'Approved') {
          return ScreenBody(
            children: [
              StateCard(
                kind: StateKind.success,
                title: draft.status == 'Approved' ? 'Proposal approved' : 'Proposal submitted',
                message: 'This proposal can no longer be edited.',
                actionLabel: 'View status',
                actionIcon: AppIcons.arrow,
                onAction: () => AppNav.push(context, Routes.approval),
              ),
            ],
          );
        }
        return _ProposalForm(draft: draft);
      },
    );
  }
}

class _ProposalForm extends StatefulWidget {
  const _ProposalForm({required this.draft});

  final ProposalDraft draft;

  @override
  State<_ProposalForm> createState() => _ProposalFormState();
}

class _ProposalFormState extends State<_ProposalForm> {
  late final _title = TextEditingController(text: widget.draft.title);
  late final _description = TextEditingController(text: widget.draft.description);
  late int? _projectId = widget.draft.projectId;
  bool _busy = false;

  @override
  void dispose() {
    _title.dispose();
    _description.dispose();
    super.dispose();
  }

  ProposalDraft _collect() => ProposalDraft(
        title: _title.text,
        description: _description.text,
        lastSaved: widget.draft.lastSaved,
        memberInitials: widget.draft.memberInitials,
        projectId: _projectId,
      );

  String? _validate() {
    final t = _title.text.trim();
    final d = _description.text.trim();
    if (t.length < 3 || t.length > 250) return 'Title must be 3–250 characters.';
    if (d.length < 10 || d.length > 4000) return 'Description must be 10–4000 characters.';
    return null;
  }

  Future<void> _run(Future<void> Function() action) async {
    final error = _validate();
    if (error != null) {
      showAppSnack(context, error);
      return;
    }
    if (_busy) return;
    setState(() => _busy = true);
    try {
      await action();
    } on ApiException catch (e) {
      if (mounted) showAppSnack(context, e.message);
    } catch (_) {
      if (mounted) showAppSnack(context, 'Something went wrong. Please try again.');
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _save() => _run(() async {
        final saved = await ServiceLocator.projects.saveProposalDraft(_collect());
        _projectId = saved.projectId;
        if (mounted) showAppSnack(context, 'Draft saved');
      });

  Future<void> _review() => _run(() async {
        await ServiceLocator.projects.submitProposal(_collect());
        if (mounted) AppNav.go(context, Routes.approval);
      });

  @override
  Widget build(BuildContext context) {
    final members = widget.draft.memberInitials;
    return Column(
      children: [
        Expanded(
          child: ScreenBody(
            padding: const EdgeInsets.fromLTRB(17, 16, 17, 12),
            children: [
              const _StepProgress(active: 1),
              Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  AppTextField(label: 'Project title', controller: _title),
                  const SizedBox(height: 14),
                  AppTextField(label: 'Project description', textarea: true, controller: _description),
                ],
              ),
              if (members.isNotEmpty)
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    border: Border.all(color: AppColors.line),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Row(
                    children: [
                      Text('TEAM MEMBERS',
                          style: AppText.s(8, w: FontWeight.w800, color: AppColors.muted)),
                      const SizedBox(width: 9),
                      SizedBox(
                        height: 25,
                        width: 25.0 + (members.length - 1) * 19,
                        child: Stack(
                          children: [
                            for (var i = 0; i < members.length; i++)
                              Positioned(
                                left: i * 19.0,
                                child: Container(
                                  width: 25,
                                  height: 25,
                                  alignment: Alignment.center,
                                  decoration: BoxDecoration(
                                    color: AppColors.navy800,
                                    shape: BoxShape.circle,
                                    border: Border.all(color: Colors.white, width: 2),
                                  ),
                                  child: Text(members[i],
                                      style: AppText.s(7, color: Colors.white)),
                                ),
                              ),
                          ],
                        ),
                      ),
                      const Spacer(),
                      Text('${members.length} members included',
                          style: AppText.s(8, color: AppColors.muted)),
                    ],
                  ),
                ),
              Text('Any team member can edit the draft. Only the team leader can submit it.',
                  style: AppText.s(8.5, color: AppColors.muted, height: 1.5)),
            ],
          ),
        ),
        Container(
          color: const Color.fromRGBO(251, 252, 254, .95),
          padding: const EdgeInsets.fromLTRB(17, 9, 17, 12),
          child: Row(
            children: [
              Expanded(
                flex: 10,
                child: AppButton(
                    label: 'Save draft', variant: ButtonVariant.outline, onPressed: _save, full: true),
              ),
              const SizedBox(width: 8),
              Expanded(
                flex: 15,
                child: AppButton(
                    label: 'Submit proposal', icon: AppIcons.arrow, onPressed: _review, full: true),
              ),
            ],
          ),
        ),
      ],
    );
  }
}

/// `.step-progress` – Details / Scope / Review.
class _StepProgress extends StatelessWidget {
  const _StepProgress({required this.active});

  /// Zero-based index of the active step.
  final int active;

  static const _labels = ['Details', 'Scope', 'Review'];

  @override
  Widget build(BuildContext context) {
    Widget step(int i) {
      final isActive = i == active;
      return SizedBox(
        width: 53,
        child: Column(
          children: [
            Container(
              width: 24,
              height: 24,
              alignment: Alignment.center,
              decoration: BoxDecoration(
                color: isActive ? AppColors.blue700 : const Color(0xFFEEF1F5),
                shape: BoxShape.circle,
              ),
              child: Text('${i + 1}',
                  style: AppText.s(9,
                      w: FontWeight.w800,
                      color: isActive ? Colors.white : AppColors.chevron)),
            ),
            const SizedBox(height: 4),
            Text(_labels[i],
                style: AppText.s(7,
                    w: FontWeight.w700,
                    color: isActive ? AppColors.blue700 : AppColors.chevron)),
          ],
        ),
      );
    }

    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        step(0),
        Container(
            width: 55,
            height: 1,
            margin: const EdgeInsets.only(top: 12),
            color: const Color(0xFFDFE4EC)),
        step(1),
        Container(
            width: 55,
            height: 1,
            margin: const EdgeInsets.only(top: 12),
            color: const Color(0xFFDFE4EC)),
        step(2),
      ],
    );
  }
}

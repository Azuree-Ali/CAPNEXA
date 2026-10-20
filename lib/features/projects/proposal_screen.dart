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
import 'package:capnexa/core/widgets/async_scaffold.dart';
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
      actionBuilder: (_, __) => const StatusBadge('Draft', tone: BadgeTone.gray),
      builder: (context, draft) => _ProposalForm(draft: draft),
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
  late final _problem = TextEditingController(text: widget.draft.problemStatement);
  late final _solution = TextEditingController(text: widget.draft.proposedSolution);
  late final _tech = TextEditingController(text: widget.draft.technologies);
  late final _category = TextEditingController(text: widget.draft.category);

  @override
  void dispose() {
    for (final c in [_title, _description, _problem, _solution, _tech, _category]) {
      c.dispose();
    }
    super.dispose();
  }

  ProposalDraft _collect() => ProposalDraft(
        title: _title.text,
        description: _description.text,
        problemStatement: _problem.text,
        proposedSolution: _solution.text,
        technologies: _tech.text,
        category: _category.text,
        lastSaved: widget.draft.lastSaved,
        memberInitials: widget.draft.memberInitials,
      );

  Future<void> _save() async {
    await ServiceLocator.projects.saveProposalDraft(_collect());
    if (mounted) showAppSnack(context, 'Draft saved');
  }

  Future<void> _review() async {
    await ServiceLocator.projects.submitProposal(_collect());
    if (mounted) AppNav.push(context, Routes.approval);
  }

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
                  const SizedBox(height: 14),
                  AppTextField(label: 'Problem statement', textarea: true, controller: _problem),
                  const SizedBox(height: 14),
                  AppTextField(label: 'Proposed solution', textarea: true, controller: _solution),
                  const SizedBox(height: 14),
                  AppTextField(label: 'Technologies', controller: _tech),
                  const SizedBox(height: 14),
                  AppTextField(label: 'Project category', controller: _category),
                ],
              ),
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
                    label: 'Review proposal', icon: AppIcons.arrow, onPressed: _review, full: true),
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
                      w: FontWeight.w800, color: isActive ? Colors.white : AppColors.chevron)),
            ),
            const SizedBox(height: 4),
            Text(_labels[i],
                style: AppText.s(7,
                    w: FontWeight.w700, color: isActive ? AppColors.blue700 : AppColors.chevron)),
          ],
        ),
      );
    }

    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        step(0),
        Container(width: 55, height: 1, margin: const EdgeInsets.only(top: 12), color: const Color(0xFFDFE4EC)),
        step(1),
        Container(width: 55, height: 1, margin: const EdgeInsets.only(top: 12), color: const Color(0xFFDFE4EC)),
        step(2),
      ],
    );
  }
}

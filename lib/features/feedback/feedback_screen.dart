import 'package:flutter/material.dart';

import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_card.dart';
import 'package:capnexa/core/widgets/app_dialog.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/app_text_field.dart';
import 'package:capnexa/core/widgets/async_scaffold.dart';
import 'package:capnexa/core/widgets/avatar_box.dart';
import 'package:capnexa/core/widgets/section_heading.dart';
import 'package:capnexa/core/widgets/status_badge.dart';
import 'package:capnexa/models/feedback.dart';
import 'package:capnexa/services/service_locator.dart';

class FeedbackScreen extends StatelessWidget {
  const FeedbackScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AsyncScaffold<SupervisorFeedback>(
      title: 'Supervisor feedback',
      tab: AppTab.project,
      loader: ServiceLocator.feedback.getLatestFeedback,
      subtitleBuilder: (f) => f.versionLabel,
      actionBuilder: (_, __) => const StatusBadge('Revision needed', tone: BadgeTone.orange),
      builder: (context, f) => _FeedbackBody(feedback: f),
    );
  }
}

class _FeedbackBody extends StatefulWidget {
  const _FeedbackBody({required this.feedback});

  final SupervisorFeedback feedback;

  @override
  State<_FeedbackBody> createState() => _FeedbackBodyState();
}

class _FeedbackBodyState extends State<_FeedbackBody> {
  late final List<bool> _done = [for (final c in widget.feedback.changes) c.done];
  final _reply = TextEditingController();

  @override
  void dispose() {
    _reply.dispose();
    super.dispose();
  }

  Future<void> _send() async {
    if (_reply.text.trim().isEmpty) {
      showAppSnack(context, 'Write a short reply first.');
      return;
    }
    await ServiceLocator.feedback.sendReply(_reply.text.trim());
    if (!mounted) return;
    _reply.clear();
    showAppSnack(context, 'Reply sent to your supervisor');
  }

  @override
  Widget build(BuildContext context) {
    final f = widget.feedback;
    return ScreenBody(
      children: [
        Row(
          children: [
            SupervisorPhoto(f.authorInitials, size: 45, radius: 13, fontSize: 12, bordered: true),
            const SizedBox(width: 10),
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(f.authorName, style: AppText.s(10, w: FontWeight.w700)),
                const SizedBox(height: 2),
                Text(f.date, style: AppText.s(8, color: AppColors.muted)),
              ],
            ),
          ],
        ),
        Container(
          padding: const EdgeInsets.all(18),
          decoration: BoxDecoration(
            color: AppColors.navy900,
            borderRadius: BorderRadius.circular(15),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              SizedBox(
                height: 22,
                child: Text('“',
                    style: TextStyle(
                        fontFamily: 'Georgia',
                        fontFamilyFallback: const ['serif'],
                        fontSize: 45,
                        height: .5,
                        color: const Color(0xFF6683AA))),
              ),
              const SizedBox(height: 7),
              Text(f.message, style: AppText.s(10, color: const Color(0xFFD3DAE4), height: 1.6)),
            ],
          ),
        ),
        Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            SectionHeading('Required changes', trailing: '${f.changes.length} items'),
            DividedList(
              radius: 13,
              children: [
                for (var i = 0; i < f.changes.length; i++)
                  InkWell(
                    onTap: () => setState(() => _done[i] = !_done[i]),
                    child: Padding(
                      padding: const EdgeInsets.all(11),
                      child: Row(
                        children: [
                          AnimatedContainer(
                            duration: const Duration(milliseconds: 120),
                            width: 19,
                            height: 19,
                            alignment: Alignment.center,
                            decoration: BoxDecoration(
                              color: _done[i] ? AppColors.teal700 : Colors.white,
                              borderRadius: BorderRadius.circular(6),
                              border: Border.all(
                                  color: _done[i] ? AppColors.teal700 : const Color(0xFFCBD3DF)),
                            ),
                            child: _done[i]
                                ? const AppIcon(AppIcons.check, size: 14, color: Colors.white)
                                : null,
                          ),
                          const SizedBox(width: 9),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(f.changes[i].title,
                                    style: AppText.s(9, w: FontWeight.w700, color: AppColors.navy900)),
                                Text(f.changes[i].location,
                                    style: AppText.s(7.5, color: AppColors.muted, height: 1.6)),
                              ],
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
              ],
            ),
          ],
        ),
        Container(
          padding: const EdgeInsets.all(13),
          decoration: BoxDecoration(
            color: AppColors.softFill,
            borderRadius: BorderRadius.circular(13),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              AppTextField(
                label: 'Reply to supervisor',
                hint: 'Add a concise response...',
                textarea: true,
                controller: _reply,
              ),
              const SizedBox(height: 9),
              AppButton(
                label: 'Send reply',
                icon: AppIcons.message,
                variant: ButtonVariant.outline,
                onPressed: _send,
              ),
            ],
          ),
        ),
        AppButton(
          label: 'Upload revised submission',
          icon: AppIcons.upload,
          full: true,
          onPressed: () => AppNav.push(context, Routes.submission),
        ),
      ],
    );
  }
}

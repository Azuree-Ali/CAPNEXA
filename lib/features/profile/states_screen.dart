import 'package:flutter/material.dart';

import 'package:capnexa/core/widgets/app_dialog.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/section_heading.dart';
import 'package:capnexa/core/widgets/state_card.dart';
import 'package:capnexa/core/widgets/status_badge.dart';

/// "System states" – the reusable status / loading / empty / error / success patterns.
class StatesScreen extends StatelessWidget {
  const StatesScreen({super.key});

  @override
  Widget build(BuildContext context) {
    void demo() => showAppSnack(context, 'Demo only');
    return AppScaffold(
      title: 'System states',
      subtitle: 'Reusable patterns',
      body: ScreenBody(
        gap: 20,
        children: [
          const Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              SectionHeading('Status system'),
              Wrap(
                spacing: 7,
                runSpacing: 7,
                children: [
                  StatusBadge('Draft', tone: BadgeTone.gray),
                  StatusBadge('Pending', tone: BadgeTone.orange),
                  StatusBadge('In progress', tone: BadgeTone.blue),
                  StatusBadge('Approved', tone: BadgeTone.teal),
                  StatusBadge('Rejected', tone: BadgeTone.red),
                  StatusBadge('Overdue', tone: BadgeTone.red),
                ],
              ),
            ],
          ),
          const StateCard(
            kind: StateKind.loading,
            title: 'Analyzing your submission',
            message: 'This usually takes less than two minutes. You can safely leave this screen.',
          ),
          StateCard(
            kind: StateKind.empty,
            title: 'No submissions yet',
            message: 'Your uploaded milestone files will appear here.',
            actionLabel: 'Upload first file',
            actionIcon: AppIcons.upload,
            onAction: demo,
          ),
          StateCard(
            kind: StateKind.error,
            title: 'We couldn’t upload your file',
            message: 'Check your connection and make sure the file is under 50 MB.',
            actionLabel: 'Try again',
            onAction: demo,
          ),
          StateCard(
            kind: StateKind.success,
            title: 'Submission received',
            message: 'Version 3 is secure and queued for similarity analysis.',
            actionLabel: 'View submission',
            onAction: demo,
          ),
        ],
      ),
    );
  }
}

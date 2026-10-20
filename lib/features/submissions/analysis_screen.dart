import 'dart:async';

import 'package:flutter/material.dart';

import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/state_card.dart';

/// Business-flow step between "Upload Submission" and the similarity result:
/// Security Scan → AI Analysis. Uses the design's loading state card.
class AnalysisScreen extends StatefulWidget {
  const AnalysisScreen({super.key});

  @override
  State<AnalysisScreen> createState() => _AnalysisScreenState();
}

class _AnalysisScreenState extends State<AnalysisScreen> {
  int _stage = 0;
  final _timers = <Timer>[];

  @override
  void initState() {
    super.initState();
    _timers.add(Timer(const Duration(milliseconds: 1500), () {
      if (mounted) setState(() => _stage = 1);
    }));
    _timers.add(Timer(const Duration(milliseconds: 3200), () {
      if (mounted) Navigator.of(context).pushReplacementNamed(Routes.similarity);
    }));
  }

  @override
  void dispose() {
    for (final t in _timers) {
      t.cancel();
    }
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: 'Analyzing submission',
      subtitle: 'system-design-v3.pdf',
      tab: AppTab.project,
      body: ScreenBody(
        children: [
          if (_stage == 0)
            const StateCard(
              kind: StateKind.loading,
              title: 'Running security scan',
              message: 'Checking your files before analysis. This only takes a moment.',
            )
          else
            const StateCard(
              kind: StateKind.loading,
              title: 'Analyzing your submission',
              message: 'This usually takes less than two minutes. You can safely leave this screen.',
            ),
        ],
      ),
    );
  }
}

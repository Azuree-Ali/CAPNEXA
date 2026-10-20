import 'package:flutter/material.dart';

/// Scrollable column that fills the viewport so `Spacer` / `Expanded`
/// children can push content to the bottom on tall screens, while still
/// scrolling on short ones (splash, onboarding, auth screens).
class FullHeightScroll extends StatelessWidget {
  const FullHeightScroll({super.key, required this.child});

  final Widget child;

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) => SingleChildScrollView(
        child: ConstrainedBox(
          constraints: BoxConstraints(minHeight: constraints.maxHeight),
          child: IntrinsicHeight(child: child),
        ),
      ),
    );
  }
}

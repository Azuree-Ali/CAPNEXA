import 'package:flutter/material.dart';

import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/widgets/app_bottom_nav.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/state_card.dart';

/// Loads data from a service and renders an [AppScaffold] around it.
/// Shows the shared loading state while waiting and the shared error state
/// (with "Try again") if the future fails – this is the single place where
/// API latency / failures surface in the UI.
class AsyncScaffold<T> extends StatefulWidget {
  const AsyncScaffold({
    super.key,
    required this.title,
    required this.loader,
    required this.builder,
    this.subtitle,
    this.subtitleBuilder,
    this.actionBuilder,
    this.bottomBuilder,
    this.tab,
    this.backgroundColor = AppColors.scaffold,
    this.titleBuilder,
  });

  final String title;
  final String? subtitle;
  final Future<T> Function() loader;
  final Widget Function(BuildContext context, T data) builder;
  final String Function(T data)? subtitleBuilder;
  final String Function(T data)? titleBuilder;
  final Widget? Function(BuildContext context, T data)? actionBuilder;
  final Widget? Function(BuildContext context, T data)? bottomBuilder;
  final AppTab? tab;
  final Color backgroundColor;

  @override
  State<AsyncScaffold<T>> createState() => _AsyncScaffoldState<T>();
}

class _AsyncScaffoldState<T> extends State<AsyncScaffold<T>> {
  late Future<T> _future = widget.loader();

  void _retry() => setState(() => _future = widget.loader());

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<T>(
      future: _future,
      builder: (context, snap) {
        if (snap.connectionState != ConnectionState.done) {
          return AppScaffold(
            title: widget.title,
            subtitle: widget.subtitle,
            tab: widget.tab,
            backgroundColor: widget.backgroundColor,
            body: const LoadingView(),
          );
        }
        if (snap.hasError) {
          return AppScaffold(
            title: widget.title,
            subtitle: widget.subtitle,
            tab: widget.tab,
            backgroundColor: widget.backgroundColor,
            body: Padding(
              padding: const EdgeInsets.all(17),
              child: Align(
                alignment: Alignment.topCenter,
                child: StateCard(
                  kind: StateKind.error,
                  title: 'We couldn’t load this screen',
                  message: 'Check your connection and try again.',
                  actionLabel: 'Try again',
                  onAction: _retry,
                ),
              ),
            ),
          );
        }
        final data = snap.data as T;
        return AppScaffold(
          title: widget.titleBuilder?.call(data) ?? widget.title,
          subtitle: widget.subtitleBuilder?.call(data) ?? widget.subtitle,
          action: widget.actionBuilder?.call(context, data),
          bottom: widget.bottomBuilder?.call(context, data),
          tab: widget.tab,
          backgroundColor: widget.backgroundColor,
          body: widget.builder(context, data),
        );
      },
    );
  }
}

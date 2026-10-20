import 'package:flutter/material.dart';

import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/state_card.dart';
import 'package:capnexa/features/notifications/widgets/notification_card.dart';
import 'package:capnexa/models/app_notification.dart';
import 'package:capnexa/services/service_locator.dart';

class NotificationsScreen extends StatefulWidget {
  const NotificationsScreen({super.key});

  @override
  State<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends State<NotificationsScreen> {
  late Future<List<AppNotification>> _future = ServiceLocator.notifications.getNotifications();
  List<AppNotification>? _items;
  NotificationFilter _filter = NotificationFilter.all;

  static const _labels = {
    NotificationFilter.all: 'All',
    NotificationFilter.project: 'Project',
    NotificationFilter.feedback: 'Feedback',
    NotificationFilter.deadlines: 'Deadlines',
  };

  void _retry() => setState(() => _future = ServiceLocator.notifications.getNotifications());

  Future<void> _markAllRead() async {
    await ServiceLocator.notifications.markAllRead();
    if (!mounted) return;
    setState(() => _items = [for (final n in _items ?? const <AppNotification>[]) n.copyWith(unread: false)]);
  }

  String? _routeFor(NotificationKind kind) => switch (kind) {
        NotificationKind.feedback => Routes.feedback,
        NotificationKind.similarity => Routes.similarity,
        NotificationKind.deadline => Routes.milestone,
        NotificationKind.supervisor => Routes.supervisor,
        NotificationKind.approval => Routes.approval,
      };

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<List<AppNotification>>(
      future: _future,
      builder: (context, snap) {
        final loading = snap.connectionState != ConnectionState.done;
        if (!loading && snap.hasData) _items ??= snap.data;
        final items = _items;
        final unread = items?.where((n) => n.unread).length ?? 0;
        return AppScaffold(
          title: 'Notifications',
          subtitle: items == null ? null : '$unread unread updates',
          tab: AppTab.alerts,
          action: AppButton(
            label: 'Mark all read',
            variant: ButtonVariant.text,
            onPressed: items == null ? null : _markAllRead,
          ),
          body: loading
              ? const LoadingView()
              : snap.hasError
                  ? Padding(
                      padding: const EdgeInsets.all(17),
                      child: StateCard(
                        kind: StateKind.error,
                        title: 'We couldn’t load notifications',
                        message: 'Check your connection and try again.',
                        actionLabel: 'Try again',
                        onAction: _retry,
                      ),
                    )
                  : _buildList(context, items ?? const []),
        );
      },
    );
  }

  Widget _buildList(BuildContext context, List<AppNotification> items) {
    final visible = items.where((n) => n.matches(_filter)).toList();
    return ScreenBody(
      padding: const EdgeInsets.fromLTRB(0, 16, 0, 26),
      children: [
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 17),
          child: SingleChildScrollView(
            scrollDirection: Axis.horizontal,
            child: Row(
              children: [
                for (final f in NotificationFilter.values) ...[
                  _Chip(
                    label: _labels[f]!,
                    active: f == _filter,
                    onTap: () => setState(() => _filter = f),
                  ),
                  const SizedBox(width: 6),
                ],
              ],
            ),
          ),
        ),
        if (visible.isEmpty)
          const Padding(
            padding: EdgeInsets.symmetric(horizontal: 17),
            child: StateCard(
              kind: StateKind.empty,
              title: 'No notifications',
              message: 'Updates for this category will appear here.',
            ),
          )
        else
          Container(
            decoration: const BoxDecoration(border: Border(top: BorderSide(color: AppColors.line))),
            child: Column(
              children: [
                for (final n in visible)
                  NotificationCard(
                    notification: n,
                    onTap: () {
                      final route = _routeFor(n.kind);
                      if (route != null) AppNav.push(context, route);
                    },
                  ),
              ],
            ),
          ),
      ],
    );
  }
}

class _Chip extends StatelessWidget {
  const _Chip({required this.label, required this.active, required this.onTap});

  final String label;
  final bool active;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
        decoration: BoxDecoration(
          color: active ? AppColors.navy900 : Colors.white,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(color: active ? AppColors.navy900 : AppColors.line),
        ),
        child: Text(label,
            style: AppText.s(8, color: active ? Colors.white : AppColors.muted)),
      ),
    );
  }
}

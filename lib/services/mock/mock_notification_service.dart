import 'package:capnexa/core/constants/app_constants.dart';
import 'package:capnexa/models/app_notification.dart';
import 'package:capnexa/services/notification_service.dart';

class MockNotificationService implements NotificationService {
  @override
  Future<List<AppNotification>> getNotifications() async {
    await Future<void>.delayed(AppConstants.mockLatency);
    return const [
      AppNotification(
        kind: NotificationKind.feedback,
        title: 'New supervisor feedback',
        message: 'Dr. Omar requested changes to System Design.',
        time: '10 min',
        unread: true,
      ),
      AppNotification(
        kind: NotificationKind.similarity,
        title: 'Similarity analysis completed',
        message: 'Version 3 returned a 27% similarity result.',
        time: '1 hr',
        unread: true,
      ),
      AppNotification(
        kind: NotificationKind.deadline,
        title: 'Deadline approaching',
        message: 'System Design is due in 4 days.',
        time: '3 hr',
        unread: true,
      ),
      AppNotification(
        kind: NotificationKind.supervisor,
        title: 'Supervisor assigned',
        message: 'Dr. Omar Rahman has joined your project.',
        time: 'May 15',
      ),
      AppNotification(
        kind: NotificationKind.approval,
        title: 'Project proposal approved',
        message: 'Your team can now begin Campus Connect.',
        time: 'May 12',
      ),
    ];
  }

  @override
  Future<void> markAllRead() async {}
}

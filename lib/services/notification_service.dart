import 'package:capnexa/models/app_notification.dart';

/// Notification contract.
/// TODO(api): `GET /notifications`, `POST /notifications/read-all`
/// (+ push registration via FCM later).
abstract class NotificationService {
  Future<List<AppNotification>> getNotifications();
  Future<void> markAllRead();
}

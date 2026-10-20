enum NotificationKind { feedback, similarity, deadline, supervisor, approval }

enum NotificationFilter { all, project, feedback, deadlines }

class AppNotification {
  const AppNotification({
    required this.kind,
    required this.title,
    required this.message,
    required this.time,
    this.unread = false,
  });

  final NotificationKind kind;
  final String title;
  final String message;
  final String time;
  final bool unread;

  AppNotification copyWith({bool? unread}) => AppNotification(
        kind: kind,
        title: title,
        message: message,
        time: time,
        unread: unread ?? this.unread,
      );

  bool matches(NotificationFilter f) => switch (f) {
        NotificationFilter.all => true,
        NotificationFilter.feedback => kind == NotificationKind.feedback,
        NotificationFilter.deadlines => kind == NotificationKind.deadline,
        NotificationFilter.project => kind == NotificationKind.similarity ||
            kind == NotificationKind.supervisor ||
            kind == NotificationKind.approval,
      };
}

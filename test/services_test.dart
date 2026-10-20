import 'package:flutter_test/flutter_test.dart';

import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/models/app_notification.dart';
import 'package:capnexa/models/milestone.dart';
import 'package:capnexa/services/service_locator.dart';

void main() {
  setUp(ServiceLocator.setup);

  test('home summary is composed from mock services', () async {
    final summary = await ServiceLocator.home.getSummary();
    expect(summary.student.firstName, 'Lina');
    expect(summary.project.progress, 62);
    expect(summary.nextDeadline.milestone.daysLeft, 4);
  });

  test('milestone plan has a single in-progress milestone', () async {
    final plan = await ServiceLocator.milestones.getPlan();
    expect(plan.milestones.length, 6);
    expect(plan.inProgressNumber, 3);
    expect(
      plan.milestones.where((m) => m.status == MilestoneStatus.inProgress).length,
      1,
    );
  });

  test('notification filters', () async {
    final items = await ServiceLocator.notifications.getNotifications();
    expect(items.where((n) => n.matches(NotificationFilter.feedback)).length, 1);
    expect(items.where((n) => n.unread).length, 3);
  });

  test('every journey item has a unique route', () {
    final routes = journeyItems.map((e) => e.route).toSet();
    expect(routes.length, journeyItems.length);
  });
}

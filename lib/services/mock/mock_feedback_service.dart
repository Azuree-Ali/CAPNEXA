import 'package:capnexa/core/constants/app_constants.dart';
import 'package:capnexa/models/feedback.dart';
import 'package:capnexa/services/feedback_service.dart';

class MockFeedbackService implements FeedbackService {
  @override
  Future<SupervisorFeedback> getLatestFeedback() async {
    await Future<void>.delayed(AppConstants.mockLatency);
    return const SupervisorFeedback(
      authorInitials: 'DR',
      authorName: 'Dr. Omar Rahman',
      date: 'May 20 · 9:42 AM',
      versionLabel: 'System Design · Version 2',
      message:
          'The overall architecture is clear and appropriately scoped. Before approval, please address the validation layer and clarify how the recommendation engine handles new users.',
      changes: [
        RequiredChange(title: 'Add input validation rules', location: 'Architecture document · Section 4.2', done: true),
        RequiredChange(title: 'Explain cold-start recommendations', location: 'Technical design · Page 18'),
        RequiredChange(title: 'Update data flow diagram', location: 'Show consent and privacy boundary'),
      ],
      homeExcerpt:
          'The user flow is clear. Please add data validation rules to the system architecture before resubmitting.',
      homeTimestamp: 'Today, 9:42 AM',
    );
  }

  @override
  Future<void> sendReply(String message) =>
      Future<void>.delayed(const Duration(milliseconds: 250));
}

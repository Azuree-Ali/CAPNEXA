import 'package:capnexa/models/feedback.dart';

/// Supervisor feedback contract.
/// TODO(api): `GET /submissions/{id}/feedback`, `POST /feedback/{id}/replies`.
abstract class FeedbackService {
  Future<SupervisorFeedback> getLatestFeedback();
  Future<void> sendReply(String message);
}

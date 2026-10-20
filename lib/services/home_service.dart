import 'package:capnexa/models/home_summary.dart';

/// Dashboard contract.
/// TODO(api): `GET /students/me/dashboard` (single aggregate call).
abstract class HomeService {
  Future<HomeSummary> getSummary();
}

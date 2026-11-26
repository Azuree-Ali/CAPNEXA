import 'package:capnexa/core/network/api_client.dart';
import 'package:capnexa/core/network/api_endpoints.dart';
import 'package:capnexa/models/home_summary.dart';
import 'package:capnexa/models/student.dart';
import 'package:capnexa/services/api/dashboard_mapper.dart';
import 'package:capnexa/services/auth_service.dart';
import 'package:capnexa/services/home_service.dart';

class ApiHomeService implements HomeService {
  ApiHomeService(this._api, this._auth);

  final ApiClient _api;
  final AuthService _auth;

  @override
  Future<HomeSummary> getSummary() async {
    final results = await Future.wait<dynamic>([
      _auth.currentStudent(),
      _api.get(ApiEndpoints.dashboard),
    ]);
    return DashboardMapper.summary(
      results[0] as Student,
      results[1] as Map<String, dynamic>,
    );
  }
}
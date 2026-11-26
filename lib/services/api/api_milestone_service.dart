import 'package:capnexa/core/network/api_client.dart';
import 'package:capnexa/core/network/api_endpoints.dart';
import 'package:capnexa/core/network/api_exception.dart';
import 'package:capnexa/models/milestone.dart';
import 'package:capnexa/services/api/dashboard_mapper.dart';
import 'package:capnexa/services/milestone_service.dart';

class ApiMilestoneService implements MilestoneService {
  ApiMilestoneService(this._api);

  final ApiClient _api;

  Future<Map<String, dynamic>> _dashboard() async =>
      await _api.get(ApiEndpoints.dashboard) as Map<String, dynamic>;

  @override
  Future<MilestonePlan> getPlan() async =>
      DashboardMapper.plan(await _dashboard());

  @override
  Future<MilestoneDetail> getCurrentMilestoneDetail() async {
    final detail = DashboardMapper.currentDetail(await _dashboard());
    if (detail == null) {
      throw const ApiException('There is no active milestone yet.');
    }
    return detail;
  }
}
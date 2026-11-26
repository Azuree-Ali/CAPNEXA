import 'package:capnexa/core/network/api_client.dart';
import 'package:capnexa/core/network/api_endpoints.dart';
import 'package:capnexa/core/network/api_exception.dart';
import 'package:capnexa/models/team.dart';
import 'package:capnexa/services/team_service.dart';

class ApiTeamService implements TeamService {
  ApiTeamService(this._api);

  final ApiClient _api;

  Team _team(dynamic data) => Team.fromJson(data as Map<String, dynamic>);

  @override
  Future<Team?> getTeam() async {
    try {
      return _team(await _api.get(ApiEndpoints.myTeam));
    } on ApiException catch (e) {
      if (e.code == 'team_not_found') return null;
      rethrow;
    }
  }

  @override
  Future<Team> createTeam(String name) async =>
      _team(await _api.post(ApiEndpoints.teams, body: {'name': name}));

  @override
  Future<Team> joinTeam(String inviteCode) async =>
      _team(await _api.post(ApiEndpoints.joinTeam, body: {'teamCode': inviteCode}));

  @override
  Future<void> leaveTeam() async {
    await _api.post(ApiEndpoints.leaveTeam);
  }
}
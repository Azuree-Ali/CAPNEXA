import 'package:capnexa/models/team.dart';

/// Team contract.
/// TODO(api): `GET /teams/me`, `POST /teams`, `POST /teams/join`, `DELETE /teams/me/membership`.
abstract class TeamService {
  Future<Team> getTeam();
  Future<Team> createTeam(String name);
  Future<Team> joinTeam(String inviteCode);
  Future<void> leaveTeam();
}

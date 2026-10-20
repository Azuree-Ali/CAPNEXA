import 'package:capnexa/core/constants/app_constants.dart';
import 'package:capnexa/models/team.dart';
import 'package:capnexa/services/team_service.dart';

class MockTeamService implements TeamService {
  static const _team = Team(
    name: 'Team 04',
    projectTitle: 'Campus Connect',
    inviteCode: 'GF-24X8Q',
    academicYear: '2025',
    status: 'Active',
    maxMembers: 5,
    members: [
      TeamMember(initials: 'LH', name: 'Lina Hassan', role: 'Team leader · Product', isLeader: true),
      TeamMember(initials: 'AK', name: 'Adam Khalil', role: 'Mobile development'),
      TeamMember(initials: 'SM', name: 'Sara Mansour', role: 'AI & data'),
      TeamMember(initials: 'YN', name: 'Yousef Nader', role: 'Backend development'),
    ],
  );

  @override
  Future<Team> getTeam() async {
    await Future<void>.delayed(AppConstants.mockLatency);
    return _team;
  }

  @override
  Future<Team> createTeam(String name) => getTeam();

  @override
  Future<Team> joinTeam(String inviteCode) => getTeam();

  @override
  Future<void> leaveTeam() async {}
}

class TeamMember {
  const TeamMember({
    required this.initials,
    required this.name,
    required this.role,
    this.isLeader = false,
  });

  final String initials;
  final String name;
  final String role;
  final bool isLeader;

  factory TeamMember.fromJson(Map<String, dynamic> json) {
    final first = (json['firstName'] ?? '').toString();
    final last = (json['lastName'] ?? '').toString();
    final leader = json['isLeader'] == true;
    final number = (json['studentNumber'] ?? '').toString();
    return TeamMember(
      initials:
          '${first.isEmpty ? '' : first[0]}${last.isEmpty ? '' : last[0]}'.toUpperCase(),
      name: '$first $last'.trim(),
      role: leader ? 'Team leader · ID $number' : 'Member · ID $number',
      isLeader: leader,
    );
  }
}

class Team {
  const Team({
    required this.name,
    required this.projectTitle,
    required this.inviteCode,
    required this.academicYear,
    required this.status,
    required this.maxMembers,
    required this.members,
  });

  final String name;
  final String projectTitle;
  final String inviteCode;
  final String academicYear;
  final String status;
  final int maxMembers;
  final List<TeamMember> members;


  factory Team.fromJson(Map<String, dynamic> json) => Team(
        name: json['name'] as String,
        projectTitle: '',
        inviteCode: json['teamCode'] as String,
        academicYear: '',
        status: 'Active',
        maxMembers: (json['maxMembers'] as num?)?.toInt() ?? 5,
        members: ((json['members'] as List?) ?? const [])
            .map((e) => TeamMember.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}

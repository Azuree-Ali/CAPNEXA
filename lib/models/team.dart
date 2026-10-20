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

  factory TeamMember.fromJson(Map<String, dynamic> json) => TeamMember(
        initials: json['initials'] as String,
        name: json['name'] as String,
        role: json['role'] as String,
        isLeader: (json['isLeader'] as bool?) ?? false,
      );
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
        projectTitle: json['projectTitle'] as String,
        inviteCode: json['inviteCode'] as String,
        academicYear: json['academicYear'] as String,
        status: json['status'] as String,
        maxMembers: json['maxMembers'] as int,
        members: (json['members'] as List)
            .map((e) => TeamMember.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}

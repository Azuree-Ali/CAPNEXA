class Supervisor {
  const Supervisor({
    required this.initials,
    required this.name,
    required this.title,
    required this.specialties,
    required this.email,
    required this.office,
    required this.officeHours,
    required this.assignmentDate,
    required this.group,
  });

  final String initials;
  final String name;
  final String title;
  final List<String> specialties;
  final String email;
  final String office;
  final String officeHours;
  final String assignmentDate;
  final String group;

  factory Supervisor.fromJson(Map<String, dynamic> json) => Supervisor(
        initials: json['initials'] as String,
        name: json['name'] as String,
        title: json['title'] as String,
        specialties: (json['specialties'] as List).cast<String>(),
        email: json['email'] as String,
        office: json['office'] as String,
        officeHours: json['officeHours'] as String,
        assignmentDate: json['assignmentDate'] as String,
        group: json['group'] as String,
      );
}

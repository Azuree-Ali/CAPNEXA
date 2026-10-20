class Student {
  const Student({
    required this.id,
    required this.fullName,
    required this.universityId,
    required this.email,
    required this.department,
    required this.academicYear,
  });

  final String id;
  final String fullName;
  final String universityId;
  final String email;
  final String department;
  final String academicYear;

  String get firstName => fullName.split(' ').first;

  String get initials {
    final parts = fullName.trim().split(RegExp(r'\s+'));
    if (parts.length == 1) return parts.first.substring(0, 1).toUpperCase();
    return (parts.first[0] + parts.last[0]).toUpperCase();
  }

  factory Student.fromJson(Map<String, dynamic> json) => Student(
        id: json['id'] as String,
        fullName: json['fullName'] as String,
        universityId: json['universityId'] as String,
        email: json['email'] as String,
        department: json['department'] as String,
        academicYear: json['academicYear'] as String,
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'fullName': fullName,
        'universityId': universityId,
        'email': email,
        'department': department,
        'academicYear': academicYear,
      };
}

class RegistrationData {
  const RegistrationData({
    required this.fullName,
    required this.universityId,
    required this.email,
    required this.department,
    required this.academicYear,
    required this.password,
  });

  final String fullName;
  final String universityId;
  final String email;
  final String department;
  final String academicYear;
  final String password;

  Map<String, dynamic> toJson() => {
        'fullName': fullName,
        'universityId': universityId,
        'email': email,
        'department': department,
        'academicYear': academicYear,
        'password': password,
      };
}

class Student {
  const Student({
    required this.id,
    required this.fullName,
    required this.universityId,
    required this.email,
    required this.department,
    required this.academicYear,
    this.academicProfileComplete = true,
  });

  final String id;
  final String fullName;
  final String universityId;
  final String email;
  final String department;
  final String academicYear;
  final bool academicProfileComplete;

  String get firstName => fullName.split(' ').first;

  String get initials {
    final parts = fullName.trim().split(RegExp(r'\s+'));
    if (parts.length == 1) return parts.first.substring(0, 1).toUpperCase();
    return (parts.first[0] + parts.last[0]).toUpperCase();
  }

  /// بيقرا رد GET /api/Student/Profile
  factory Student.fromJson(Map<String, dynamic> json) => Student(
        id: json['userId'].toString(),
        fullName: '${json['firstName'] ?? ''} ${json['lastName'] ?? ''}'.trim(),
        universityId: (json['studentNumber'] ?? '').toString(),
        email: (json['email'] ?? '').toString(),
        department: (json['department'] ?? '').toString(),
        academicYear: (json['academicYear'] ?? '').toString(),
        academicProfileComplete:
            json['academicProfileComplete'] as bool? ?? true,
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'fullName': fullName,
        'universityId': universityId,
        'email': email,
        'department': department,
        'academicYear': academicYear,
        'academicProfileComplete': academicProfileComplete,
      };
}

class RegistrationData {
  const RegistrationData({
    required this.firstName,
    required this.lastName,
    required this.universityId,
    required this.email,
    required this.department,
    required this.academicYear,
    required this.password,
    required this.confirmPassword,
  });

  final String firstName;
  final String lastName;
  final String universityId;
  final String email;
  final String department;
  final String academicYear;
  final String password;
  final String confirmPassword;

  Map<String, dynamic> toJson() => {
        'firstName': firstName,
        'lastName': lastName,
        'email': email,
        'studentNumber': universityId,
        'department': department,
        'academicYear': academicYear,
        'password': password,
        'confirmPassword': confirmPassword,
      };
}

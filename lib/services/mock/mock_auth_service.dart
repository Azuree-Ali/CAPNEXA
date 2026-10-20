import 'package:capnexa/core/constants/app_constants.dart';
import 'package:capnexa/models/student.dart';
import 'package:capnexa/services/auth_service.dart';

class MockAuthService implements AuthService {
  static const _student = Student(
    id: 'stu-001',
    fullName: 'Lina Hassan',
    universityId: '2021-04812',
    email: 'lina.hassan@northbridge.edu',
    department: 'Computer Science',
    academicYear: 'Year 4',
  );

  @override
  Future<Student> signIn({required String email, required String password}) async {
    await Future<void>.delayed(AppConstants.mockLatency);
    return _student;
  }

  @override
  Future<Student> signInWithProvider(SocialProvider provider) async {
    await Future<void>.delayed(const Duration(milliseconds: 700));
    return _student;
  }

  @override
  Future<Student> register(RegistrationData data) async {
    await Future<void>.delayed(AppConstants.mockLatency);
    return _student;
  }

  @override
  Future<Student> currentStudent() async {
    await Future<void>.delayed(AppConstants.mockLatency);
    return _student;
  }

  @override
  Future<void> signOut() async {}
}

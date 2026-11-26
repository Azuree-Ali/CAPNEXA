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
  Future<Student> completeProfile({
    required String studentNumber,
    required String department,
    required String academicYear,
    String? level,
  }) async {
    await Future<void>.delayed(AppConstants.mockLatency);
    return _student;
  }

  @override
  Future<Student> signIn(
      {required String email, required String password}) async {
    await Future<void>.delayed(AppConstants.mockLatency);
    return _student;
  }

  @override
  Future<Student> signInWithProvider(SocialProvider provider) async {
    await Future<void>.delayed(const Duration(milliseconds: 700));
    return _student;
  }

  @override
  Future<RegistrationResult> register(RegistrationData data) async {
    await Future<void>.delayed(AppConstants.mockLatency);
    return const RegistrationResult(
      message: 'Account created. Check your email to confirm it.',
      emailConfirmationSent: true,
    );
  }

  @override
  Future<void> resendConfirmation(String email) async {
    await Future<void>.delayed(AppConstants.mockLatency);
  }

  @override
  Future<Student> currentStudent() async {
    await Future<void>.delayed(AppConstants.mockLatency);
    return _student;
  }

    @override
  Future<void> requestPasswordReset(String email) async {
    await Future<void>.delayed(AppConstants.mockLatency);
  }

  @override
  Future<void> resetPassword({
    required String email,
    required String otp,
    required String newPassword,
    required String confirmPassword,
  }) async {
    await Future<void>.delayed(AppConstants.mockLatency);
  }

  @override
  Future<void> signOut() async {}

  @override
  Future<Student?> restoreSession() async => null;

  @override
  Future<Student> updateProfile({required String firstName, required String lastName}) async {
    await Future<void>.delayed(AppConstants.mockLatency);
    return _student;
  }

  @override
  Future<void> changePassword({
    String? currentPassword,
    required String newPassword,
    required String confirmPassword,
  }) async {
    await Future<void>.delayed(AppConstants.mockLatency);
  }

  @override
  Future<void> requestEmailChange(String newEmail) async {
    await Future<void>.delayed(AppConstants.mockLatency);
  }
}

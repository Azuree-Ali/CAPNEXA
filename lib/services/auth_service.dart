import 'package:capnexa/models/student.dart';

enum SocialProvider { google, github }

class RegistrationResult {
  const RegistrationResult({
    required this.message,
    required this.emailConfirmationSent,
  });

  final String message;
  final bool emailConfirmationSent;
}

/// Authentication contract.
abstract class AuthService {
  Future<Student> signIn({required String email, required String password});
  Future<Student> signInWithProvider(SocialProvider provider);
  Future<RegistrationResult> register(RegistrationData data);
  Future<void> resendConfirmation(String email);
  Future<Student> currentStudent();
  Future<void> signOut();
  Future<Student> completeProfile({
    required String studentNumber,
    required String department,
    required String academicYear,
    String? level,
  });
  Future<void> requestPasswordReset(String email);
  Future<void> resetPassword({
    required String email,
    required String otp,
    required String newPassword,
    required String confirmPassword,
  });
}

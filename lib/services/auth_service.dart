import 'package:capnexa/models/student.dart';

enum SocialProvider { google, github }

/// Authentication contract.
/// TODO(api): implement with `POST /auth/login`, `/auth/register`,
/// `/auth/external/{provider}` against the ASP.NET Core Web API.
abstract class AuthService {
  Future<Student> signIn({required String email, required String password});
  Future<Student> signInWithProvider(SocialProvider provider);
  Future<Student> register(RegistrationData data);
  Future<Student> currentStudent();
  Future<void> signOut();
}

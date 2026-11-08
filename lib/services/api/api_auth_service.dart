import 'package:capnexa/core/network/api_client.dart';
import 'package:capnexa/core/network/api_endpoints.dart';
import 'package:capnexa/core/network/api_exception.dart';
import 'package:capnexa/models/student.dart';
import 'package:capnexa/services/auth_service.dart';

class ApiAuthService implements AuthService {
  ApiAuthService(this._api);

  final ApiClient _api;

  @override
  Future<Student> signIn({
    required String email,
    required String password,
  }) async {
    final data = await _api.post(
      ApiEndpoints.login,
      body: {'email': email, 'password': password},
    );
    await _saveToken(data);
    return currentStudent();
  }

  @override
  Future<Student> register(RegistrationData data) async {
    await _api.post(ApiEndpoints.register, body: data.toJson());
    // لو الباك اند بيرجع token عند التسجيل نقدر نختصر الخطوة دي بعدين
    return signIn(email: data.email, password: data.password);
  }

  @override
  Future<Student> currentStudent() async {
    final data = await _api.get(ApiEndpoints.me);
    return Student.fromJson(data as Map<String, dynamic>);
  }

  @override
  Future<Student> signInWithProvider(SocialProvider provider) async {
    // TODO: لحد ما نعرف الباك اند بيدعم Google/GitHub ولا لأ
    throw const ApiException('This sign-in method is not available yet.');
  }

  @override
  Future<void> signOut() => _api.tokenStorage.clear();

  Future<void> _saveToken(dynamic data) async {
    // TODO: اسم الحقل حسب الباك اند (token / accessToken ...)
    final token = (data is Map ? (data['token'] ?? data['accessToken']) : null)
        ?.toString();
    if (token == null || token.isEmpty) {
      throw const ApiException('Unexpected response from the server.');
    }
    await _api.tokenStorage.saveToken(token);
  }
}
import 'dart:convert';

import 'package:flutter/services.dart';
import 'package:flutter_web_auth_2/flutter_web_auth_2.dart';

import 'package:capnexa/core/constants/app_constants.dart';
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
    await _saveTokens(data);
    try {
      return await currentStudent();
    } catch (_) {
      // لو الـ profile فشل (مثلًا الحساب مش Student) منسيبش tokens متخزنة
      await _api.tokenStorage.clear();
      rethrow;
    }
  }

  @override
  Future<void> requestPasswordReset(String email) async {
    // الباك اند بيرجع 200 دايمًا (بدون ما يكشف لو الإيميل موجود)
    await _api.post(ApiEndpoints.forgotPassword, body: {'email': email});
  }

  @override
  Future<void> resetPassword({
    required String email,
    required String otp,
    required String newPassword,
    required String confirmPassword,
  }) async {
    try {
      await _api.post(ApiEndpoints.resetPassword, body: {
        'email': email,
        'otp': otp,
        'newPassword': newPassword,
        'confirmPassword': confirmPassword,
      });
    } on ApiException catch (e) {
      // الباك اند بيرجع الكود الخام password_policy_failed كرسالة
      if (e.message == 'password_policy_failed') {
        throw ApiException(
          'Password does not meet the requirements. Use 8+ characters with a number and a symbol.',
          statusCode: e.statusCode,
        );
      }
      rethrow;
    }
  }


  @override
  Future<RegistrationResult> register(RegistrationData data) async {
    final res = await _api.post(ApiEndpoints.register, body: data.toJson());
    final map = res is Map ? res : const {};
    return RegistrationResult(
      message: (map['message'] ?? 'Account created.').toString(),
      emailConfirmationSent: map['emailConfirmationSent'] == true,
    );
  }

  @override
  Future<void> resendConfirmation(String email) async {
    await _api.post(ApiEndpoints.resendConfirmation, body: {'email': email});
  }

  @override
  Future<Student> currentStudent() async {
    final data = await _api.get(ApiEndpoints.profile);
    return Student.fromJson(data as Map<String, dynamic>);
  }

  @override
  Future<Student> signInWithProvider(SocialProvider provider) async {
    final path = switch (provider) {
      SocialProvider.google => ApiEndpoints.googleLogin,
      SocialProvider.github => ApiEndpoints.githubLogin,
    };

    final String result;
    try {
      result = await FlutterWebAuth2.authenticate(
        url: '${AppConstants.apiBaseUrl}$path',
        callbackUrlScheme: AppConstants.oauthCallbackScheme,
      );
    } on PlatformException catch (e) {
      if (e.code == 'CANCELED') {
        throw const ApiException('Sign-in was cancelled.');
      }
      throw const ApiException('Could not open the sign-in page.');
    }

    final uri = Uri.parse(result);

    // حالة الخطأ: ?error=<code>
    final error = uri.queryParameters['error'];
    if (error != null) {
      throw ApiException(_oauthErrorMessage(error), code: error);
    }

    // حالة النجاح: #auth=<base64url-json>  (مايتطبعش في أي log)
    final fragment = uri.fragment;
    if (!fragment.startsWith('auth=')) {
      throw const ApiException('Unexpected response from the server.');
    }
    final Object? decoded;
    try {
      final encoded = fragment.substring('auth='.length);
      decoded = jsonDecode(utf8.decode(base64Url.decode(base64Url.normalize(encoded))));
    } catch (_) {
      throw const ApiException('Unexpected response from the server.');
    }

    await _saveTokens(decoded);
    try {
      return await currentStudent();
    } catch (_) {
      await _api.tokenStorage.clear();
      rethrow;
    }
  }

  String _oauthErrorMessage(String code) => switch (code) {
        'email_conflict' =>
          'This email already has a password account. Sign in with your password.',
        'email_unverified' || 'email_required' =>
          'Your provider did not share a verified email address.',
        'github_profile_failed' => 'Could not read your GitHub profile.',
        'account_disabled' => 'This account is disabled.',
        _ => 'Sign-in failed. Please try again.',
      };

  @override
  Future<Student> completeProfile({
    required String studentNumber,
    required String department,
    required String academicYear,
    String? level,
  }) async {
    final data = await _api.post(
      ApiEndpoints.completeProfile,
      body: {
        'studentNumber': studentNumber,
        'department': department,
        'academicYear': academicYear,
        if (level != null && level.isNotEmpty) 'level': level,
      },
    );
    return Student.fromJson(data as Map<String, dynamic>);
  }

  @override
  Future<void> signOut() => _api.tokenStorage.clear();

  Future<void> _saveTokens(dynamic data) async {
    final map = data is Map ? data : const {};
    final access = map['accessToken']?.toString();
    final refresh = map['refreshToken']?.toString();
    if (access == null ||
        access.isEmpty ||
        refresh == null ||
        refresh.isEmpty) {
      throw const ApiException('Unexpected response from the server.');
    }
    await _api.tokenStorage
        .saveTokens(accessToken: access, refreshToken: refresh);
  }
}

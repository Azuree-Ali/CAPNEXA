class ApiEndpoints {
  ApiEndpoints._();

  static const register = '/api/Identity/Auth/register';
  static const resendConfirmation = '/api/Identity/Auth/resend-confirmation';
  static const login = '/api/Identity/Auth/login';
  static const refresh = '/api/Identity/Auth/refresh';
  static const profile = '/api/Student/Profile';
  static const googleLogin = '/api/Identity/Auth/google/login';
  static const githubLogin = '/api/Identity/Auth/github/login';
  static const completeProfile = '/api/Student/Profile/complete';
  static const forgotPassword = '/api/Identity/Auth/forgot-password';
  static const resetPassword = '/api/Identity/Auth/reset-password';
}

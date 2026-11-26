class ApiEndpoints {
  ApiEndpoints._();

  // Auth
  static const register = '/api/Identity/Auth/register';
  static const resendConfirmation = '/api/Identity/Auth/resend-confirmation';
  static const login = '/api/Identity/Auth/login';
  static const refresh = '/api/Identity/Auth/refresh';
  static const forgotPassword = '/api/Identity/Auth/forgot-password';
  static const resetPassword = '/api/Identity/Auth/reset-password';
  static const googleLogin = '/api/Identity/Auth/google/login';
  static const githubLogin = '/api/Identity/Auth/github/login';

  // Profile
  static const profile = '/api/Student/Profile';
  static const completeProfile = '/api/Student/Profile/complete';
  static const changePassword = '/api/Student/Profile/password';
  static const emailChange = '/api/Student/Profile/email-change';

  // Teams
  static const teams = '/api/Student/Teams';
  static const myTeam = '/api/Student/Teams/me';
  static const joinTeam = '/api/Student/Teams/join';
  static const leaveTeam = '/api/Student/Teams/leave';

  // Proposals
  static const proposals = '/api/Student/ProjectProposals';
  static String proposal(int id) => '$proposals/$id';
  static String submitProposal(int id) => '$proposals/$id/submit';

  // Dashboard
  static const dashboard = '/api/Student/Dashboard';
}

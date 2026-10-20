/// Route names. One constant per screen.
class Routes {
  Routes._();

  static const splash = '/';
  static const onboarding = '/onboarding';
  static const register = '/register';
  static const login = '/login';
  static const home = '/home';
  static const teamChoice = '/team-choice';
  static const team = '/team';
  static const proposal = '/proposal';
  static const approval = '/approval';
  static const supervisor = '/supervisor';
  static const project = '/project';
  static const milestones = '/milestones';
  static const milestone = '/milestone';
  static const submission = '/submission';
  static const analysis = '/analysis';
  static const similarity = '/similarity';
  static const feedback = '/feedback';
  static const history = '/history';
  static const notifications = '/notifications';
  static const profile = '/profile';
  static const settings = '/settings';
  static const completion = '/completion';
  static const states = '/states';

  /// Screens shown without the bottom navigation bar.
  static const standalone = {splash, onboarding, register, login};
}

/// Entry of the "All screens" journey drawer (same list/groups as the
/// Figma prototype's left panel, plus the screens added from the brief).
class JourneyItem {
  const JourneyItem(this.route, this.label, this.group);

  final String route;
  final String label;
  final String group;
}

const journeyItems = <JourneyItem>[
  JourneyItem(Routes.splash, 'Splash', 'Access'),
  JourneyItem(Routes.onboarding, 'Onboarding', 'Access'),
  JourneyItem(Routes.register, 'Registration', 'Access'),
  JourneyItem(Routes.login, 'Login', 'Access'),
  JourneyItem(Routes.home, 'Home dashboard', 'Project setup'),
  JourneyItem(Routes.teamChoice, 'Create / join team', 'Project setup'),
  JourneyItem(Routes.team, 'Team', 'Project setup'),
  JourneyItem(Routes.proposal, 'Project proposal', 'Project setup'),
  JourneyItem(Routes.approval, 'Approval status', 'Project setup'),
  JourneyItem(Routes.supervisor, 'Supervisor assignment', 'Project setup'),
  JourneyItem(Routes.project, 'Project workspace', 'Delivery'),
  JourneyItem(Routes.milestones, 'Milestones', 'Delivery'),
  JourneyItem(Routes.milestone, 'Milestone details', 'Delivery'),
  JourneyItem(Routes.submission, 'Upload submission', 'Delivery'),
  JourneyItem(Routes.analysis, 'Security scan & AI analysis', 'Review'),
  JourneyItem(Routes.similarity, 'AI similarity', 'Review'),
  JourneyItem(Routes.feedback, 'Supervisor feedback', 'Review'),
  JourneyItem(Routes.history, 'Submission history', 'Review'),
  JourneyItem(Routes.notifications, 'Notifications', 'Account'),
  JourneyItem(Routes.profile, 'Profile', 'Account'),
  JourneyItem(Routes.settings, 'Settings', 'Account'),
  JourneyItem(Routes.completion, 'Project completion', 'Account'),
  JourneyItem(Routes.states, 'System states', 'Account'),
];

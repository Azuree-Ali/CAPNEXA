import 'package:capnexa/core/constants/app_constants.dart';
import 'package:capnexa/models/milestone.dart';
import 'package:capnexa/services/milestone_service.dart';

class MockMilestoneService implements MilestoneService {
  static const _systemDesign = Milestone(
      id: 'm3',
      name: 'System Design',
      dateRange: 'May 19 – May 24',
      progress: 70,
      status: MilestoneStatus.inProgress,
      daysLeft: 4);

  static const _milestones = [
    Milestone(id: 'm1', name: 'Proposal', dateRange: 'May 1 – May 12', progress: 100, status: MilestoneStatus.completed),
    Milestone(id: 'm2', name: 'Analysis', dateRange: 'May 13 – May 18', progress: 100, status: MilestoneStatus.completed),
    _systemDesign,
    Milestone(id: 'm4', name: 'Implementation', dateRange: 'May 25 – Jun 20', progress: 0, status: MilestoneStatus.upcoming),
    Milestone(id: 'm5', name: 'Testing', dateRange: 'Jun 21 – Jul 4', progress: 0, status: MilestoneStatus.upcoming),
    Milestone(id: 'm6', name: 'Final Report', dateRange: 'Jul 5 – Jul 18', progress: 0, status: MilestoneStatus.upcoming),
  ];

  @override
  Future<MilestonePlan> getPlan() async {
    await Future<void>.delayed(AppConstants.mockLatency);
    return const MilestonePlan(
      overallProgress: 62,
      finalSubmission: 'July 18, 2025',
      milestones: _milestones,
    );
  }

  @override
  Future<MilestoneDetail> getCurrentMilestoneDetail() async {
    await Future<void>.delayed(AppConstants.mockLatency);
    return const MilestoneDetail(
      milestone: _systemDesign,
      number: 3,
      total: 6,
      day: '24',
      month: 'MAY',
      deadlineLabel: 'Friday, 11:59 PM',
      remainingLabel: '4 days remaining',
      about:
          'Translate approved requirements into a complete technical blueprint covering architecture, data, interfaces, and interaction flows.',
      requirements: [
        'System architecture diagram',
        'Database schema and data dictionary',
        'Interface wireframes',
        'API and validation specification',
      ],
      supervisorInstructions:
          'Include validation rules in the architecture document. Use the department template for all diagrams.',
    );
  }
}

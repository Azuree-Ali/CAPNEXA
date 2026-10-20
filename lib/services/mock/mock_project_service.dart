import 'package:capnexa/core/constants/app_constants.dart';
import 'package:capnexa/models/project.dart';
import 'package:capnexa/models/proposal.dart';
import 'package:capnexa/models/supervisor.dart';
import 'package:capnexa/services/project_service.dart';

class MockProjectService implements ProjectService {
  Future<T> _wait<T>(T value) async {
    await Future<void>.delayed(AppConstants.mockLatency);
    return value;
  }

  @override
  Future<Project> getCurrentProject() => _wait(const Project(
        id: 'prj-084',
        title: 'Campus Connect',
        tagline: 'AI-powered campus service discovery platform',
        status: ProjectStatus.inProgress,
        progress: 62,
        currentMilestone: 'System Design',
        milestoneNumber: 3,
        milestoneCount: 6,
        supervisorName: 'Dr. Omar Rahman',
        teamSize: 4,
        finalDeadline: 'July 18, 2025',
        submissionCount: 6,
        dueLabel: 'Due May 24',
        daysLeft: 4,
      ));

  @override
  Future<Supervisor> getSupervisor() => _wait(const Supervisor(
        initials: 'DR',
        name: 'Dr. Omar Rahman',
        title: 'Associate Professor · Software Engineering',
        specialties: ['Human–Computer Interaction', 'AI Systems'],
        email: 'o.rahman@northbridge.edu',
        office: 'Engineering Building · Room 412',
        officeHours: 'Sun & Tue · 1:00–3:00 PM',
        assignmentDate: 'May 15, 2025',
        group: 'Group SE-04',
      ));

  @override
  Future<ProposalDraft> getProposalDraft() => _wait(const ProposalDraft(
        title: 'Campus Connect: Intelligent Service Discovery',
        description:
            'A mobile platform that helps students discover university services, events, and academic resources through personalized recommendations.',
        problemStatement:
            'Students often struggle to find accurate, relevant campus information across fragmented university channels.',
        proposedSolution:
            'A unified mobile experience powered by semantic search and a personalized recommendation engine.',
        technologies: 'Flutter, Supabase, Python, FastAPI',
        category: 'Artificial Intelligence & Mobile',
        lastSaved: 'Last saved 2 minutes ago',
        memberInitials: ['LH', 'AK', 'SM', 'YN'],
      ));

  @override
  Future<void> saveProposalDraft(ProposalDraft draft) =>
      Future<void>.delayed(const Duration(milliseconds: 200));

  @override
  Future<void> submitProposal(ProposalDraft draft) =>
      Future<void>.delayed(const Duration(milliseconds: 200));

  @override
  Future<ProposalStatusInfo> getProposalStatus() => _wait(const ProposalStatusInfo(
        status: ProposalStatus.approved,
        projectTitle: 'Campus Connect',
        proposalId: 'PRJ-2025-084',
        reviewer: 'CS Department',
        rejectionReason:
            'The scope is too broad for one semester. Please narrow the problem statement and resubmit.',
        steps: [
          ReviewStep(title: 'Proposal submitted', subtitle: 'May 8 · 10:24 AM', state: ReviewState.complete),
          ReviewStep(title: 'Administrative review', subtitle: 'Reviewed by CS Department', state: ReviewState.complete),
          ReviewStep(title: 'Proposal approved', subtitle: 'May 12 · 2:15 PM', state: ReviewState.current),
          ReviewStep(title: 'Supervisor assignment', subtitle: 'Expected within 2–3 working days', state: ReviewState.pending),
        ],
      ));
}

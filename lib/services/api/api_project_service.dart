import 'package:capnexa/core/network/api_client.dart';
import 'package:capnexa/core/network/api_endpoints.dart';
import 'package:capnexa/core/network/api_exception.dart';
import 'package:capnexa/core/utils/date_format.dart';
import 'package:capnexa/models/project.dart';
import 'package:capnexa/models/proposal.dart';
import 'package:capnexa/models/supervisor.dart';
import 'package:capnexa/models/team.dart';
import 'package:capnexa/services/api/dashboard_mapper.dart';
import 'package:capnexa/services/project_service.dart';

typedef _Json = Map<String, dynamic>;

class ApiProjectService implements ProjectService {
  ApiProjectService(this._api);

  final ApiClient _api;

  Future<List<_Json>> _proposals() async {
    final data = await _api.get(ApiEndpoints.proposals);
    return List<_Json>.from((data as List?) ?? const []);
  }

  Future<_Json> _dashboard() async =>
      await _api.get(ApiEndpoints.dashboard) as _Json;

  ProposalDraft _draftOf(_Json p, List<String> initials) {
    final created = DateTime.tryParse('${p['createdAt']}')?.toLocal();
    return ProposalDraft(
      title: (p['title'] ?? '').toString(),
      description: (p['description'] ?? '').toString(),
      lastSaved: created == null ? '' : 'Created ${formatDate(created)}',
      memberInitials: initials,
      projectId: p['projectId'] as int?,
      status: (p['status'] ?? 'Draft').toString(),
    );
  }

  // ---------- Proposals ----------

  @override
  Future<ProposalDraft> getProposalDraft() async {
    final results = await Future.wait<dynamic>([
      _proposals(),
      _api.get(ApiEndpoints.myTeam),
    ]);
    final list = results[0] as List<_Json>;
    final team = results[1] as _Json;
    final initials = [
      for (final m in List<_Json>.from((team['members'] as List?) ?? const []))
        TeamMember.fromJson(m).initials,
    ];

    // الأولوية: draft موجود ← proposal مقدّم/معتمد (مينفعش يتعدل) ← proposal جديد
    final drafts = list.where((p) => p['status'] == 'Draft');
    if (drafts.isNotEmpty) return _draftOf(drafts.first, initials);
    if (list.isNotEmpty &&
        (list.first['status'] == 'PendingReview' || list.first['status'] == 'Approved')) {
      return _draftOf(list.first, initials);
    }
    return ProposalDraft(
      title: '',
      description: '',
      lastSaved: 'New proposal',
      memberInitials: initials,
    );
  }

  @override
  Future<ProposalDraft> saveProposalDraft(ProposalDraft draft) async {
    final body = {
      'title': draft.title.trim(),
      'description': draft.description.trim(),
    };
    final data = draft.projectId == null
        ? await _api.post(ApiEndpoints.proposals, body: body)
        : await _api.put(ApiEndpoints.proposal(draft.projectId!), body: body);
    return _draftOf(data as _Json, draft.memberInitials);
  }

  @override
  Future<void> submitProposal(ProposalDraft draft) async {
    final saved = await saveProposalDraft(draft);
    await _api.post(ApiEndpoints.submitProposal(saved.projectId!));
  }

  @override
  Future<ProposalStatusInfo> getProposalStatus() async {
    final list = await _proposals();
    if (list.isEmpty) {
      throw const ApiException('You haven’t created a proposal yet.');
    }
    final p = list.first; // الأحدث أولًا
    final status = switch (p['status']) {
      'PendingReview' => ProposalStatus.pending,
      'Approved' => ProposalStatus.approved,
      'Rejected' => ProposalStatus.rejected,
      _ => ProposalStatus.draft,
    };
    final submitted = DateTime.tryParse('${p['submittedAt']}')?.toLocal();
    final states = switch (status) {
      ProposalStatus.draft => [
          ReviewState.current, ReviewState.pending, ReviewState.pending, ReviewState.pending
        ],
      ProposalStatus.pending => [
          ReviewState.complete, ReviewState.current, ReviewState.pending, ReviewState.pending
        ],
      ProposalStatus.approved => [
          ReviewState.complete, ReviewState.complete, ReviewState.complete, ReviewState.current
        ],
      ProposalStatus.rejected => [
          ReviewState.complete, ReviewState.complete, ReviewState.current, ReviewState.pending
        ],
    };
    return ProposalStatusInfo(
      status: status,
      projectTitle: (p['title'] ?? '').toString(),
      proposalId: 'PRJ-${p['projectId']}',
      reviewer: 'Department review',
      rejectionReason: p['rejectionReason'] as String?,
      steps: [
        ReviewStep(
          title: 'Proposal submitted',
          subtitle: submitted == null ? 'Not submitted yet' : formatDate(submitted),
          state: states[0],
        ),
        ReviewStep(
          title: 'Administrative review',
          subtitle: 'Reviewed by the department',
          state: states[1],
        ),
        ReviewStep(
          title: status == ProposalStatus.rejected ? 'Changes requested' : 'Proposal approved',
          subtitle: status == ProposalStatus.rejected
              ? 'See the reason above'
              : 'Decision from the department',
          state: states[2],
        ),
        ReviewStep(
          title: 'Supervisor assignment',
          subtitle: 'Follows approval',
          state: states[3],
        ),
      ],
    );
  }

  // ---------- من الـ Dashboard ----------

  @override
  Future<Project> getCurrentProject() async {
    final project = DashboardMapper.project(await _dashboard());
    if (project == null) {
      throw const ApiException('Your project appears here once your proposal is approved.');
    }
    return project;
  }

  /// الباك اند بيرجّع اسم المشرف بس (لسه مفيش endpoint لتفاصيله).
  @override
  Future<Supervisor> getSupervisor() async {
    final d = await _dashboard();
    final name = (d['project'] as _Json?)?['supervisorName'] as String?;
    if (name == null || name.isEmpty) {
      throw const ApiException('A supervisor hasn’t been assigned yet.');
    }
    final initials = name
        .replaceAll('Dr.', '')
        .split(RegExp(r'\s+'))
        .where((w) => w.isNotEmpty)
        .take(2)
        .map((w) => w[0])
        .join()
        .toUpperCase();
    return Supervisor(
      initials: initials,
      name: name,
      title: 'Project supervisor',
      specialties: const [],
      email: '',
      office: '',
      officeHours: '',
      assignmentDate: '',
      group: '',
    );
  }
}
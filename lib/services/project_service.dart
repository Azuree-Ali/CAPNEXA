import 'package:capnexa/models/project.dart';
import 'package:capnexa/models/proposal.dart';
import 'package:capnexa/models/supervisor.dart';

/// Project / proposal / supervisor contract.
/// TODO(api): `GET /projects/current`, `GET/PUT /proposals/draft`,
/// `POST /proposals/submit`, `GET /proposals/status`, `GET /projects/current/supervisor`.
abstract class ProjectService {
  Future<Project> getCurrentProject();
  Future<Supervisor> getSupervisor();
  Future<ProposalDraft> getProposalDraft();
  Future<void> saveProposalDraft(ProposalDraft draft);
  Future<void> submitProposal(ProposalDraft draft);
  Future<ProposalStatusInfo> getProposalStatus();
}

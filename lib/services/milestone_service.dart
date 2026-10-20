import 'package:capnexa/models/milestone.dart';

/// Milestone contract.
/// TODO(api): `GET /projects/current/milestones`, `GET /milestones/{id}`.
abstract class MilestoneService {
  Future<MilestonePlan> getPlan();
  Future<MilestoneDetail> getCurrentMilestoneDetail();
}

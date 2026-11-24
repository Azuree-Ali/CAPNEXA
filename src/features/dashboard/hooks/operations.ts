import {
  listStudents,
  studentProfile,
  setStudentStatus,
} from "@/features/students/api/students.api";
import {
  listSupervisors,
  supervisorDetails,
  setCapacity,
} from "@/features/supervisors/api/supervisors.api";
import {
  listProposals,
  proposalDetails,
  reviewProposal,
} from "@/features/proposals/api/proposals.api";
import { listUnassigned, assignSupervisor } from "@/features/assignments/api/assignments.api";
import { operations } from "../content/operations.content";
import type {
  OperationDomain,
  OperationFilters,
  Change,
  Detail,
  Listing,
  Entry,
  RecordField,
} from "../types/operations.types";
const t = operations.text;

const dateFormat = new Intl.DateTimeFormat("en", { dateStyle: "medium", timeZone: "UTC" });
function dateLabel(value: string | null) {
  if (!value) return t.missing;
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? value : dateFormat.format(date);
}

function proposalStatus(value: string) {
  if (value === "PendingReview" || value === "Approved" || value === "Rejected") {
    return operations.proposalStatuses[value];
  }
  return value;
}

export async function loadList(
  domain: OperationDomain,
  search: string,
  page: number,
  signal: AbortSignal,
  filters: OperationFilters = {},
): Promise<Listing> {
  const query = { search, page, pageSize: 20 };
  switch (domain) {
    case "students": {
      // [BACKEND-API] List students. GET /api/Admin/Students. AdminStudentQueryDto -> AdminStudentListDto.
      const result = await listStudents(
        {
          ...query,
          ...(filters.isActive !== undefined ? { isActive: filters.isActive } : {}),
        },
        signal,
      );
      return {
        ...result,
        items: result.items.map((s) => ({
          id: s.userId,
          title: s.firstName + " " + s.lastName,
          cells: [
            s.studentNumber ?? t.missing,
            s.department ?? t.missing,
            s.academicYear ?? t.missing,
            s.isActive ? t.active : t.inactive,
          ],
        })),
      };
    }
    case "supervisors": {
      // [BACKEND-API] List faculty. GET /api/Admin/Supervisors. AdminSupervisorQueryDto -> AdminSupervisorListDto.
      const result = await listSupervisors(
        {
          ...query,
          ...(filters.isActive !== undefined ? { isActive: filters.isActive } : {}),
          ...(filters.hasCapacity !== undefined ? { hasCapacity: filters.hasCapacity } : {}),
        },
        signal,
      );
      return {
        ...result,
        items: result.items.map((s) => ({
          id: String(s.supervisorId),
          title: s.firstName + " " + s.lastName,
          cells: [
            s.department ?? t.missing,
            s.specialization ?? t.missing,
            `${s.assignedProjectsCount} / ${s.maxProjects}`,
            String(s.availableCapacity),
            s.isActive ? t.active : t.inactive,
          ],
        })),
      };
    }
    case "proposals": {
      // [BACKEND-API] Review queue. GET /api/Admin/ProjectProposals. AdminProposalQueryDto -> AdminProposalListDto.
      const result = await listProposals(
        { ...query, status: filters.status ?? "PendingReview" },
        signal,
      );
      return {
        ...result,
        items: result.items.map((s) => ({
          id: String(s.projectId),
          title: s.title,
          cells: [
            s.team.name,
            s.leaderName || t.missing,
            dateLabel(s.submittedAt),
            proposalStatus(s.status),
          ],
        })),
      };
    }
    case "assignments": {
      // [BACKEND-API] Allocation queue. GET /api/Admin/ProjectAssignments/unassigned. AdminUnassignedProjectsQueryDto -> AdminUnassignedProjectListDto.
      const result = await listUnassigned(query, signal);
      return {
        ...result,
        items: result.items.map((s) => ({
          id: String(s.projectId),
          title: s.title,
          cells: [s.team.name, s.leaderName || t.missing, dateLabel(s.reviewedAt)],
          fields: [
            [t.team, s.team.name],
            [t.teamCode, s.team.teamCode],
            [t.members, String(s.team.memberCount)],
            [t.leader, s.leaderName || t.missing],
            [t.reviewed, dateLabel(s.reviewedAt)],
          ],
        })),
      };
    }
  }
}

export async function loadDetail(
  domain: OperationDomain,
  row: Entry,
  signal: AbortSignal,
): Promise<Detail> {
  switch (domain) {
    case "students": {
      // [BACKEND-API] Inspect student. GET /api/Admin/Students/{userId}/profile. userId -> StudentProfileDto.
      const s = await studentProfile(row.id, signal);
      return {
        title: s.firstName + " " + s.lastName,
        fields: [
          [t.account, s.isActive ? t.active : t.inactive],
          [t.email, s.email ?? t.missing],
          [t.emailStatus, s.emailConfirmed ? t.confirmed : t.unconfirmed],
          [t.number, s.studentNumber ?? t.missing],
          [t.department, s.department ?? t.missing],
          [t.year, s.academicYear ?? t.missing],
          [t.level, s.level ?? t.missing],
          [t.profileStatus, s.academicProfileComplete ? t.complete : t.incomplete],
        ],
        sections: [
          {
            title: t.teamMembership,
            empty: t.noTeams,
            records: s.teams.map((team) => ({
              id: String(team.teamId),
              title: team.name,
              fields: [
                [t.teamCode, team.teamCode],
                [t.role, team.isLeader ? t.leader : team.isCreator ? t.creator : t.member],
                [t.joined, dateLabel(team.joinedAt)],
                ...team.projects.map((project): RecordField => [
                  project.title,
                  proposalStatus(project.status),
                ]),
              ],
            })),
          },
        ],
        actions: [s.isActive ? "deactivate" : "activate"],
      };
    }
    case "supervisors": {
      // [BACKEND-API] Inspect faculty load. GET /api/Admin/Supervisors/{supervisorId}. supervisorId -> AdminSupervisorDetailDto.
      const s = await supervisorDetails(Number(row.id), signal);
      return {
        title: s.firstName + " " + s.lastName,
        fields: [
          [t.supervisor, row.id],
          [t.account, s.isActive ? t.active : t.inactive],
          [t.email, s.email ?? t.missing],
          [t.department, s.department ?? t.missing],
          [t.specialization, s.specialization ?? t.missing],
          [t.research, s.researchInterests ?? t.missing],
          [t.assigned, String(s.assignedProjectsCount)],
          [t.capacity, String(s.maxProjects)],
          [t.available, String(s.availableCapacity)],
        ],
        actions: ["capacity"],
        capacity: { assigned: s.assignedProjectsCount, maximum: s.maxProjects },
        sections: [
          {
            title: t.assignedProjects,
            empty: t.noProjects,
            records: s.assignedProjects.map((project) => ({
              id: String(project.projectId),
              title: project.title,
              fields: [
                [t.team, project.teamName],
                [t.teamCode, project.teamCode],
                [t.status, proposalStatus(project.status)],
              ],
              ...(project.status === "Approved"
                ? {
                    action: {
                      change: "assign" as const,
                      targetId: String(project.projectId),
                      targetTitle: project.title,
                      label: t.reassign,
                    },
                  }
                : {}),
            })),
          },
        ],
      };
    }
    case "proposals": {
      // [BACKEND-API] Read proposal. GET /api/Admin/ProjectProposals/{projectId}. projectId -> AdminProposalDetailDto.
      const s = await proposalDetails(Number(row.id), signal);
      return {
        title: s.title,
        fields: [
          [t.project, String(s.projectId)],
          [t.team, s.team.name],
          [t.teamCode, s.team.teamCode],
          [t.submitted, dateLabel(s.submittedAt)],
          [t.status, proposalStatus(s.status)],
          [t.description, s.description ?? t.missing],
        ],
        sections: [
          {
            title: t.members,
            empty: t.noMembers,
            records: s.team.members.map((member) => ({
              id: member.userId,
              title:
                member.firstName +
                " " +
                member.lastName +
                (member.isLeader ? " (" + t.leader + ")" : ""),
              fields: [
                [t.number, member.studentNumber],
                [t.department, member.department ?? t.missing],
                [t.year, member.academicYear ?? t.missing],
              ],
            })),
          },
          {
            title: t.reviewHistory,
            empty: t.noReview,
            records: s.review
              ? [
                  {
                    id: String(s.projectId),
                    title: proposalStatus(s.review.decision),
                    fields: [
                      [t.reason, s.review.reason ?? t.missing],
                      [
                        t.reviewedBy,
                        s.review.reviewedByName ?? s.review.reviewedByUserId ?? t.missing,
                      ],
                      [t.reviewedOn, dateLabel(s.review.reviewedAt)],
                    ],
                  },
                ]
              : [],
          },
        ],
        actions: s.status === "PendingReview" ? ["approve", "reject"] : [],
      };
    }
    case "assignments":
      return { title: row.title, fields: row.fields ?? [], actions: ["assign"] };
  }
}

export async function applyChange(action: Change, id: string, value: string, signal: AbortSignal) {
  switch (action) {
    case "activate":
    case "deactivate":
      // [BACKEND-API] Update account. PUT /api/Admin/Students/{userId}/status. UpdateStudentStatusDto -> AdminStudentStatusDto.
      return setStudentStatus(id, { isActive: action === "activate" }, signal);
    case "capacity":
      // [BACKEND-API] Update workload. PUT /api/Admin/Supervisors/{supervisorId}/max-projects. UpdateSupervisorCapacityDto -> AdminSupervisorDetailDto.
      return setCapacity(Number(id), { maxProjects: Number(value) }, signal);
    case "approve":
    case "reject":
      // [BACKEND-API] Record decision. PUT /api/Admin/ProjectProposals/{projectId}/review. ReviewProjectProposalDto -> AdminProposalDetailDto.
      return reviewProposal(
        Number(id),
        { decision: action === "approve" ? "Approved" : "Rejected", reason: value.trim() || null },
        signal,
      );
    case "assign":
      // [BACKEND-API] Allocate or reassign approved project. PUT /api/Admin/ProjectAssignments/{projectId}. AssignSupervisorDto -> AdminProjectAssignmentDto.
      return assignSupervisor(Number(id), { supervisorId: Number(value) }, signal);
  }
}

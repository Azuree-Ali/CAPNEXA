import { snapshot, save } from "@/features/students/hooks/studentPreview";
import { studentForm } from "@/features/students/content/studentProfile.content";
import { operationsPreview as seed } from "../content/operationsPreview.content";
import { operations } from "../content/operations.content";
import type { Change } from "../types/operations.types";
import type { AdminStudentQueryDto } from "@/features/students/types/students.types";
import type { AdminSupervisorQueryDto } from "@/features/supervisors/types/supervisors.types";
import type { AdminProposalQueryDto } from "@/features/proposals/types/proposals.types";
import type { AdminUnassignedProjectsQueryDto } from "@/features/assignments/types/assignments.types";

const records = structuredClone(seed);
const t = operations.text;
function find<T>(items: T[], matches: (item: T) => boolean): T {
  const item = items.find(matches);
  if (!item) throw new Error(t.previewMissing);
  return item;
}
function paginate<T>(items: T[], { page = 1, pageSize = 5 }: { page?: number; pageSize?: number }) {
  return {
    items: items.slice((page - 1) * pageSize, page * pageSize),
    page,
    pageSize,
    totalCount: items.length,
    totalPages: Math.ceil(items.length / pageSize),
  };
}
function includes(search: string | null | undefined, ...values: (string | null)[]) {
  return values.some((value) => value?.toLowerCase().includes(search?.trim().toLowerCase() ?? ""));
}
function matches(value: string | null, filter?: string | null) {
  return !filter || value?.toLowerCase() === filter.toLowerCase();
}
function studentRecords() {
  const saved = snapshot();
  return [
    ...saved.map((item) => {
      const existing = records.students.find((student) => student.userId === item.id);
      const [firstName, ...surname] = item.fullName.split(" ");
      const team = records.proposals.find((proposal) => proposal.team.name === item.team);
      const project = records.proposals.find((proposal) => proposal.title === item.project);
      return {
        userId: item.id,
        firstName,
        lastName: surname.join(" "),
        email: item.email,
        department: item.department,
        academicYear: item.year,
        level: existing?.level ?? null,
        studentNumber: existing?.studentNumber ?? null,
        isActive: item.active,
        academicProfileComplete: Boolean(item.department && item.year),
        emailConfirmed: existing?.emailConfirmed ?? false,
        createdAt: existing?.createdAt ?? records.date,
        teams: team
          ? [
              {
                teamId: team.team.teamId,
                name: team.team.name,
                teamCode: team.team.teamCode,
                isLeader: item.role === studentForm.roles[0],
                isCreator: item.role === studentForm.roles[0],
                joinedAt: existing?.teams[0]?.joinedAt ?? null,
                projects: project
                  ? [
                      {
                        projectId: project.projectId,
                        title: project.title,
                        status: project.status,
                        createdAt: project.createdAt,
                      },
                    ]
                  : [],
              },
            ]
          : [],
      };
    }),
    ...records.students.filter((item) => !saved.some((savedItem) => savedItem.id === item.userId)),
  ];
}
export function previewStudents(query: AdminStudentQueryDto) {
  return paginate(
    studentRecords().filter(
      (s) =>
        includes(query.search, `${s.firstName} ${s.lastName}`, s.email, s.studentNumber) &&
        matches(s.department, query.department) &&
        matches(s.academicYear, query.academicYear) &&
        (query.isActive == null || s.isActive === query.isActive),
    ),
    query,
  );
}
export function previewSupervisors(query: AdminSupervisorQueryDto) {
  return paginate(
    records.supervisors.filter(
      (s) =>
        includes(query.search, `${s.firstName} ${s.lastName}`, s.email, s.specialization) &&
        matches(s.department, query.department) &&
        (query.isActive == null || s.isActive === query.isActive) &&
        (query.hasCapacity == null || s.availableCapacity > 0 === query.hasCapacity),
    ),
    query,
  );
}
export const previewStudent = (id: string) => find(studentRecords(), (s) => s.userId === id);
export const previewSupervisor = (id: number) =>
  find(records.supervisors, (s) => s.supervisorId === id);
export const previewProposal = (id: number) => find(records.proposals, (s) => s.projectId === id);

export const previewProposalByTitle = (title: string) =>
  records.proposals.find((item) => item.title === title);

const entries = () =>
  records.proposals.map((p) => ({
    ...p,
    reviewedAt: p.review?.reviewedAt ?? null,
    leaderName: p.team.members
      .filter((m) => m.isLeader)
      .map((m) => `${m.firstName} ${m.lastName}`)
      .join(", "),
  }));
export function previewProposals(query: AdminProposalQueryDto) {
  return paginate(
    entries().filter(
      (p) =>
        p.status === (query.status ?? "PendingReview") &&
        includes(query.search, p.title, p.team.name, p.team.teamCode, p.leaderName),
    ),
    query,
  );
}
export function previewUnassigned(query: AdminUnassignedProjectsQueryDto) {
  return paginate(
    entries().filter(
      (p) =>
        p.status === "Approved" &&
        !records.supervisors.some((s) =>
          s.assignedProjects.some((project) => project.projectId === p.projectId),
        ) &&
        includes(query.search, p.title, p.team.name, p.team.teamCode, p.leaderName),
    ),
    query,
  );
}
function updateCapacity(supervisor: ReturnType<typeof previewSupervisor>) {
  supervisor.assignedProjectsCount = supervisor.assignedProjects.length;
  supervisor.availableCapacity = supervisor.maxProjects - supervisor.assignedProjectsCount;
}
export function applyPreviewChange(action: Change, id: string, value: string): void {
  if (action === "activate" || action === "deactivate") {
    const existing = snapshot().find((student) => student.id === id);
    if (existing) save({ ...existing, active: action === "activate" });
    else
      find(records.students, (student) => student.userId === id).isActive = action === "activate";
    return;
  }
  if (action === "capacity") {
    const supervisor = previewSupervisor(Number(id));
    const capacity = Number(value);
    if (
      !value.trim() ||
      !Number.isInteger(capacity) ||
      capacity < supervisor.assignedProjects.length ||
      capacity > 20
    )
      throw new Error(t.capacityError);
    supervisor.maxProjects = capacity;
    updateCapacity(supervisor);
    return;
  }
  const proposal = previewProposal(Number(id));
  if (action === "assign") {
    const target = previewSupervisor(Number(value));
    if (proposal.status !== "Approved" || !target.isActive || target.availableCapacity < 1)
      throw new Error(t.previewUnavailable);
    for (const supervisor of records.supervisors) {
      supervisor.assignedProjects = supervisor.assignedProjects.filter(
        (p) => p.projectId !== proposal.projectId,
      );
      updateCapacity(supervisor);
    }
    target.assignedProjects.push({
      projectId: proposal.projectId,
      title: proposal.title,
      status: proposal.status,
      teamId: proposal.team.teamId,
      teamName: proposal.team.name,
      teamCode: proposal.team.teamCode,
    });
    updateCapacity(target);
    return;
  }
  if (proposal.status !== "PendingReview") throw new Error(t.previewUnavailable);
  if ((action === "reject" && !value.trim()) || value.length > 2000)
    throw new Error(t.reasonRequired);
  proposal.status = action === "approve" ? "Approved" : "Rejected";
  proposal.review = {
    decision: proposal.status,
    reason: value.trim() || null,
    reviewedAt: records.date,
    reviewedByUserId: null,
    reviewedByName: records.reviewer,
  };
  for (const student of records.students) {
    for (const team of student.teams) {
      for (const project of team.projects) {
        if (project.projectId === proposal.projectId) project.status = proposal.status;
      }
    }
  }
}

import type { StudentProfileDto } from "@/features/students/types/students.types";
import type { AdminSupervisorDetailDto } from "@/features/supervisors/types/supervisors.types";
import type {
  AdminProposalDetailDto,
  AdminProposalMemberDto,
} from "@/features/proposals/types/proposals.types";
import {
  exampleStudent,
  studentAcademicContext,
} from "@/features/students/content/studentProfile.content";

export interface OperationsPreviewContent {
  students: StudentProfileDto[];
  supervisors: AdminSupervisorDetailDto[];
  proposals: AdminProposalDetailDto[];
  reviewer: string;
  date: string;
}
const date = "2026-11-20T12:00:00Z";
const names = [
  ["Alex", "Morgan"],
  ["Maya", "Chen"],
  ["Nora", "Ibrahim"],
  ["Ethan", "Rivera"],
  ["Lina", "Hassan"],
  ["Sam", "Patel"],
  ["Amira", "Soliman"],
  ["Noah", "Brooks"],
  ["Hana", "Youssef"],
  ["Sara", "Reed"],
  ["Tariq", "Ali"],
  ["Yara", "Nasser"],
] as const;
const departments = ["Computer Science", "Information Systems", "Software Engineering"];
const students: StudentProfileDto[] = names.map(([firstName, lastName], index) => ({
  userId: index ? `${firstName}-${lastName}`.toLowerCase() : exampleStudent.id,
  firstName,
  lastName,
  email: index
    ? `${firstName.toLowerCase()}.${lastName.toLowerCase()}@example.edu`
    : exampleStudent.email,
  studentNumber: index
    ? `ST-${String(index + 1).padStart(3, "0")}`
    : studentAcademicContext.displayId,
  department: departments[index % departments.length],
  academicYear: index % 3 === 1 ? "2026–2027" : exampleStudent.year,
  level: index % 3 === 1 ? "Year 3" : "Year 4",
  isActive: index % 5 !== 4,
  emailConfirmed: index % 5 !== 4,
  academicProfileComplete: index % 5 !== 4,
  createdAt: date,
  teams: [],
}));
const projects = [
  ["Campus Atlas", "Atlas", "ATL", "Approved"],
  ["Library Lens", "Lens", "LEN", "PendingReview"],
  ["Campus Maps", "Maps", "MAP", "Approved"],
  ["Lab Inventory", "Inventory", "INV", "Rejected"],
  ["Research Archive", "Archive", "ARC", "PendingReview"],
  ["Studio Scheduler", "Studio", "STD", "Approved"],
] as const;
const summaries = [
  "Accessible navigation across university buildings and facilities.",
  "Search and discovery for the university library catalogue.",
  "A directory of campus services and accessible routes.",
  "Inventory tracking for shared technical laboratories.",
  "A structured archive for student research outputs.",
  "Scheduling for shared studio rooms and equipment.",
];
const reviewer = "Jordan Blake";
const proposals = projects.map(([title, name, teamCode, status], index): AdminProposalDetailDto => {
  const projectId = index + 1;
  const members = students
    .slice(index * 2, index * 2 + 2)
    .map((student, position): AdminProposalMemberDto => {
      student.teams.push({
        teamId: projectId,
        name,
        teamCode,
        isLeader: position === 0,
        isCreator: position === 0,
        joinedAt: date,
        projects: [{ projectId, title, status, createdAt: date }],
      });
      return {
        userId: student.userId,
        firstName: student.firstName,
        lastName: student.lastName,
        email: student.email,
        studentNumber: student.studentNumber ?? "",
        department: student.department,
        academicYear: student.academicYear,
        isLeader: position === 0,
        joinedAt: date,
      };
    });
  return {
    projectId,
    title,
    status,
    createdAt: date,
    submittedAt: date,
    description: summaries[index],
    team: {
      teamId: projectId,
      name,
      teamCode,
      maxMembers: 5,
      memberCount: members.length,
      members,
    },
    review:
      status === "PendingReview"
        ? null
        : {
            decision: status,
            reason:
              status === "Rejected" ? "The submission requires a feasible delivery plan." : null,
            reviewedByUserId: null,
            reviewedByName: reviewer,
            reviewedAt: date,
          },
  };
});
export const operationsPreview: OperationsPreviewContent = {
  students,
  proposals,
  reviewer,
  date,
  supervisors: (
    [
      ["Olivia", "Bennett", "Distributed systems", 3, true],
      ["Daniel", "Reed", "Information retrieval", 4, true],
      ["Leila", "Mansour", "Data engineering", 2, true],
      ["Omar", "Kamal", "Human-computer interaction", 0, true],
      ["Priya", "Shah", "Software testing", 3, false],
      ["Adam", "Salem", "Applied machine learning", 2, true],
    ] as const
  ).map(([firstName, lastName, specialization, maxProjects, isActive], index) => {
    const assignedProjects =
      index === 0
        ? [
            {
              projectId: 1,
              title: "Campus Atlas",
              status: "Approved",
              teamId: 1,
              teamName: "Atlas",
              teamCode: "ATL",
            },
          ]
        : [];
    return {
      supervisorId: index + 1,
      userId: `faculty-${index + 1}`,
      firstName,
      lastName,
      specialization,
      maxProjects,
      isActive,
      email: `${String(firstName).toLowerCase()}.faculty@example.edu`,
      department: departments[index % departments.length],
      researchInterests: specialization,
      assignedProjects,
      assignedProjectsCount: assignedProjects.length,
      availableCapacity: maxProjects - assignedProjects.length,
    };
  }),
};

export interface AdminStudentQueryDto {
  search?: string | null;
  department?: string | null;
  academicYear?: string | null;
  isActive?: boolean | null;
  page?: number;
  pageSize?: number;
}
export interface AdminStudentListDto {
  items: AdminStudentListItemDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
export interface AdminStudentListItemDto {
  userId: string;
  email: string | null;
  firstName: string;
  lastName: string;
  studentNumber: string | null;
  department: string | null;
  academicYear: string | null;
  academicProfileComplete: boolean;
  emailConfirmed: boolean;
  isActive: boolean;
  createdAt: string;
}
export interface StudentProfileDto {
  userId: string;
  email: string | null;
  emailConfirmed: boolean;
  firstName: string;
  lastName: string;
  isActive: boolean;
  createdAt: string;
  academicProfileComplete: boolean;
  studentNumber: string | null;
  department: string | null;
  academicYear: string | null;
  level: string | null;
  teams: StudentTeamDto[];
}
export interface StudentTeamDto {
  teamId: number;
  name: string;
  teamCode: string;
  isLeader: boolean;
  isCreator: boolean;
  joinedAt: string | null;
  projects: StudentProjectDto[];
}
export interface StudentProjectDto {
  projectId: number;
  title: string;
  status: string;
  createdAt: string;
}
export interface UpdateStudentStatusDto {
  isActive: boolean | null;
}
export interface AdminStudentStatusDto {
  userId: string;
  isActive: boolean;
}

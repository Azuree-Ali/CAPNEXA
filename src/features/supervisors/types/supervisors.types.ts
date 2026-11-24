export interface AdminSupervisorQueryDto {
  search?: string | null;
  department?: string | null;
  isActive?: boolean | null;
  hasCapacity?: boolean | null;
  page?: number;
  pageSize?: number;
}
export interface AdminSupervisorListDto {
  items: AdminSupervisorListItemDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
export interface AdminSupervisorListItemDto {
  supervisorId: number;
  userId: string;
  firstName: string;
  lastName: string;
  email: string | null;
  isActive: boolean;
  department: string | null;
  specialization: string | null;
  researchInterests: string | null;
  maxProjects: number;
  assignedProjectsCount: number;
  availableCapacity: number;
}
export interface AdminSupervisorDetailDto {
  supervisorId: number;
  userId: string;
  firstName: string;
  lastName: string;
  email: string | null;
  isActive: boolean;
  department: string | null;
  specialization: string | null;
  researchInterests: string | null;
  maxProjects: number;
  assignedProjectsCount: number;
  availableCapacity: number;
  assignedProjects: AdminSupervisorProjectDto[];
}
export interface AdminSupervisorProjectDto {
  projectId: number;
  title: string;
  status: string;
  teamId: number;
  teamName: string;
  teamCode: string;
}
export interface UpdateSupervisorCapacityDto {
  maxProjects: number | null;
}

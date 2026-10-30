export enum Department {
  Engineering = 'Engineering',
  HR = 'HR',
  Finance = 'Finance',
  Sales = 'Sales',
  Operations = 'Operations',
  Support = 'Support'
}

export enum UserRole {
  Admin = 'Admin',
  Manager = 'Manager',
  Employee = 'Employee'
}

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  department: Department;
  departmentName: string;
  role: UserRole;
  roleName: string;
  isActive: boolean;
  createdDate: string;
  modifiedDate?: string;
}

export interface CreateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  department: Department;
  role: UserRole;
}

export interface UpdateUserRequest {
  firstName: string;
  lastName: string;
  department: Department;
  role: UserRole;
}

export interface UserQueryParameters {
  searchTerm?: string;
  department?: Department | '';
  role?: UserRole | '';
  isActive?: boolean | '';
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
}


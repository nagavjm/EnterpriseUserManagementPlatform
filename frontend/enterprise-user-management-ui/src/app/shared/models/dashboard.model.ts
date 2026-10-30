import { User } from './user.model';

export interface DashboardSummary {
  totalUsers: number;
  activeUsers: number;
  inactiveUsers: number;
  usersByDepartment: { [department: string]: number };
  usersByRole: { [role: string]: number };
  recentUsers: User[];
}


export type UserType =
  | 'User'
  | 'UserLab'
  | 'Store'
  | 'AdminLab'
  | 'Administrator'
  | 'Admin';

export interface UserDto {
  id: string;
  userType: UserType;
  firstName: string;
  lastName: string;
  username: string;
  mobileNumber: string;
  isActive: boolean;
  address: string;
  phone: string;
  email: string;
  emailConfirmed: boolean;
  mobileConfirmed: boolean;
  createdAt: string;
}

export interface GetUsersQuery {
  search?: string;
  userType?: UserType;
  isActive?: boolean;
  page: number;
  pageSize: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface UserStatsDto {
  totalUsers: number;
  activeUsers: number;
  administratorCount: number;
  laboratoryCount: number;
  userCount: number;
  storeCount: number;
  monthlySiteVisits: number;
  onlineUsersCount: number;
}

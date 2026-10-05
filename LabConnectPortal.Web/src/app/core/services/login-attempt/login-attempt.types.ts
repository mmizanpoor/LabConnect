export type LoginMethod = 'Mobile' | 'Username';

export interface UserLoginLogDto {
  id: number;
  userId?: string | null;
  displayName?: string | null;
  loginMethod: LoginMethod | number;
  success: boolean;
  username?: string | null;
  mobileNumber?: string | null;
  ipAddress?: string | null;
  failureReason?: string | null;
  createdAt: string;
}

export interface GetLoginAttemptsQuery {
  from?: string;
  to?: string;
  success?: boolean;
  loginMethod?: LoginMethod;
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

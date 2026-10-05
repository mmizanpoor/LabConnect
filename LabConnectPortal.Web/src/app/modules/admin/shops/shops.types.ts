export type ProfileCompletionFilter = 'All' | 'Complete' | 'Incomplete';

export interface CenterProfileListItemDto {
  id: string;
  name: string;
  mobileNumber: string;
  isComplete: boolean;
  isApproved: boolean;
  isApiKeyEnabled: boolean;
}

export interface GetCenterProfilesQuery {
  name?: string;
  mobile?: string;
  completionStatus?: ProfileCompletionFilter;
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CenterProfileDto {
  id: string;
  name: string;
  address: string;
  phone: string;
  economicCode?: string | null;
  registrationNumber?: string | null;
  hasLogo: boolean;
  hasNationalCard: boolean;
  hasLicense: boolean;
  hasOfficialImage: boolean;
  hasTradeCard: boolean;
  isComplete: boolean;
  isApproved: boolean;
  isApiKeyEnabled: boolean;
  approvedAt?: string;
  mobileNumber?: string;
  rejectionReason?: string;
  rejectedAt?: string;
}

export type ProfileCompletionFilter = 'All' | 'Complete' | 'Incomplete';

export type CenterProfileStatus = 'Pending' | 'Active' | 'Suspended';

export interface CenterProfileListItemDto {
  id: string;
  name: string;
  labCode?: number;
  mobileNumber: string;
  status?: CenterProfileStatus | number;
  isComplete: boolean;
  isApproved: boolean;
  isApiKeyEnabled: boolean;
}

export interface GetCenterProfilesQuery {
  name?: string;
  labCode?: string;
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
  centerType: string;
  status?: CenterProfileStatus | number;
  labCode?: number;
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
  completedAt?: string;
  approvedAt?: string;
  mobileNumber?: string;
  rejectionReason?: string;
  rejectedAt?: string;
}

export interface CreateLaboratoryCommand {
  labCodeNew: number;
  mobileNumber: string;
}

export interface UpdateLaboratoryMobileCommand {
  profileId: string;
  mobileNumber: string;
}

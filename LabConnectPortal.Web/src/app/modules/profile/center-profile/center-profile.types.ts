import { SiteChargePricingMode } from '../../admin/site-charge-services/site-charge-services.types';

export type CenterType = 'Lab' | 'Store';

export type CenterProfileStatus = 'Pending' | 'Active' | 'Suspended';

export type CenterProfileFileKind =
  | 'Logo'
  | 'NationalCard'
  | 'License'
  | 'OfficialImage'
  | 'TradeCard';

export type EmployeeCountRange =
  | 'LessThan10'
  | 'Between10And50'
  | 'Between50And200'
  | 'MoreThan200';

export type ProfileCompletionFilter = 'All' | 'Complete' | 'Incomplete';

export interface CenterProfileDto {
  id: string;
  centerType: CenterType;
  status?: CenterProfileStatus | number;
  labCode?: number;
  name: string;
  address: string;
  phone: string;
  establishedYear?: number | null;
  description: string;
  website?: string | null;
  employeeCount?: EmployeeCountRange | null;
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
  email?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  rejectionReason?: string;
  rejectedAt?: string;
  smsCount?: number;
}

export const EMPLOYEE_COUNT_RANGES: EmployeeCountRange[] = [
  'LessThan10',
  'Between10And50',
  'Between50And200',
  'MoreThan200',
];

export interface UpdateCenterProfileCommand {
  name: string;
  address: string;
  phone: string;
  establishedYear?: number | null;
  description: string;
  website?: string | null;
  employeeCount?: EmployeeCountRange | null;
  economicCode?: string | null;
  registrationNumber?: string | null;
  mobileNumber?: string | null;
  email?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  code?: string | null;
  channel?: 'Email' | 'Mobile' | null;
}

export interface SendCenterContactChangeOtpCommand {
  channel: 'Email' | 'Mobile';
}

export interface SmsChargeInfoDto {
  remainingCount: number;
  pricingMode: SiteChargePricingMode | string | number;
  unitPrice?: number | null;
  prices: SiteChargeServicePriceOptionDto[];
}

export interface SiteChargeServicePriceOptionDto {
  siteChargeServicePriceId: number;
  title?: string | null;
  minQuantity?: number | null;
  maxQuantity?: number | null;
  packageQuantity?: number | null;
  price: number;
}

export interface InitPayChargeCommand {
  count?: number | null;
  priceId?: number | null;
}

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

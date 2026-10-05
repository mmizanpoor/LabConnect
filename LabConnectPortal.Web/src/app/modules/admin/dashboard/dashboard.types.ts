export interface DashboardStats {
  totalUsers: number;
  activeUsers: number;
  administratorCount: number;
  laboratoryCount: number;
  userCount: number;
  storeCount: number;
  monthlySiteVisits: number;
  onlineUsersCount: number;
}

export interface LabAgreementStats {
  activeCount: number;
  expiredCount: number;
  pendingCount: number;
}

export type {
  AdminReceptionDashboardStats,
  TopLabMetric,
} from '../../profile/receptions/receptions.types';
export type { JobPostingDashboardStats } from '../../profile/job-postings/job-postings.types';
export type { SpecialOfferDashboardStats } from '../../profile/special-offers/special-offers.types';

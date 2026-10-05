export interface SRLabNameDto {
  intLabId: number;
  intLabIdNew?: number | null;
  vchLabName?: string | null;
}

export interface LabReceiverRangeDetailDto {
  id?: number | null;
  targetLabId: number;
  targetRangeDetailId: number;
  minNormalValue?: number;
  maxNormalValue?: number;
  normalText?: string | null;
  borderLineText?: string | null;
  unitDesc?: string | null;
}

export interface ReceptTestDto {
  id: number;
  sourceLabId: number;
  sourceReceptId?: string | null;
  sourceSendDate?: string | null;
  sourceTestName?: string | null;
  sourceCPN?: string | null;
  targetLabId: number;
  targetReceptId?: string | null;
  targetRejected: boolean;
  isUrgent: boolean;
  result?: string | null;
  rangeDetail?: LabReceiverRangeDetailDto | null;
}

export interface ReceptionDto {
  sourceLabId: number;
  sourceReceptId: string;
  targetLabId: number;
  targetReceptId?: string | null;
  sourceSendReceptDate?: string | null;
  age: number;
  ageType?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  gender: boolean;
  mobile?: string | null;
  nic?: string | null;
  isUrgent: boolean;
  receptTests: ReceptTestDto[];
}

export interface ReceiveReceptionGroupFilterQuery {
  labCode: number;
  sourceLabCodes?: number[] | null;
  fromDate: string;
  toDate: string;
  /** true = only accepted; false = only pending; null/undefined = all */
  isReception?: boolean | null;
  isReject: boolean;
  receptNoSender?: string | null;
}

export interface ClearReceiverReceptionItem {
  id: number;
  sourceLabId: number;
  sourceReceptId: string;
  targetLabId: number;
}

export interface ClearReceiverReceptionCommand {
  items: ClearReceiverReceptionItem[];
}

export interface TopLabMetric {
  labCode?: number | null;
  labName?: string | null;
  count: number;
}

export interface LabReceptionSummaryRow {
  labCode: number;
  labName?: string | null;
  receptionCount: number;
  testsCount: number;
}

export interface PopularTestMetric {
  testName: string;
  count: number;
}

/** Matches LabConnectPortal.Api.Domain.Enums.AdminReceptionDashboardSection */
export type AdminReceptionDashboardSection =
  | 'TopLabs'
  | 'MonthlyPopularTests'
  | 'YearlyPopularTests'
  | 'DailySentLabs'
  | 'DailyReceivedLabs'
  | 'MonthlySentLabs'
  | 'MonthlyReceivedLabs';

export interface AdminReceptionDashboardStats {
  dailyMostUsage: TopLabMetric;
  monthlyMostUsage: TopLabMetric;
  dailyMostSentTests: TopLabMetric;
  monthlyMostSentTests: TopLabMetric;
  dailyMostReceivedTests: TopLabMetric;
  monthlyMostReceivedTests: TopLabMetric;
  dailyLabs: LabReceptionSummaryRow[];
  monthlyLabs: LabReceptionSummaryRow[];
  sentDailyStats?: LabReceptionPeriodStats | null;
  sentMonthlyStats?: LabReceptionPeriodStats | null;
  receivedDailyStats?: LabReceptionPeriodStats | null;
  receivedMonthlyStats?: LabReceptionPeriodStats | null;
  dailySentLabs?: LabReceptionSummaryRow[];
  monthlySentLabs?: LabReceptionSummaryRow[];
  dailyReceivedLabs?: LabReceptionSummaryRow[];
  monthlyReceivedLabs?: LabReceptionSummaryRow[];
  monthlyPopularTests?: PopularTestMetric[];
  yearlyPopularTests?: PopularTestMetric[];
}

export interface LabReceptionPeriodStats {
  usageCount: number;
  testsCount: number;
}

export interface LabReceptionDirectionStats {
  daily: LabReceptionPeriodStats;
  monthly: LabReceptionPeriodStats;
  dailyPartners: LabReceptionSummaryRow[];
  monthlyPartners: LabReceptionSummaryRow[];
}

export interface LabReceptionDashboardStats {
  sent: LabReceptionDirectionStats;
  received: LabReceptionDirectionStats;
}

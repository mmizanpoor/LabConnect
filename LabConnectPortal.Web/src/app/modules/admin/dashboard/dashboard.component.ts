import { Component, OnInit } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { downloadTableAsExcel } from '@core/utils/excel-export.util';
import {
  dashboardThemeClasses,
  DashboardTheme,
} from '@core/utils/dashboard-theme.util';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { AgreementsService } from '../../profile/agreements/agreements.service';
import { JobPostingsService } from '../../profile/job-postings/job-postings.service';
import { ReceptionsService } from '../../profile/receptions/receptions.service';
import { SpecialOffersService } from '../../profile/special-offers/special-offers.service';
import { SpecialOfferLabMonthlyRow } from '../../profile/special-offers/special-offers.types';
import {
  TopLabMetric,
  LabReceptionSummaryRow,
  PopularTestMetric,
  AdminReceptionDashboardSection,
} from '../../profile/receptions/receptions.types';
import { UsersService } from '../users/users.service';
import {
  AdminReceptionDashboardStats,
  DashboardStats,
  JobPostingDashboardStats,
  LabAgreementStats,
  SpecialOfferDashboardStats,
} from './dashboard.types';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { SitePermissionService } from '@core/services/auth/site-permission.service';

interface DashboardStatCard {
  labelKey: string;
  icon: string;
  theme: 'blue' | 'green' | 'violet' | 'amber' | 'slate' | 'rose';
  value: keyof DashboardStats;
  link?: string;
}

interface AgreementStatCard {
  labelKey: string;
  icon: string;
  theme: 'blue' | 'green' | 'violet' | 'amber' | 'slate' | 'rose';
  value: keyof LabAgreementStats;
}

interface JobPostingStatCard {
  labelKey: string;
  icon: string;
  theme: 'blue' | 'green' | 'violet' | 'amber' | 'slate' | 'rose';
  value: keyof JobPostingDashboardStats;
}

interface TopLabStatCard {
  labelKey: string;
  icon: string;
  theme: 'blue' | 'green' | 'violet' | 'amber' | 'slate' | 'rose';
  value:
    | 'dailyMostUsage'
    | 'monthlyMostUsage'
    | 'dailyMostSentTests'
    | 'monthlyMostSentTests'
    | 'dailyMostReceivedTests'
    | 'monthlyMostReceivedTests';
}

interface DashboardQuickLink {
  labelKey: string;
  icon: string;
  link: string;
  theme: 'blue' | 'green' | 'violet' | 'amber' | 'slate' | 'rose';
}

interface LabTableSection {
  titleKey: string;
  rowsKey:
    | 'dailySentLabs'
    | 'dailyReceivedLabs'
    | 'monthlySentLabs'
    | 'monthlyReceivedLabs';
  section: AdminReceptionDashboardSection;
}

type ReceptionReportKey =
  | 'topLabs'
  | 'monthlyPopularTests'
  | 'yearlyPopularTests'
  | 'dailySentLabs'
  | 'dailyReceivedLabs'
  | 'monthlySentLabs'
  | 'monthlyReceivedLabs';

interface ReceptionSectionUiState {
  loading: boolean;
  loaded: boolean;
  error: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [MatIconModule, RouterLink, TranslocoPipe],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  readonly entity = SystemEntity.Dashboard;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.Dashboard);

  stats: DashboardStats | null = null;
  agreementStats: LabAgreementStats | null = null;
  jobPostingStats: JobPostingDashboardStats | null = null;
  receptionStats: AdminReceptionDashboardStats | null = null;
  specialOfferStats: SpecialOfferDashboardStats | null = null;
  labNameMap: Record<number, string> = {};
  loading = true;
  agreementLoading = true;
  jobPostingLoading = true;
  specialOfferLoading = true;
  statsError = '';
  agreementError = '';
  jobPostingError = '';
  specialOfferError = '';

  readonly receptionSectionState: Record<ReceptionReportKey, ReceptionSectionUiState> = {
    topLabs: { loading: false, loaded: false, error: '' },
    monthlyPopularTests: { loading: false, loaded: false, error: '' },
    yearlyPopularTests: { loading: false, loaded: false, error: '' },
    dailySentLabs: { loading: false, loaded: false, error: '' },
    dailyReceivedLabs: { loading: false, loaded: false, error: '' },
    monthlySentLabs: { loading: false, loaded: false, error: '' },
    monthlyReceivedLabs: { loading: false, loaded: false, error: '' },
  };

  private readonly _receptionSectionApi: Record<
    ReceptionReportKey,
    AdminReceptionDashboardSection
  > = {
    topLabs: 'TopLabs',
    monthlyPopularTests: 'MonthlyPopularTests',
    yearlyPopularTests: 'YearlyPopularTests',
    dailySentLabs: 'DailySentLabs',
    dailyReceivedLabs: 'DailyReceivedLabs',
    monthlySentLabs: 'MonthlySentLabs',
    monthlyReceivedLabs: 'MonthlyReceivedLabs',
  };

  readonly overviewCards: DashboardStatCard[] = [
    {
      labelKey: 'modules.admin.dashboard.totalUsers',
      icon: 'groups',
      theme: 'blue',
      value: 'totalUsers',
      link: '/admin/users',
    },
    {
      labelKey: 'modules.admin.dashboard.activeUsers',
      icon: 'verified_user',
      theme: 'green',
      value: 'activeUsers',
      link: '/admin/users',
    },
    {
      labelKey: 'modules.admin.dashboard.administrators',
      icon: 'admin_panel_settings',
      theme: 'violet',
      value: 'administratorCount',
      link: '/admin/users',
    },
    {
      labelKey: 'modules.admin.dashboard.monthlySiteVisits',
      icon: 'visibility',
      theme: 'green',
      value: 'monthlySiteVisits',
    },
    {
      labelKey: 'modules.admin.dashboard.onlineUsers',
      icon: 'sensors',
      theme: 'amber',
      value: 'onlineUsersCount',
    },
  ];

  readonly userTypeCards: DashboardStatCard[] = [
    {
      labelKey: 'modules.admin.dashboard.laboratories',
      icon: 'science',
      theme: 'amber',
      value: 'laboratoryCount',
      link: '/admin/laboratories',
    },
    {
      labelKey: 'modules.admin.dashboard.users',
      icon: 'person',
      theme: 'slate',
      value: 'userCount',
      link: '/admin/users',
    },
    {
      labelKey: 'modules.admin.dashboard.stores',
      icon: 'storefront',
      theme: 'rose',
      value: 'storeCount',
      link: '/admin/shops',
    },
  ];

  readonly agreementCards: AgreementStatCard[] = [
    {
      labelKey: 'modules.admin.dashboard.agreements.active',
      icon: 'task_alt',
      theme: 'green',
      value: 'activeCount',
    },
    {
      labelKey: 'modules.admin.dashboard.agreements.expired',
      icon: 'event_busy',
      theme: 'slate',
      value: 'expiredCount',
    },
    {
      labelKey: 'modules.admin.dashboard.agreements.pending',
      icon: 'hourglass_top',
      theme: 'amber',
      value: 'pendingCount',
    },
  ];

  readonly jobPostingCards: JobPostingStatCard[] = [
    {
      labelKey: 'modules.admin.dashboard.jobPostings.active',
      icon: 'work',
      theme: 'green',
      value: 'activeCount',
    },
    {
      labelKey: 'modules.admin.dashboard.jobPostings.expired',
      icon: 'event_busy',
      theme: 'slate',
      value: 'expiredCount',
    },
    {
      labelKey: 'modules.admin.dashboard.jobPostings.sentApplications',
      icon: 'send',
      theme: 'blue',
      value: 'sentApplicationsCount',
    },
  ];

  readonly receptionTopLabCards: TopLabStatCard[] = [
    {
      labelKey: 'modules.admin.dashboard.receptions.dailyMostUsage',
      icon: 'leaderboard',
      theme: 'blue',
      value: 'dailyMostUsage',
    },
    {
      labelKey: 'modules.admin.dashboard.receptions.monthlyMostUsage',
      icon: 'insights',
      theme: 'violet',
      value: 'monthlyMostUsage',
    },
    {
      labelKey: 'modules.admin.dashboard.receptions.dailyMostSentTests',
      icon: 'outbound',
      theme: 'green',
      value: 'dailyMostSentTests',
    },
    {
      labelKey: 'modules.admin.dashboard.receptions.monthlyMostSentTests',
      icon: 'upload',
      theme: 'amber',
      value: 'monthlyMostSentTests',
    },
    {
      labelKey: 'modules.admin.dashboard.receptions.dailyMostReceivedTests',
      icon: 'inbox',
      theme: 'slate',
      value: 'dailyMostReceivedTests',
    },
    {
      labelKey: 'modules.admin.dashboard.receptions.monthlyMostReceivedTests',
      icon: 'download',
      theme: 'rose',
      value: 'monthlyMostReceivedTests',
    },
  ];

  readonly labTableSections: LabTableSection[] = [
    {
      titleKey: 'modules.admin.dashboard.receptions.dailySentLabsTable',
      rowsKey: 'dailySentLabs',
      section: 'DailySentLabs',
    },
    {
      titleKey: 'modules.admin.dashboard.receptions.dailyReceivedLabsTable',
      rowsKey: 'dailyReceivedLabs',
      section: 'DailyReceivedLabs',
    },
    {
      titleKey: 'modules.admin.dashboard.receptions.monthlySentLabsTable',
      rowsKey: 'monthlySentLabs',
      section: 'MonthlySentLabs',
    },
    {
      titleKey: 'modules.admin.dashboard.receptions.monthlyReceivedLabsTable',
      rowsKey: 'monthlyReceivedLabs',
      section: 'MonthlyReceivedLabs',
    },
  ];

  readonly quickLinks: DashboardQuickLink[] = [
    {
      labelKey: 'modules.admin.layout.users',
      icon: 'manage_accounts',
      link: '/admin/users',
      theme: 'blue',
    },
    {
      labelKey: 'modules.admin.layout.laboratories',
      icon: 'biotech',
      link: '/admin/laboratories',
      theme: 'amber',
    },
    {
      labelKey: 'modules.admin.layout.shops',
      icon: 'store',
      link: '/admin/shops',
      theme: 'rose',
    },
    {
      labelKey: 'modules.admin.layout.products',
      icon: 'inventory_2',
      link: '/admin/products',
      theme: 'green',
    },
    {
      labelKey: 'modules.admin.layout.posts',
      icon: 'article',
      link: '/admin/posts',
      theme: 'violet',
    },
    {
      labelKey: 'modules.admin.layout.sliderGroups',
      icon: 'view_carousel',
      link: '/admin/slider-groups',
      theme: 'slate',
    },
    {
      labelKey: 'modules.profile.layout.changePassword',
      icon: 'lock_reset',
      link: '/admin/change-password',
      theme: 'violet',
    },
  ];

  constructor(
    private _usersService: UsersService,
    private _agreementsService: AgreementsService,
    private _jobPostingsService: JobPostingsService,
    private _receptionsService: ReceptionsService,
    private _specialOffersService: SpecialOffersService,
    private _localization: LocalizationService,
    private _authUtils: AuthUtils,
    private _sitePermission: SitePermissionService
  ) {}

  /** Global agreement/job/reception admin stats are Administrator-only. */
  get canViewAgreements(): boolean {
    return this._authUtils.isAdministrator();
  }

  get canViewJobPostings(): boolean {
    return this._authUtils.isAdministrator();
  }

  get canViewReceptions(): boolean {
    return this._authUtils.isAdministrator();
  }

  get canViewSpecialOffers(): boolean {
    return this._sitePermission.canView(SystemEntity.SpecialOffer);
  }

  get canViewUserStats(): boolean {
    return this._sitePermission.canView(SystemEntity.User);
  }

  async ngOnInit(): Promise<void> {
    const loadUsers = this.canViewUserStats
      ? this._usersService.getStats()
      : Promise.resolve(null);
    const loadAgreements = this.canViewAgreements
      ? this._agreementsService.getGlobalStats()
      : Promise.resolve(null);
    const loadJobPostings = this.canViewJobPostings
      ? this._jobPostingsService.getDashboardStats()
      : Promise.resolve(null);
    const loadSpecialOffers = this.canViewSpecialOffers
      ? this._specialOffersService.getDashboardStats()
      : Promise.resolve(null);

    if (!this.canViewUserStats) this.loading = false;
    if (!this.canViewAgreements) this.agreementLoading = false;
    if (!this.canViewJobPostings) this.jobPostingLoading = false;
    if (!this.canViewSpecialOffers) this.specialOfferLoading = false;

    const [
      userResult,
      agreementResult,
      jobPostingResult,
      specialOfferResult,
    ] = await Promise.allSettled([
      loadUsers,
      loadAgreements,
      loadJobPostings,
      loadSpecialOffers,
    ]);

    if (this.canViewUserStats) {
      if (
        userResult.status === 'fulfilled' &&
        userResult.value &&
        userResult.value.success &&
        userResult.value.data
      ) {
        this.stats = userResult.value.data;
      } else if (!this.isAccessDenied(userResult)) {
        this.statsError = this.resolveSettledError(
          userResult,
          'بارگذاری آمار کاربران ناموفق بود.'
        );
      }
      this.loading = false;
    }

    if (this.canViewAgreements) {
      if (agreementResult.status === 'fulfilled' && agreementResult.value) {
        this.agreementStats = agreementResult.value;
      } else if (!this.isAccessDenied(agreementResult)) {
        this.agreementError = this.resolveSettledError(agreementResult);
      }
      this.agreementLoading = false;
    }

    if (this.canViewJobPostings) {
      if (jobPostingResult.status === 'fulfilled' && jobPostingResult.value) {
        this.jobPostingStats = jobPostingResult.value;
      } else if (!this.isAccessDenied(jobPostingResult)) {
        this.jobPostingError = this.resolveSettledError(jobPostingResult);
      }
      this.jobPostingLoading = false;
    }

    if (this.canViewSpecialOffers) {
      if (
        specialOfferResult.status === 'fulfilled' &&
        specialOfferResult.value
      ) {
        this.specialOfferStats = specialOfferResult.value;
      } else if (!this.isAccessDenied(specialOfferResult)) {
        this.specialOfferError = this.resolveSettledError(specialOfferResult);
      }
      this.specialOfferLoading = false;
    }
  }

  async loadReceptionSection(key: ReceptionReportKey): Promise<void> {
    if (!this.canViewReceptions || this.receptionSectionState[key].loading) return;

    this.receptionSectionState[key] = {
      loading: true,
      loaded: this.receptionSectionState[key].loaded,
      error: '',
    };

    try {
      const partial = await this._receptionsService.getAdminDashboardStats(
        undefined,
        this._receptionSectionApi[key],
      );
      this.mergeReceptionSection(key, partial);
      await this.loadLabNames();
      this.receptionSectionState[key] = { loading: false, loaded: true, error: '' };
    } catch (err) {
      const error = this.isAccessDeniedReason(err) ? '' : this.errorMessage(err);
      this.receptionSectionState[key] = {
        loading: false,
        loaded: false,
        error,
      };
    }
  }

  isReceptionSectionLoading(key: ReceptionReportKey): boolean {
    return this.receptionSectionState[key].loading;
  }

  isReceptionSectionLoaded(key: ReceptionReportKey): boolean {
    return this.receptionSectionState[key].loaded;
  }

  receptionSectionError(key: ReceptionReportKey): string {
    return this.receptionSectionState[key].error;
  }

  private mergeReceptionSection(
    key: ReceptionReportKey,
    partial: AdminReceptionDashboardStats,
  ): void {
    if (!this.receptionStats) {
      this.receptionStats = {
        dailyMostUsage: { count: 0 },
        monthlyMostUsage: { count: 0 },
        dailyMostSentTests: { count: 0 },
        monthlyMostSentTests: { count: 0 },
        dailyMostReceivedTests: { count: 0 },
        monthlyMostReceivedTests: { count: 0 },
        dailyLabs: [],
        monthlyLabs: [],
        dailySentLabs: [],
        monthlySentLabs: [],
        dailyReceivedLabs: [],
        monthlyReceivedLabs: [],
        monthlyPopularTests: [],
        yearlyPopularTests: [],
      };
    }

    switch (key) {
      case 'topLabs':
        this.receptionStats.dailyMostUsage = partial.dailyMostUsage;
        this.receptionStats.monthlyMostUsage = partial.monthlyMostUsage;
        this.receptionStats.dailyMostSentTests = partial.dailyMostSentTests;
        this.receptionStats.monthlyMostSentTests = partial.monthlyMostSentTests;
        this.receptionStats.dailyMostReceivedTests = partial.dailyMostReceivedTests;
        this.receptionStats.monthlyMostReceivedTests = partial.monthlyMostReceivedTests;
        break;
      case 'monthlyPopularTests':
        this.receptionStats.monthlyPopularTests = partial.monthlyPopularTests ?? [];
        break;
      case 'yearlyPopularTests':
        this.receptionStats.yearlyPopularTests = partial.yearlyPopularTests ?? [];
        break;
      case 'dailySentLabs':
        this.receptionStats.dailySentLabs = partial.dailySentLabs ?? [];
        break;
      case 'dailyReceivedLabs':
        this.receptionStats.dailyReceivedLabs = partial.dailyReceivedLabs ?? [];
        break;
      case 'monthlySentLabs':
        this.receptionStats.monthlySentLabs = partial.monthlySentLabs ?? [];
        break;
      case 'monthlyReceivedLabs':
        this.receptionStats.monthlyReceivedLabs = partial.monthlyReceivedLabs ?? [];
        break;
    }
  }

  private isAccessDenied(
    result: PromiseSettledResult<unknown>
  ): boolean {
    if (result.status === 'rejected') {
      return this.isAccessDeniedReason(result.reason);
    }
    return false;
  }

  private isAccessDeniedReason(err: unknown): boolean {
    const msg = this.errorMessage(err);
    if (msg.includes('دسترسی مجاز نیست')) return true;
    if (err && typeof err === 'object' && 'status' in err) {
      const status = (err as { status?: number }).status;
      if (status === 403 || status === 401) return true;
    }
    return false;
  }

  private resolveSettledError(
    result: PromiseSettledResult<unknown>,
    fallback = ''
  ): string {
    if (result.status === 'rejected') {
      return this.errorMessage(result.reason) || fallback;
    }
    return fallback;
  }

  private errorMessage(err: unknown): string {
    if (err instanceof Error) return err.message;
    if (err && typeof err === 'object') {
      const e = err as {
        error?: { message?: string };
        message?: string;
      };
      if (typeof e.error?.message === 'string') return e.error.message;
      if (typeof e.message === 'string') return e.message;
    }
    return '';
  }

  statValue(card: DashboardStatCard): number {
    return this.stats?.[card.value] ?? 0;
  }

  agreementStatValue(card: AgreementStatCard): number {
    return this.agreementStats?.[card.value] ?? 0;
  }

  jobPostingStatValue(card: JobPostingStatCard): number {
    return this.jobPostingStats?.[card.value] ?? 0;
  }

  topLabMetric(card: TopLabStatCard): TopLabMetric | null {
    return this.receptionStats?.[card.value] ?? null;
  }

  topLabDisplayName(metric: TopLabMetric | null): string {
    if (!metric?.count || metric.labCode == null) return '—';
    return this._receptionsService.resolveLabName(
      this.labNameMap,
      metric.labCode
    );
  }

  labRowDisplayName(row: LabReceptionSummaryRow): string {
    return this._receptionsService.resolveLabName(this.labNameMap, row.labCode);
  }

  labTableRows(key: LabTableSection['rowsKey']): LabReceptionSummaryRow[] {
    return this.receptionStats?.[key] ?? [];
  }

  popularTestRows(period: 'monthly' | 'yearly'): PopularTestMetric[] {
    if (period === 'monthly')
      return this.receptionStats?.monthlyPopularTests ?? [];
    return this.receptionStats?.yearlyPopularTests ?? [];
  }

  specialOfferMonthlyRows(): SpecialOfferLabMonthlyRow[] {
    return this.specialOfferStats?.monthlyOffersByLab ?? [];
  }

  exportSpecialOffersTable(): void {
    const rows = this.specialOfferMonthlyRows();
    const headers = [
      this._localization.translate(
        'modules.admin.dashboard.specialOffers.table.labCode'
      ),
      this._localization.translate(
        'modules.admin.dashboard.specialOffers.table.labName'
      ),
      this._localization.translate(
        'modules.admin.dashboard.specialOffers.table.offerCount'
      ),
    ];
    const data = rows.map((row) => [
      row.labCodeNew,
      row.labName || '—',
      row.offerCount,
    ]);
    const title = this._localization.translate(
      'modules.admin.dashboard.specialOffers.monthlyTable'
    );
    downloadTableAsExcel(title, headers, data);
  }

  exportLabsTable(section: LabTableSection): void {
    const rows = this.labTableRows(section.rowsKey);
    const headers = [
      this._localization.translate(
        'modules.admin.dashboard.receptions.table.labCode'
      ),
      this._localization.translate(
        'modules.admin.dashboard.receptions.table.labName'
      ),
      this._localization.translate(
        'modules.admin.dashboard.receptions.table.receptionCount'
      ),
      this._localization.translate(
        'modules.admin.dashboard.receptions.table.testsCount'
      ),
    ];
    const data = rows.map((row) => [
      row.labCode,
      this.labRowDisplayName(row),
      row.receptionCount,
      row.testsCount,
    ]);
    const title = this._localization.translate(section.titleKey);
    downloadTableAsExcel(title, headers, data);
  }

  private async loadLabNames(): Promise<void> {
    if (!this.receptionStats) return;

    const codes = new Set<number>();
    const metrics = [
      this.receptionStats.dailyMostUsage,
      this.receptionStats.monthlyMostUsage,
      this.receptionStats.dailyMostSentTests,
      this.receptionStats.monthlyMostSentTests,
      this.receptionStats.dailyMostReceivedTests,
      this.receptionStats.monthlyMostReceivedTests,
    ];

    for (const metric of metrics) {
      if (metric.labCode != null) codes.add(metric.labCode);
    }
    for (const section of this.labTableSections) {
      for (const row of this.labTableRows(section.rowsKey)) {
        codes.add(row.labCode);
      }
    }

    const labs = await this._receptionsService.getLabs([...codes]);
    this.labNameMap = this._receptionsService.buildLabNameMap(labs);
  }

  get activeRate(): number {
    if (!this.stats?.totalUsers) return 0;
    return Math.round((this.stats.activeUsers / this.stats.totalUsers) * 100);
  }

  get inactiveUsers(): number {
    if (!this.stats) return 0;
    return Math.max(this.stats.totalUsers - this.stats.activeUsers, 0);
  }

  get activityRingBackground(): string {
    return `conic-gradient(#059669 calc(${this.activeRate} * 1%), #e2e8f0 0)`;
  }

  theme(themeName: DashboardTheme) {
    return dashboardThemeClasses(themeName);
  }
}

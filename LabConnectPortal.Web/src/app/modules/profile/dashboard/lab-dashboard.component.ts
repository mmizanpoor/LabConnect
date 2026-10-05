import { Component, OnInit } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthService } from '@core/services/auth/auth.service';
import { LocalizationService } from '@core/services/localization/localization.service';
import { downloadTableAsExcel } from '@core/utils/excel-export.util';
import { dashboardThemeClasses, DashboardTheme } from '@core/utils/dashboard-theme.util';
import { AgreementsService } from '../agreements/agreements.service';
import { JobPostingsService } from '../job-postings/job-postings.service';
import { ReceptionsService } from '../receptions/receptions.service';
import {
  AdminReceptionDashboardStats,
  LabReceptionSummaryRow,
} from '../receptions/receptions.types';
import { JobPostingDashboardStats } from '../job-postings/job-postings.types';
import {
  LabAgreementLabStats,
  LabAgreementStats,
} from './lab-dashboard.types';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';

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

interface ReceptionStatCard {
  labelKey: string;
  icon: string;
  theme: 'blue' | 'green' | 'violet' | 'amber' | 'slate' | 'rose';
  period: 'daily' | 'monthly';
  metric: 'usageCount' | 'testsCount';
}

interface DashboardSection {
  titleKey: string;
  icon: string;
  agreementKey: keyof LabAgreementLabStats;
  direction: 'sent' | 'received';
}

interface LabTablePeriod {
  period: 'daily' | 'monthly';
}

@Component({
  selector: 'app-lab-dashboard',
  standalone: true,
  imports: [MatIconModule, TranslocoPipe],
  templateUrl: './lab-dashboard.component.html',
  styleUrl: './lab-dashboard.component.scss',
})
export class LabDashboardComponent implements OnInit {
  readonly entity = SystemEntity.LabAgreement;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.LabAgreement);

  agreementStats: LabAgreementLabStats | null = null;
  jobPostingStats: JobPostingDashboardStats | null = null;
  receptionStats: AdminReceptionDashboardStats | null = null;
  labNameMap: Record<number, string> = {};
  loading = true;
  error = '';
  jobPostingError = '';

  readonly agreementCards: AgreementStatCard[] = [
    {
      labelKey: 'modules.profile.dashboard.activeAgreements',
      icon: 'task_alt',
      theme: 'green',
      value: 'activeCount',
    },
    {
      labelKey: 'modules.profile.dashboard.expiredAgreements',
      icon: 'event_busy',
      theme: 'slate',
      value: 'expiredCount',
    },
    {
      labelKey: 'modules.profile.dashboard.pendingAgreements',
      icon: 'hourglass_top',
      theme: 'amber',
      value: 'pendingCount',
    },
  ];

  readonly jobPostingCards: JobPostingStatCard[] = [
    {
      labelKey: 'modules.profile.dashboard.activeJobPostings',
      icon: 'work',
      theme: 'green',
      value: 'activeCount',
    },
    {
      labelKey: 'modules.profile.dashboard.expiredJobPostings',
      icon: 'event_busy',
      theme: 'slate',
      value: 'expiredCount',
    },
    {
      labelKey: 'modules.profile.dashboard.sentJobApplications',
      icon: 'send',
      theme: 'blue',
      value: 'sentApplicationsCount',
    },
  ];

  readonly receptionUsageCards: ReceptionStatCard[] = [
    {
      labelKey: 'modules.profile.dashboard.dailyUsage',
      icon: 'today',
      theme: 'blue',
      period: 'daily',
      metric: 'usageCount',
    },
    {
      labelKey: 'modules.profile.dashboard.monthlyUsage',
      icon: 'calendar_month',
      theme: 'violet',
      period: 'monthly',
      metric: 'usageCount',
    },
  ];

  readonly receptionTestsCards: ReceptionStatCard[] = [
    {
      labelKey: 'modules.profile.dashboard.dailyTests',
      icon: 'science',
      theme: 'green',
      period: 'daily',
      metric: 'testsCount',
    },
    {
      labelKey: 'modules.profile.dashboard.monthlyTests',
      icon: 'biotech',
      theme: 'amber',
      period: 'monthly',
      metric: 'testsCount',
    },
  ];

  readonly sections: DashboardSection[] = [
    {
      titleKey: 'modules.profile.dashboard.receivedAgreements',
      icon: 'inbox',
      agreementKey: 'received',
      direction: 'received',
    },
    {
      titleKey: 'modules.profile.dashboard.sentAgreements',
      icon: 'outbox',
      agreementKey: 'sent',
      direction: 'sent',
    },
  ];

  readonly labTablePeriods: LabTablePeriod[] = [{ period: 'daily' }, { period: 'monthly' }];

  constructor(
    private _agreementsService: AgreementsService,
    private _authService: AuthService,
    private _jobPostingsService: JobPostingsService,
    private _labPermission: LabPermissionService,
    private _localization: LocalizationService,
    private _receptionsService: ReceptionsService,
  ) {}

  get canViewJobPostings(): boolean {
    return this._labPermission.canView(SystemEntity.JobPosting);
  }

  get canViewAgreements(): boolean {
    return this._labPermission.canView(SystemEntity.LabAgreement);
  }

  get canViewReceptions(): boolean {
    return this._labPermission.canView(SystemEntity.Reception);
  }

  get canExportReceptions(): boolean {
    return this._labPermission.canExport(SystemEntity.Reception);
  }

  async ngOnInit(): Promise<void> {
    try {
      const profile = await this._authService.getProfile();
      const labCode = profile.labCode ?? undefined;
      const loadDirectionCards =
        !!labCode && this.canViewAgreements && this.canViewReceptions;

      const loadAgreements = loadDirectionCards
        ? this._agreementsService.getStatsForCurrentLab()
        : Promise.resolve(null);
      const loadJobPostings = this.canViewJobPostings
        ? this._jobPostingsService.getDashboardStatsForCurrentLab()
        : Promise.resolve(null);
      const loadReceptions = loadDirectionCards
        ? this._receptionsService.getAdminDashboardStats(labCode)
        : Promise.resolve(null);

      if (this.canViewJobPostings) {
        this.jobPostingStats = {
          activeCount: 0,
          expiredCount: 0,
          sentApplicationsCount: 0,
        };
      }

      const [agreementResult, jobPostingResult, receptionResult] =
        await Promise.allSettled([loadAgreements, loadJobPostings, loadReceptions]);

      if (
        this.canViewAgreements &&
        this.canViewReceptions &&
        agreementResult.status === 'fulfilled' &&
        agreementResult.value
      ) {
        this.agreementStats = agreementResult.value;
      } else if (
        this.canViewAgreements &&
        this.canViewReceptions &&
        agreementResult.status === 'rejected' &&
        !this.isAccessDenied(agreementResult.reason) &&
        !this.isMissingLabCode(agreementResult.reason)
      ) {
        this.error = this.errorMessage(agreementResult.reason);
      }

      if (
        this.canViewJobPostings &&
        jobPostingResult.status === 'fulfilled' &&
        jobPostingResult.value
      ) {
        this.jobPostingStats = jobPostingResult.value;
      } else if (
        this.canViewJobPostings &&
        jobPostingResult.status === 'rejected' &&
        !this.isAccessDenied(jobPostingResult.reason) &&
        !this.isMissingLabCode(jobPostingResult.reason)
      ) {
        this.jobPostingError = this.errorMessage(jobPostingResult.reason);
      }

      if (
        this.canViewAgreements &&
        this.canViewReceptions &&
        receptionResult.status === 'fulfilled' &&
        receptionResult.value
      ) {
        this.receptionStats = receptionResult.value;
        try {
          await this.loadLabNames();
        } catch (err) {
          if (!this.error && !this.isAccessDenied(err)) {
            this.error = this.errorMessage(err);
          }
        }
      } else if (
        this.canViewAgreements &&
        this.canViewReceptions &&
        receptionResult.status === 'rejected' &&
        !this.error &&
        !this.isAccessDenied(receptionResult.reason) &&
        !this.isMissingLabCode(receptionResult.reason)
      ) {
        this.error = this.errorMessage(receptionResult.reason);
      }
    } catch (err) {
      if (!this.isAccessDenied(err) && !this.isMissingLabCode(err)) {
        this.error = this.errorMessage(err);
      }
    } finally {
      this.loading = false;
    }
  }

  private isMissingLabCode(err: unknown): boolean {
    return this.errorMessage(err).includes('کد آزمایشگاه (LabCode) تعریف نشده است');
  }

  private isAccessDenied(err: unknown): boolean {
    const msg = this.errorMessage(err);
    if (msg.includes('دسترسی مجاز نیست')) return true;
    if (err && typeof err === 'object' && 'status' in err) {
      const status = (err as { status?: number }).status;
      if (status === 403 || status === 401) return true;
    }
    return false;
  }

  private errorMessage(err: unknown): string {
    if (err instanceof Error) return err.message;
    if (err && typeof err === 'object') {
      const e = err as { error?: { message?: string }; message?: string };
      if (typeof e.error?.message === 'string') return e.error.message;
      if (typeof e.message === 'string') return e.message;
    }
    return '';
  }

  jobPostingValue(card: JobPostingStatCard): number {
    return this.jobPostingStats?.[card.value] ?? 0;
  }

  agreementValue(section: DashboardSection, card: AgreementStatCard): number {
    return this.agreementStats?.[section.agreementKey]?.[card.value] ?? 0;
  }

  receptionValue(section: DashboardSection, card: ReceptionStatCard): number {
    const stats = this.receptionStats;
    if (!stats) return 0;

    const periodStats =
      section.direction === 'sent'
        ? card.period === 'daily'
          ? stats.sentDailyStats
          : stats.sentMonthlyStats
        : card.period === 'daily'
          ? stats.receivedDailyStats
          : stats.receivedMonthlyStats;

    return periodStats?.[card.metric] ?? 0;
  }

  partnerRows(section: DashboardSection, period: 'daily' | 'monthly'): LabReceptionSummaryRow[] {
    const stats = this.receptionStats;
    if (!stats) return [];

    if (section.direction === 'sent') {
      return period === 'daily' ? stats.dailySentLabs ?? [] : stats.monthlySentLabs ?? [];
    }

    return period === 'daily' ? stats.dailyReceivedLabs ?? [] : stats.monthlyReceivedLabs ?? [];
  }

  labTableTitleKey(section: DashboardSection, period: 'daily' | 'monthly'): string {
    const keys: Record<DashboardSection['direction'], Record<'daily' | 'monthly', string>> = {
      sent: {
        daily: 'modules.admin.dashboard.receptions.dailySentLabsTable',
        monthly: 'modules.admin.dashboard.receptions.monthlySentLabsTable',
      },
      received: {
        daily: 'modules.admin.dashboard.receptions.dailyReceivedLabsTable',
        monthly: 'modules.admin.dashboard.receptions.monthlyReceivedLabsTable',
      },
    };
    return keys[section.direction][period];
  }

  exportLabsTable(section: DashboardSection, period: 'daily' | 'monthly'): void {
    if (!this.canExportReceptions) return;
    const rows = this.partnerRows(section, period);
    const headers = [
      this._localization.translate('modules.profile.dashboard.table.labCode'),
      this._localization.translate('modules.profile.dashboard.table.labName'),
      this._localization.translate('modules.profile.dashboard.table.receptionCount'),
      this._localization.translate('modules.profile.dashboard.table.testsCount'),
    ];
    const data = rows.map((row) => [
      row.labCode,
      this.labDisplayName(row),
      row.receptionCount,
      row.testsCount,
    ]);
    const title = this._localization.translate(this.labTableTitleKey(section, period));
    downloadTableAsExcel(title, headers, data);
  }

  labDisplayName(row: LabReceptionSummaryRow): string {
    return this._receptionsService.resolveLabName(this.labNameMap, row.labCode);
  }

  theme(themeName: DashboardTheme) {
    return dashboardThemeClasses(themeName);
  }

  private async loadLabNames(): Promise<void> {
    if (!this.receptionStats) return;

    const codes = new Set<number>();
    for (const row of [
      ...(this.receptionStats.dailySentLabs ?? []),
      ...(this.receptionStats.monthlySentLabs ?? []),
      ...(this.receptionStats.dailyReceivedLabs ?? []),
      ...(this.receptionStats.monthlyReceivedLabs ?? []),
    ]) {
      codes.add(row.labCode);
    }

    const labs = await this._receptionsService.getLabs([...codes]);
    this.labNameMap = this._receptionsService.buildLabNameMap(labs);
  }
}

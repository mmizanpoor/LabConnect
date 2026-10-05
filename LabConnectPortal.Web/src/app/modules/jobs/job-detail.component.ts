import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { BreadcrumbService } from '@core/services/breadcrumb/breadcrumb.service';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { isResumeSectionComplete } from '../profile/resume/resume-completeness';
import { ResumeService } from '../profile/resume/resume.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import {
  ContractType,
  DegreeLevel,
  getEnumName,
} from '../profile/resume/resume.types';
import { JobApplicationsService, PublicJobsService } from './jobs.service';
import { JobApplicationStatus, PublicJobPostingDetailDto } from './jobs.types';

interface ParsedJobDescription {
  intro: string[];
  responsibilities: string[];
  requirements: string[];
  remaining: string;
}

const BOOKMARK_STORAGE_KEY = 'labconnect.savedJobs';

@Component({
  selector: 'app-job-detail',
  standalone: true,
  imports: [RouterLink, MatIconModule, TranslocoPipe, BaseButtonComponent],
  templateUrl: './job-detail.component.html',
})
export class JobDetailComponent implements OnInit {
  readonly entity = SystemEntity.JobPosting;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.JobPosting);
  loading = true;
  error = '';
  job: PublicJobPostingDetailDto | null = null;
  parsedDescription: ParsedJobDescription = {
    intro: [],
    responsibilities: [],
    requirements: [],
    remaining: '',
  };
  applyLoading = false;
  applyError = '';
  applySuccess = '';
  resumeComplete = false;
  applicantName = '';
  applicantMobile = '';
  applicationStatus: JobApplicationStatus | null = null;
  isUser = false;
  isBookmarked = false;

  constructor(
    private _route: ActivatedRoute,
    private _router: Router,
    private _publicJobsService: PublicJobsService,
    private _jobApplicationsService: JobApplicationsService,
    private _resumeService: ResumeService,
    private _breadcrumb: BreadcrumbService,
    readonly authUtils: AuthUtils,
    private _localization: LocalizationService
  ) {}

  get showApplySidebar(): boolean {
    return !this.authUtils.isAuthenticated || this.authUtils.isUser();
  }

  get locationLabel(): string {
    if (!this.job) return '';
    const parts = [this.job.provinceName, this.job.locationName].filter(
      Boolean
    );
    return parts.join('، ');
  }

  get experienceLabel(): string {
    if (!this.job) return '';
    const years = this.job.minimumWorkExperienceYears;
    if (years <= 0) {
      return this._localization.translate('modules.jobs.experience.none');
    }
    if (years < 3) {
      return this._localization.translate(
        'modules.jobs.experience.lessThanThreeYears'
      );
    }
    return this._localization.translate('modules.jobs.experience.years', {
      count: years,
    });
  }

  ngOnInit(): void {
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    const id = this._route.snapshot.paramMap.get('id');
    if (!id) {
      this.error = this._localization.translate('modules.jobs.errors.notFound');
      this.loading = false;
      return;
    }

    try {
      const result = await this._publicJobsService.getById(id);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.jobs.errors.notFound')
        );
      }
      this.job = result.data;
      this.parsedDescription = this.parseDescription(
        result.data.jobDescription
      );
      this.isBookmarked = this.readBookmarkState(id);
      this._breadcrumb.setDynamicLabel(
        this.buildJobBreadcrumbLabel(result.data)
      );
      await this.loadApplyContext(id);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.jobs.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  toggleBookmark(): void {
    if (!this.job) return;

    const saved = this.readSavedJobIds();
    const jobId = this.job.jobPostingId;
    const next = saved.includes(jobId)
      ? saved.filter((id) => id !== jobId)
      : [...saved, jobId];
    localStorage.setItem(BOOKMARK_STORAGE_KEY, JSON.stringify(next));
    this.isBookmarked = next.includes(jobId);
  }

  private readBookmarkState(jobId: string): boolean {
    return this.readSavedJobIds().includes(jobId);
  }

  private readSavedJobIds(): string[] {
    try {
      const raw = localStorage.getItem(BOOKMARK_STORAGE_KEY);
      if (!raw) return [];
      const parsed = JSON.parse(raw);
      return Array.isArray(parsed)
        ? parsed.filter((id): id is string => typeof id === 'string')
        : [];
    } catch {
      return [];
    }
  }

  private buildJobBreadcrumbLabel(job: PublicJobPostingDetailDto): string {
    if (job.locationName) {
      return `${job.jobCategoryName} — ${job.locationName}`;
    }
    return job.jobCategoryName;
  }

  private parseDescription(text: string): ParsedJobDescription {
    const empty: ParsedJobDescription = {
      intro: [],
      responsibilities: [],
      requirements: [],
      remaining: '',
    };
    if (!text?.trim()) return empty;

    const normalized = text.replace(/\r\n/g, '\n').trim();
    const responsibilityMarkers = [
      'مسئولیت\u200cها:',
      'مسئولیت ها:',
      'مسئولیتها:',
    ];
    const requirementMarkers = [
      'نیازمندی\u200cها:',
      'نیازمندی ها:',
      'نیازمندیها:',
    ];

    const responsibilityIndex = this.findSectionIndex(
      normalized,
      responsibilityMarkers
    );
    const requirementIndex = this.findSectionIndex(
      normalized,
      requirementMarkers
    );

    if (responsibilityIndex === -1 && requirementIndex === -1) {
      return { ...empty, remaining: normalized };
    }

    const introEnd = [responsibilityIndex, requirementIndex]
      .filter((index) => index >= 0)
      .sort((a, b) => a - b)[0];

    const intro = this.toParagraphs(normalized.slice(0, introEnd).trim());

    let responsibilities: string[] = [];
    let requirements: string[] = [];

    if (responsibilityIndex >= 0) {
      const end =
        requirementIndex >= 0 && requirementIndex > responsibilityIndex
          ? requirementIndex
          : normalized.length;
      responsibilities = this.extractSectionItems(
        normalized.slice(responsibilityIndex, end),
        responsibilityMarkers
      );
    }

    if (requirementIndex >= 0) {
      requirements = this.extractSectionItems(
        normalized.slice(requirementIndex),
        requirementMarkers
      );
    }

    return { intro, responsibilities, requirements, remaining: '' };
  }

  private findSectionIndex(text: string, markers: string[]): number {
    let found = -1;
    for (const marker of markers) {
      const index = text.indexOf(marker);
      if (index >= 0 && (found === -1 || index < found)) {
        found = index;
      }
    }
    return found;
  }

  private extractSectionItems(section: string, markers: string[]): string[] {
    let content = section.trim();
    for (const marker of markers) {
      if (content.startsWith(marker)) {
        content = content.slice(marker.length).trim();
        break;
      }
    }
    return this.toBulletItems(content);
  }

  private toParagraphs(text: string): string[] {
    return text
      .split(/\n{2,}/)
      .map((part) => part.replace(/\n+/g, ' ').trim())
      .filter(Boolean);
  }

  private toBulletItems(text: string): string[] {
    return text
      .split('\n')
      .map((line) => line.trim())
      .filter(Boolean)
      .map((line) => line.replace(/^[-•*–—]\s*/, '').trim())
      .filter(Boolean);
  }

  private async loadApplyContext(jobPostingId: string): Promise<void> {
    if (!this.authUtils.isAuthenticated) return;

    this.isUser = this.authUtils.userType === 'User';
    if (!this.isUser) return;

    try {
      const resume = await this._resumeService.getMyResume();
      this.applicantName = `${resume.firstName} ${resume.lastName}`.trim();
      this.applicantMobile = resume.mobileNumber;
      this.resumeComplete =
        resume.isCompleteForApplication === true || isResumeSectionComplete('basicInfo', resume);

      const appsResult = await this._jobApplicationsService.getMyApplications();
      if (appsResult.success && appsResult.data) {
        const existing = appsResult.data.find(
          (a) => a.jobPostingId === jobPostingId
        );
        this.applicationStatus = existing?.status ?? null;
      }
    } catch {
      // Sidebar falls back to safe defaults when resume/apps cannot be loaded.
    }
  }

  async apply(): Promise<void> {
    if (!this.job) return;

    if (!this.authUtils.isAuthenticated) {
      void this._router.navigate(['/auth/login'], {
        queryParams: { returnUrl: `/jobs/${this.job.jobPostingId}` },
      });
      return;
    }

    if (!this.isUser) {
      return;
    }

    if (!this.resumeComplete) {
      this.applyError = this._localization.translate(
        'modules.jobs.apply.incompleteResume'
      );
      return;
    }

    if (this.applicationStatus) {
      return;
    }

    this.applyLoading = true;
    this.applyError = '';
    this.applySuccess = '';
    try {
      const result = await this._jobApplicationsService.submit(
        this.job.jobPostingId
      );
      if (!result.success) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.jobs.apply.submitFailed')
        );
      }
      this.applicationStatus = 'Pending';
      this.applySuccess = this._localization.translate(
        'modules.jobs.apply.success'
      );
    } catch (e: unknown) {
      this.applyError =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.jobs.apply.submitFailed');
    } finally {
      this.applyLoading = false;
    }
  }

  goToLogin(): void {
    if (!this.job) return;
    void this._router.navigate(['/auth/login'], {
      queryParams: { returnUrl: `/jobs/${this.job.jobPostingId}` },
    });
  }

  contractLabel(type: ContractType): string {
    return this._localization.translate(
      `modules.profile.resume.enums.contractType.${type}`
    );
  }

  contractLabels(types: ContractType[]): string {
    return types.map((t) => this.contractLabel(t)).join('، ');
  }

  degreeLabel(value: DegreeLevel | string): string {
    const name = getEnumName(DegreeLevel, value);
    return this._localization.translate(
      `modules.profile.resume.enums.degreeLevel.${name}`
    );
  }

  statusLabel(status: JobApplicationStatus): string {
    return this._localization.translate(`enum.jobApplicationStatus.${status}`);
  }
}

import { Component, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { BasePagingComponent } from '@modules/base/components/base-paging/base-paging.component';
import { CenterProfileService } from '../center-profile/center-profile.service';
import { CenterProfileDto } from '../center-profile/center-profile.types';
import { JobPostingsService } from './job-postings.service';
import { JobPostingListItemDto, JobPostingStatus } from './job-postings.types';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { JobPostingsColDef, JobPostingAction } from './job-postings.coldef';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';
import { AuthUtils } from '@core/services/auth/auth.utils';

@Component({
  selector: 'app-job-postings-list',
  standalone: true,
  imports: [
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    TranslocoPipe,
    BaseButtonComponent,
    BasePagingComponent,
    BaseGridComponent,
  ],
  templateUrl: './job-postings-list.component.html',
  styleUrl: './job-postings-list.component.scss',
})
export class JobPostingsListComponent implements OnInit {
  readonly entity = SystemEntity.JobPosting;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.JobPosting);

  loading = false;
  error = '';
  rows: JobPostingListItemDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<JobPostingListItemDto[]>([]);
  totalCount = 0;
  page = 1;
  pageSize = 20;
  centerProfile: CenterProfileDto | null = null;

  constructor(
    private _jobPostingsService: JobPostingsService,
    private _centerProfileService: CenterProfileService,
    private _localization: LocalizationService,
    private _router: Router,
    private _colDef: JobPostingsColDef,
    private _labPermission: LabPermissionService,
    private _authUtils: AuthUtils,
  ) {}

  get canManageJobPostings(): boolean {
    return this.centerProfile?.isApproved ?? false;
  }

  get showProfileStatusMessage(): boolean {
    return this._authUtils.canManageCenterProfile();
  }

  get canCreate(): boolean {
    return this.canManageJobPostings && this._labPermission.can(this.entity, 'create');
  }

  get canView(): boolean {
    return this._labPermission.canView(this.entity);
  }

  get canUpdate(): boolean {
    return this.canManageJobPostings && this._labPermission.can(this.entity, 'update');
  }

  get canDelete(): boolean {
    return this.canManageJobPostings && this._labPermission.can(this.entity, 'delete');
  }

  get profileBannerKey(): string {
    if (!this.centerProfile) {
      return 'modules.profile.jobPostings.profileNotApprovedBanner';
    }
    if (this.centerProfile.rejectionReason && !this.centerProfile.isComplete) {
      return 'modules.profile.jobPostings.profileRejectedBanner';
    }
    if (this.centerProfile.isComplete) {
      return 'modules.profile.jobPostings.profileAwaitingApproval';
    }
    return 'modules.profile.jobPostings.profileNotApprovedBanner';
  }

  ngOnInit(): void {
    this.colDef = this._colDef.get(
      (s) => this.statusClass(s),
      (s) => this.statusLabel(s),
      () => this.canView,
      () => this.canUpdate,
      () => this.canDelete,
    );
    this._colDef.actionClicked.subscribe((evt: JobPostingAction) => {
      if (!evt?.row) return;
      if (evt.type === 'edit') this.edit(evt.row);
      if (evt.type === 'publish') void this.publish(evt.row);
      if (evt.type === 'delete') void this.deletePosting(evt.row);
      if (evt.type === 'close') void this.close(evt.row);
      if (evt.type === 'reopen') void this.reopen(evt.row);
      if (evt.type === 'applications') {
        void this._router.navigate(['/profile/job-postings', evt.row.jobPostingId, 'applications']);
      }
    });

    void this.loadCenterProfile();
    void this.load();
  }

  private async loadCenterProfile(): Promise<void> {
    const result = await this._centerProfileService.getMyProfile();
    if (result.success && result.data) {
      this.centerProfile = result.data;
    }
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const result = await this._jobPostingsService.getMyPostings({
        page: this.page,
        pageSize: this.pageSize,
      });
      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate('modules.profile.jobPostings.errors.loadFailed'));
      }
      this.rows = result.data.items;
      this.totalCount = result.data.totalCount;
      this.list$.next(this.rows);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.jobPostings.errors.loadFailed');
      this.rows = [];
      this.list$.next([]);
    } finally {
      this.loading = false;
    }
  }

  onPageChange(page: number): void {
    this.page = page;
    void this.load();
  }

  statusLabel(status: JobPostingStatus): string {
    return this._localization.translate(`enum.jobPostingStatus.${status}`);
  }

  statusClass(status: JobPostingStatus): string {
    switch (status) {
      case 'Active':
        return 'bg-green-100 text-green-800';
      case 'Closed':
        return 'bg-gray-100 text-gray-700';
      default:
        return 'bg-amber-100 text-amber-800';
    }
  }

  async publish(row: JobPostingListItemDto): Promise<void> {
    const result = await this._jobPostingsService.publish(row.jobPostingId);
    if (!result.success) {
      this.error = result.message ?? this._localization.translate('modules.profile.jobPostings.errors.publishFailed');
      return;
    }
    await this.load();
  }

  async close(row: JobPostingListItemDto): Promise<void> {
    const result = await this._jobPostingsService.close(row.jobPostingId);
    if (!result.success) {
      this.error = result.message ?? this._localization.translate('modules.profile.jobPostings.errors.closeFailed');
      return;
    }
    await this.load();
  }

  async reopen(row: JobPostingListItemDto): Promise<void> {
    const result = await this._jobPostingsService.reopen(row.jobPostingId);
    if (!result.success) {
      this.error = result.message ?? this._localization.translate('modules.profile.jobPostings.errors.reopenFailed');
      return;
    }
    await this.load();
  }

  async deletePosting(row: JobPostingListItemDto): Promise<void> {
    if (!confirm(this._localization.translate('modules.profile.jobPostings.confirmDelete'))) return;
    const result = await this._jobPostingsService.delete(row.jobPostingId);
    if (!result.success) {
      this.error = result.message ?? this._localization.translate('modules.profile.jobPostings.errors.deleteFailed');
      return;
    }
    await this.load();
  }

  edit(row: JobPostingListItemDto): void {
    void this._router.navigate(['/profile/job-postings', row.jobPostingId]);
  }
}

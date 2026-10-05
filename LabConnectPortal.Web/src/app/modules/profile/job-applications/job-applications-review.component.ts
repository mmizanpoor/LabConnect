import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { BasePagingComponent } from '@modules/base/components/base-paging/base-paging.component';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { JobApplicationsService } from '../../jobs/jobs.service';
import { JobApplicationDto, JobApplicationStatus } from '../../jobs/jobs.types';
import { JobApplicationsColDef, JobApplicationAction } from './job-applications.coldef';
import {
  JobApplicationApproveDialogComponent,
  JobApplicationApproveDialogData,
} from './job-application-approve-dialog.component';
import {
  JobApplicationRejectDialogComponent,
  JobApplicationRejectDialogData,
} from './job-application-reject-dialog.component';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';

@Component({
  selector: 'app-job-applications-review',
  standalone: true,
  imports: [
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatDialogModule,
    TranslocoPipe,
    BasePagingComponent,
    BaseGridComponent,
  ],
  templateUrl: './job-applications-review.component.html',
})
export class JobApplicationsReviewComponent implements OnInit {
  readonly entity = SystemEntity.JobPosting;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.JobPosting);

  loading = false;
  error = '';
  rows: JobApplicationDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<JobApplicationDto[]>([]);
  totalCount = 0;
  page = 1;
  pageSize = 20;
  jobPostingId = '';

  constructor(
    private _route: ActivatedRoute,
    private _jobApplicationsService: JobApplicationsService,
    private _localization: LocalizationService,
    private _dialog: MatDialog,
    private _colDefService: JobApplicationsColDef,
    private _labPermission: LabPermissionService,
  ) {}

  get canUpdate(): boolean {
    return this._labPermission.can(this.entity, 'update');
  }

  ngOnInit(): void {
    this.jobPostingId = this._route.snapshot.paramMap.get('id') ?? '';
    this.colDef = this._colDefService.get(
      (s) => this.statusClass(s),
      (s) => this.statusLabel(s),
      this.canUpdate,
    );
    this._colDefService.actionClicked.subscribe((evt: JobApplicationAction) => {
      if (!evt?.row) return;
      if (evt.type === 'approve') this.openApprove(evt.row);
      if (evt.type === 'reject') this.openReject(evt.row);
    });
    void this.load();
  }

  async load(): Promise<void> {
    if (!this.jobPostingId) return;
    this.loading = true;
    this.error = '';
    try {
      const result = await this._jobApplicationsService.getForPosting({
        jobPostingId: this.jobPostingId,
        page: this.page,
        pageSize: this.pageSize,
      });
      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate('modules.profile.jobApplications.errors.loadFailed'));
      }
      this.rows = result.data.items;
      this.totalCount = result.data.totalCount;
      this.list$.next(this.rows);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.jobApplications.errors.loadFailed');
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

  statusLabel(status: JobApplicationStatus): string {
    return this._localization.translate(`enum.jobApplicationStatus.${status}`);
  }

  statusClass(status: JobApplicationStatus): string {
    switch (status) {
      case 'Approved':
        return 'bg-green-100 text-green-800';
      case 'Rejected':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-amber-100 text-amber-800';
    }
  }

  openApprove(row: JobApplicationDto): void {
    const dialogRef = this._dialog.open(JobApplicationApproveDialogComponent, {
      width: '480px',
      data: { applicantName: `${row.applicantFirstName} ${row.applicantLastName}`.trim() } satisfies JobApplicationApproveDialogData,
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        void this.approve(row.jobApplicationId, result);
      }
    });
  }

  openReject(row: JobApplicationDto): void {
    const dialogRef = this._dialog.open(JobApplicationRejectDialogComponent, {
      width: '480px',
      data: { applicantName: `${row.applicantFirstName} ${row.applicantLastName}`.trim() } satisfies JobApplicationRejectDialogData,
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        void this.reject(row.jobApplicationId, result.reviewNotes);
      }
    });
  }

  private async approve(
    jobApplicationId: string,
    data: { reviewNotes: string; visitScheduledAt?: string | null; visitLocation?: string | null },
  ): Promise<void> {
    const result = await this._jobApplicationsService.approve({
      jobApplicationId,
      reviewNotes: data.reviewNotes,
      visitScheduledAt: data.visitScheduledAt,
      visitLocation: data.visitLocation,
    });
    if (!result.success) {
      this.error = result.message ?? this._localization.translate('modules.profile.jobApplications.errors.approveFailed');
      return;
    }
    await this.load();
  }

  private async reject(jobApplicationId: string, reviewNotes: string): Promise<void> {
    const result = await this._jobApplicationsService.reject({ jobApplicationId, reviewNotes });
    if (!result.success) {
      this.error = result.message ?? this._localization.translate('modules.profile.jobApplications.errors.rejectFailed');
      return;
    }
    await this.load();
  }
}

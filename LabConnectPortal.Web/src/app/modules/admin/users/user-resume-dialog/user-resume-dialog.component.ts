import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { ProductResumeApplicationStatus } from '../../../products/products.types';
import {
  isPendingResumeApplication,
  resumeApplicationStatusKey,
} from '../../../profile/product-resume-applications/product-resume-application-status';
import {
  ContractType,
  DegreeLevel,
  EmploymentStatus,
  Gender,
  getEnumName,
  LanguageProficiencyLevel,
  MaritalStatus,
  MilitaryServiceStatus,
  PersianMonth,
  ReferenceItemDto,
  ResumeDto,
  SeniorityLevel,
} from '../../../profile/resume/resume.types';
import { ProductResumeApplicationsService } from '../../../profile/product-resume-applications/product-resume-applications.service';
import { ResumeService } from '../../../profile/resume/resume.service';
import { UsersService } from '../users.service';

export interface UserResumeDialogData {
  userId: string;
  fullName: string;
  productResumeApplicationId?: string;
  status?: ProductResumeApplicationStatus;
  reviewNotes?: string;
}

@Component({
  selector: 'app-user-resume-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseDialogComponent,
  ],
  templateUrl: './user-resume-dialog.component.html',
})
export class UserResumeDialogComponent implements OnInit, OnDestroy {
  loading = true;
  error = '';
  downloadError = '';
  resume: ResumeDto | null = null;
  photoUrl: string | null = null;
  downloadingResume = false;
  reviewing = false;
  reviewError = '';
  currentStatus: ProductResumeApplicationStatus | undefined;
  currentReviewNotes = '';
  reviewNotes = new FormControl('', { nonNullable: true });

  readonly EmploymentStatus = EmploymentStatus;
  readonly Gender = Gender;
  readonly MaritalStatus = MaritalStatus;
  readonly MilitaryServiceStatus = MilitaryServiceStatus;
  readonly LanguageProficiencyLevel = LanguageProficiencyLevel;
  readonly DegreeLevel = DegreeLevel;

  provinces: ReferenceItemDto[] = [];
  jobCategories: ReferenceItemDto[] = [];
  salaryRanges: ReferenceItemDto[] = [];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: UserResumeDialogData,
    private _dialogRef: MatDialogRef<UserResumeDialogComponent>,
    private _usersService: UsersService,
    private _productResumeApplications: ProductResumeApplicationsService,
    private _resumeService: ResumeService,
    private _localization: LocalizationService,
  ) {
    this.currentStatus = data.status;
    this.currentReviewNotes = data.reviewNotes ?? '';
    this.reviewNotes.setValue(this.currentReviewNotes);
  }

  get isProductApplication(): boolean {
    return !!this.data.productResumeApplicationId;
  }

  get canReview(): boolean {
    return this.isProductApplication && isPendingResumeApplication(this.currentStatus);
  }

  get statusLabelKey(): string {
    return `modules.profile.productResumeApplications.status.${resumeApplicationStatusKey(this.currentStatus)}`;
  }

  ngOnInit(): void {
    void this.load();
  }

  ngOnDestroy(): void {
    this.revokePhotoUrl();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const resumeResult = this.isProductApplication
        ? await this._productResumeApplications.getApplicantResume(this.data.productResumeApplicationId!)
        : await this._usersService.getUserResume(this.data.userId);

      if (!resumeResult.success || !resumeResult.data) {
        this.error =
          resumeResult.message ??
          this._localization.translate('modules.admin.users.resumeDialog.loadFailed');
        return;
      }

      this.resume = resumeResult.data;

      try {
        const refData = await this._resumeService.loadReferenceData();
        this.provinces = refData.provinces;
        this.jobCategories = refData.jobCategories;
        this.salaryRanges = refData.salaryRanges;
      } catch {
        // Reference labels are optional; resume content should still display.
      }

      if (this.resume.basicInfo.hasProfilePhoto) {
        await this.loadPhoto();
      }
    } catch {
      this.error = this._localization.translate('modules.admin.users.resumeDialog.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  enumLabel(group: string, enumObj: Record<string, string | number>, value: unknown): string {
    const key = getEnumName(enumObj, value as number | string | null | undefined);
    return key
      ? this._localization.translate(`modules.profile.resume.enums.${group}.${key}`)
      : this._localization.translate('modules.profile.resume.empty');
  }

  monthLabel(month: PersianMonth | null): string {
    if (month == null) return '';
    return this.enumLabel('persianMonth', PersianMonth, month);
  }

  workPeriod(item: ResumeDto['workExperiences'][number]): string {
    const start = [this.monthLabel(item.startMonth), item.startYear].filter(Boolean).join(' ');
    if (item.isCurrentlyEmployed) {
      return `${start} — ${this._localization.translate('modules.profile.resume.workExperience.current')}`;
    }
    const end = [this.monthLabel(item.endMonth), item.endYear].filter(Boolean).join(' ');
    return [start, end].filter(Boolean).join(' — ');
  }

  educationPeriod(item: ResumeDto['educations'][number]): string {
    const start = item.startYear ? String(item.startYear) : '';
    if (item.isCurrentlyStudying) {
      return start
        ? `${start} — ${this._localization.translate('modules.profile.resume.education.currentlyStudying')}`
        : this._localization.translate('modules.profile.resume.education.currentlyStudying');
    }
    const end = item.endYear ? String(item.endYear) : '';
    return [start, end].filter(Boolean).join(' — ');
  }

  provinceNames(ids: number[] | undefined): string {
    return (ids ?? [])
      .map((id) => this.provinces.find((p) => p.id === id)?.name ?? String(id))
      .join('، ');
  }

  categoryNames(ids: number[] | undefined): string {
    return (ids ?? [])
      .map((id) => this.jobCategories.find((c) => c.id === id)?.name ?? String(id))
      .join('، ');
  }

  salaryName(id: number | null | undefined): string {
    if (id == null) return '';
    return this.salaryRanges.find((s) => s.id === id)?.name ?? String(id);
  }

  seniorityLabels(levels: SeniorityLevel[] | undefined): string {
    return (levels ?? [])
      .map((level) => this.enumLabel('seniorityLevel', SeniorityLevel, level))
      .filter(Boolean)
      .join('، ');
  }

  contractLabels(types: ContractType[] | undefined): string {
    return (types ?? [])
      .map((type) => this.enumLabel('contractType', ContractType, type))
      .filter(Boolean)
      .join('، ');
  }

  async review(approved: boolean): Promise<void> {
    if (!this.data.productResumeApplicationId || this.reviewing) return;
    const notes = this.reviewNotes.value.trim();
    if (!approved && !notes) {
      this.reviewError = this._localization.translate(
        'modules.profile.productResumeApplications.review.reasonRequired',
      );
      return;
    }

    this.reviewing = true;
    this.reviewError = '';
    try {
      const result = await this._productResumeApplications.review({
        productResumeApplicationId: this.data.productResumeApplicationId,
        approved,
        reviewNotes: notes,
      });
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.profile.productResumeApplications.review.failed'),
        );
      }
      this.currentStatus = result.data.status;
      this.currentReviewNotes = result.data.reviewNotes ?? notes;
      this._dialogRef.close({ reviewed: true, application: result.data });
    } catch (e: unknown) {
      this.reviewError =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.productResumeApplications.review.failed');
    } finally {
      this.reviewing = false;
    }
  }

  async downloadResumeFile(): Promise<void> {
    if (!this.resume?.resumeFile || this.downloadingResume) return;

    this.downloadingResume = true;
    this.downloadError = '';
    try {
      const blob = this.isProductApplication
        ? await this._productResumeApplications.getApplicantResumeFile(this.data.productResumeApplicationId!)
        : await this._usersService.getUserResumeFile(this.data.userId);
      const url = URL.createObjectURL(blob);
      const anchor = document.createElement('a');
      anchor.href = url;
      anchor.download = this.resume.resumeFile.fileName || 'resume';
      anchor.click();
      URL.revokeObjectURL(url);
    } catch (e: unknown) {
      const status =
        e instanceof HttpErrorResponse ? e.status : (e as { status?: number })?.status;
      this.downloadError =
        status === 404
          ? this._localization.translate('modules.admin.users.resumeDialog.fileNotFound')
          : this._localization.translate('modules.admin.users.resumeDialog.downloadFailed');
    } finally {
      this.downloadingResume = false;
    }
  }

  private async loadPhoto(): Promise<void> {
    try {
      const blob = this.isProductApplication
        ? await this._productResumeApplications.getApplicantResumePhoto(this.data.productResumeApplicationId!)
        : await this._usersService.getUserResumePhoto(this.data.userId);
      this.revokePhotoUrl();
      this.photoUrl = URL.createObjectURL(blob);
    } catch {
      this.photoUrl = null;
    }
  }

  private revokePhotoUrl(): void {
    if (this.photoUrl) {
      URL.revokeObjectURL(this.photoUrl);
      this.photoUrl = null;
    }
  }
}

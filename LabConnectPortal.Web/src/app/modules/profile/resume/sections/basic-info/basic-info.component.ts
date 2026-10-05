import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent, BaseFormSelectOption } from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseImageUploadComponent } from '@modules/base/components/base-image-upload/base-image-upload.component';
import { isResumeSectionComplete } from '../../resume-completeness';
import {
  DegreeLevel,
  EMPLOYMENT_STATUSES,
  EmploymentStatus,
  ResumeDto,
  ResumeSectionAnchor,
  enumTranslateKey,
  resumeEnumOptions,
} from '../../resume.types';
import { ResumeService } from '../../resume.service';
import { ResumeInfoItemComponent } from '../resume-info-item/resume-info-item.component';
import { ResumeSectionCardComponent } from '../resume-section-card/resume-section-card.component';

@Component({
  selector: 'app-resume-basic-info',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BaseImageUploadComponent,
    ResumeSectionCardComponent,
    ResumeInfoItemComponent,
  ],
  templateUrl: './basic-info.component.html',
})
export class BasicInfoComponent implements OnChanges {
  @Input({ required: true }) resume!: ResumeDto;
  @Output() saved = new EventEmitter<ResumeDto>();
  @Output() navigateToSection = new EventEmitter<ResumeSectionAnchor>();

  editing = false;
  saving = false;
  uploading = false;
  error = '';
  photoUrl: string | null = null;
  readonly enumTranslateKey = enumTranslateKey;
  readonly DegreeLevel = DegreeLevel;
  readonly EmploymentStatus = EmploymentStatus;

  form = new FormGroup({
    firstName: new FormControl('', Validators.required),
    lastName: new FormControl('', Validators.required),
    jobTitle: new FormControl(''),
    employmentStatus: new FormControl<EmploymentStatus | null>(null),
  });

  constructor(
    private _resumeService: ResumeService,
    private _localization: LocalizationService,
  ) {}

  get isComplete(): boolean {
    return isResumeSectionComplete('basicInfo', this.resume);
  }

  get employmentStatusOptions(): BaseFormSelectOption[] {
    return resumeEnumOptions(
      (key) => this._localization.translate(key),
      'employmentStatus',
      EmploymentStatus,
      EMPLOYMENT_STATUSES,
    );
  }

  get latestEducation() {
    const latest = [...this.resume.educations].sort(
      (a, b) => (b.startYear ?? 0) - (a.startYear ?? 0),
    )[0];
    if (!latest) return null;
    return {
      degreeKey: enumTranslateKey('degreeLevel', DegreeLevel, latest.degreeLevel),
      field: latest.fieldOfStudy,
    };
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['resume']) {
      this.patchForm();
      void this.loadPhoto();
    }
  }

  startEdit(): void {
    this.editing = true;
    this.error = '';
    this.patchForm();
  }

  cancel(): void {
    this.editing = false;
    this.error = '';
    this.patchForm();
  }

  scrollToSection(section: ResumeSectionAnchor): void {
    this.navigateToSection.emit(section);
  }

  async save(): Promise<void> {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.saving = true;
    this.error = '';
    try {
      const value = this.form.getRawValue();
      const updated = await this._resumeService.updateBasicInfo({
        firstName: value.firstName ?? '',
        lastName: value.lastName ?? '',
        jobTitle: value.jobTitle ?? '',
        employmentStatus: value.employmentStatus ?? null,
      });
      this.editing = false;
      this.saved.emit(updated);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.resume.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }

  async onPhotoSelected(file: File): Promise<void> {
    this.uploading = true;
    this.error = '';
    try {
      const updated = await this._resumeService.uploadPhoto(file);
      this.saved.emit(updated);
      await this.loadPhoto();
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.resume.errors.uploadFailed');
    } finally {
      this.uploading = false;
    }
  }

  private patchForm(): void {
    this.form.patchValue({
      firstName: this.resume.firstName,
      lastName: this.resume.lastName,
      jobTitle: this.resume.basicInfo.jobTitle,
      employmentStatus: this.resume.basicInfo.employmentStatus,
    });
  }

  private async loadPhoto(): Promise<void> {
    if (this.photoUrl) {
      URL.revokeObjectURL(this.photoUrl);
      this.photoUrl = null;
    }
    if (!this.resume.basicInfo.hasProfilePhoto) return;
    try {
      const blob = await this._resumeService.getPhotoBlob();
      this.photoUrl = URL.createObjectURL(blob);
    } catch {
      this.photoUrl = null;
    }
  }
}

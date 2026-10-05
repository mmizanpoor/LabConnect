import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent, BaseFormSelectOption } from '@modules/base/components/base-form-field/base-form-field.component';
import { isResumeSectionComplete } from '../../resume-completeness';
import {
  GENDERS,
  Gender,
  MARITAL_STATUSES,
  MILITARY_STATUSES,
  MaritalStatus,
  MilitaryServiceStatus,
  ReferenceItemDto,
  ResumeDto,
  enumTranslateKey,
  resumeEnumOptions,
} from '../../resume.types';
import { ResumeService } from '../../resume.service';
import { ResumeInfoItemComponent } from '../resume-info-item/resume-info-item.component';
import { ResumeSectionCardComponent } from '../resume-section-card/resume-section-card.component';

@Component({
  selector: 'app-resume-personal-info',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    ResumeSectionCardComponent,
    ResumeInfoItemComponent,
  ],
  templateUrl: './personal-info.component.html',
})
export class PersonalInfoComponent implements OnChanges {
  @Input({ required: true }) resume!: ResumeDto;
  @Input({ required: true }) provinces: ReferenceItemDto[] = [];
  @Output() saved = new EventEmitter<ResumeDto>();

  editing = false;
  saving = false;
  error = '';
  readonly Gender = Gender;
  readonly MaritalStatus = MaritalStatus;
  readonly MilitaryServiceStatus = MilitaryServiceStatus;
  readonly enumTranslateKey = enumTranslateKey;

  form = new FormGroup({
    email: new FormControl(''),
    mobilePhone: new FormControl(''),
    provinceId: new FormControl<number | null>(null),
    address: new FormControl(''),
    maritalStatus: new FormControl<MaritalStatus | null>(null),
    birthYear: new FormControl<number | null>(null),
    gender: new FormControl<Gender | null>(null),
    militaryServiceStatus: new FormControl<MilitaryServiceStatus | null>(null),
  });

  constructor(
    private _resumeService: ResumeService,
    private _localization: LocalizationService,
  ) {}

  get isComplete(): boolean {
    return isResumeSectionComplete('personalInfo', this.resume);
  }

  get showMilitary(): boolean {
    return this.form.controls.gender.value === Gender.Male;
  }

  get provinceOptions(): BaseFormSelectOption[] {
    return this.provinces.map((province) => ({ value: province.id, label: province.name }));
  }

  get maritalStatusOptions(): BaseFormSelectOption[] {
    return resumeEnumOptions(
      (key) => this._localization.translate(key),
      'maritalStatus',
      MaritalStatus,
      MARITAL_STATUSES,
    );
  }

  get genderOptions(): BaseFormSelectOption[] {
    return resumeEnumOptions(
      (key) => this._localization.translate(key),
      'gender',
      Gender,
      GENDERS,
    );
  }

  get militaryStatusOptions(): BaseFormSelectOption[] {
    return resumeEnumOptions(
      (key) => this._localization.translate(key),
      'militaryStatus',
      MilitaryServiceStatus,
      MILITARY_STATUSES,
    );
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['resume']) this.patchForm();
  }

  startEdit(): void {
    this.editing = true;
    this.error = '';
    this.patchForm();
  }

  cancel(): void {
    this.editing = false;
    this.patchForm();
  }

  async save(): Promise<void> {
    this.saving = true;
    this.error = '';
    try {
      const value = this.form.getRawValue();
      const updated = await this._resumeService.updatePersonalInfo({
        email: value.email ?? '',
        mobilePhone: value.mobilePhone ?? '',
        provinceId: value.provinceId ?? null,
        address: value.address ?? '',
        maritalStatus: value.maritalStatus ?? null,
        birthYear: value.birthYear ?? null,
        gender: value.gender ?? null,
        militaryServiceStatus: value.gender === Gender.Male ? value.militaryServiceStatus ?? null : null,
      });
      this.editing = false;
      this.saved.emit(updated);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.resume.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }

  private patchForm(): void {
    const info = this.resume.personalInfo;
    this.form.patchValue({
      email: info.email,
      mobilePhone: info.mobilePhone,
      provinceId: info.provinceId,
      address: info.address,
      maritalStatus: info.maritalStatus,
      birthYear: info.birthYear,
      gender: info.gender,
      militaryServiceStatus: info.militaryServiceStatus,
    });
  }
}

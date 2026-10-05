import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { BaseFormFieldComponent, BaseFormSelectOption } from '@modules/base/components/base-form-field/base-form-field.component';
import { isResumeSectionComplete } from '../../resume-completeness';
import {
  CONTRACT_TYPES,
  ContractType,
  ReferenceItemDto,
  ResumeDto,
  SENIORITY_LEVELS,
  SeniorityLevel,
  enumTranslateKey,
} from '../../resume.types';
import { ResumeService } from '../../resume.service';
import { ResumeInfoItemComponent } from '../resume-info-item/resume-info-item.component';
import { ResumeSectionCardComponent } from '../resume-section-card/resume-section-card.component';

@Component({
  selector: 'app-resume-job-preferences',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseCheckboxComponent,
    BaseFormFieldComponent,
    ResumeSectionCardComponent,
    ResumeInfoItemComponent,
  ],
  templateUrl: './job-preferences.component.html',
})
export class JobPreferencesComponent implements OnChanges {
  @Input({ required: true }) resume!: ResumeDto;
  @Input({ required: true }) provinces: ReferenceItemDto[] = [];
  @Input({ required: true }) jobCategories: ReferenceItemDto[] = [];
  @Input({ required: true }) salaryRanges: ReferenceItemDto[] = [];
  @Output() saved = new EventEmitter<ResumeDto>();

  editing = false;
  saving = false;
  error = '';
  seniorityLevels = SENIORITY_LEVELS;
  contractTypes = CONTRACT_TYPES;
  readonly SeniorityLevel = SeniorityLevel;
  readonly ContractType = ContractType;
  readonly enumTranslateKey = enumTranslateKey;

  form = new FormGroup({
    preferredProvinceIds: new FormControl<number[]>([]),
    jobCategoryIds: new FormControl<number[]>([]),
    seniorityLevels: new FormControl<SeniorityLevel[]>([]),
    acceptableContractTypes: new FormControl<ContractType[]>([]),
    minimumSalaryId: new FormControl<number | null>(null),
  });

  constructor(
    private _resumeService: ResumeService,
    private _localization: LocalizationService,
  ) {}

  get isComplete(): boolean {
    return isResumeSectionComplete('jobPreferences', this.resume);
  }

  get provinceOptions(): BaseFormSelectOption[] {
    return this.provinces.map((province) => ({ value: province.id, label: province.name }));
  }

  get categoryOptions(): BaseFormSelectOption[] {
    return this.jobCategories.map((category) => ({ value: category.id, label: category.name }));
  }

  get salaryOptions(): BaseFormSelectOption[] {
    return this.salaryRanges.map((range) => ({ value: range.id, label: range.name }));
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

  toggleSeniority(level: SeniorityLevel, checked: boolean): void {
    const current = this.form.controls.seniorityLevels.value ?? [];
    this.form.controls.seniorityLevels.setValue(
      checked ? [...current, level] : current.filter((l) => l !== level),
    );
  }

  toggleContract(type: ContractType, checked: boolean): void {
    const current = this.form.controls.acceptableContractTypes.value ?? [];
    this.form.controls.acceptableContractTypes.setValue(
      checked ? [...current, type] : current.filter((t) => t !== type),
    );
  }

  isSeniorityChecked(level: SeniorityLevel): boolean {
    return (this.form.controls.seniorityLevels.value ?? []).includes(level);
  }

  isContractChecked(type: ContractType): boolean {
    return (this.form.controls.acceptableContractTypes.value ?? []).includes(type);
  }

  async save(): Promise<void> {
    this.saving = true;
    this.error = '';
    try {
      const value = this.form.getRawValue();
      const updated = await this._resumeService.updateJobPreference({
        preferredProvinceIds: value.preferredProvinceIds ?? [],
        jobCategoryIds: value.jobCategoryIds ?? [],
        seniorityLevels: value.seniorityLevels ?? [],
        acceptableContractTypes: value.acceptableContractTypes ?? [],
        minimumSalaryId: value.minimumSalaryId ?? null,
      });
      this.editing = false;
      this.saved.emit(updated);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.resume.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }

  provinceName(id: number): string {
    return this.provinces.find((p) => p.id === id)?.name ?? String(id);
  }

  categoryName(id: number): string {
    return this.jobCategories.find((c) => c.id === id)?.name ?? String(id);
  }

  salaryName(id: number | null): string {
    if (id == null) return '';
    return this.salaryRanges.find((s) => s.id === id)?.name ?? String(id);
  }

  get preferredProvincesDisplay(): string {
    const ids = this.resume.jobPreference?.preferredProvinceIds ?? [];
    return ids.map((id) => this.provinceName(id)).join('، ');
  }

  get jobCategoriesDisplay(): string {
    const ids = this.resume.jobPreference?.jobCategoryIds ?? [];
    return ids.map((id) => this.categoryName(id)).join('، ');
  }

  get seniorityDisplay(): string {
    return (this.resume.jobPreference?.seniorityLevels ?? [])
      .map((level) => this._localization.translate(enumTranslateKey('seniorityLevel', SeniorityLevel, level)))
      .join('، ');
  }

  get contractDisplay(): string {
    return (this.resume.jobPreference?.acceptableContractTypes ?? [])
      .map((type) => this._localization.translate(enumTranslateKey('contractType', ContractType, type)))
      .join('، ');
  }

  private patchForm(): void {
    const pref = this.resume.jobPreference;
    this.form.patchValue({
      preferredProvinceIds: pref?.preferredProvinceIds ?? [],
      jobCategoryIds: pref?.jobCategoryIds ?? [],
      seniorityLevels: pref?.seniorityLevels ?? [],
      acceptableContractTypes: pref?.acceptableContractTypes ?? [],
      minimumSalaryId: pref?.minimumSalaryId ?? null,
    });
  }
}

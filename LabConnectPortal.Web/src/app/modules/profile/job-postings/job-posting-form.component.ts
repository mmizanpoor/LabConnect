import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { MatChipsModule } from '@angular/material/chips';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent, BaseFormSelectOption } from '@modules/base/components/base-form-field/base-form-field.component';
import { OrganizationLocationsService } from '../organization-locations/organization-locations.service';
import { OrganizationLocationDto } from '../organization-locations/organization-locations.types';
import { CenterProfileService } from '../center-profile/center-profile.service';
import { CenterProfileDto } from '../center-profile/center-profile.types';
import {
  CONTRACT_TYPES,
  ContractType,
  DEGREE_LEVELS,
  DegreeLevel,
  ReferenceItemDto,
} from '../resume/resume.types';
import { ResumeService } from '../resume/resume.service';
import { JobPostingsService } from './job-postings.service';
import {
  GENDER_REQUIREMENTS,
  GenderRequirement,
  JOB_MILITARY_REQUIREMENTS,
  JobMilitaryRequirement,
  JobPostingDto,
  JobPostingStatus,
  SUGGESTED_BENEFITS,
  SUGGESTED_PERSONAL_TRAITS,
} from './job-postings.types';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';

@Component({
  selector: 'app-job-posting-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    BaseCheckboxComponent,
    MatChipsModule,
    MatButtonModule,
    TranslocoPipe,
    BaseBackButtonComponent,
    BaseButtonComponent,
    BaseFormFieldComponent,
  ],
  templateUrl: './job-posting-form.component.html',
})
export class JobPostingFormComponent implements OnInit {
  readonly entity = SystemEntity.JobPosting;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.JobPosting);

  loading = false;
  saving = false;
  error = '';
  isNew = true;
  postingId = '';
  posting: JobPostingDto | null = null;
  centerProfile: CenterProfileDto | null = null;
  readOnly = false;

  jobCategories: ReferenceItemDto[] = [];
  salaryRanges: ReferenceItemDto[] = [];
  locations: OrganizationLocationDto[] = [];
  contractTypes = CONTRACT_TYPES;
  degreeLevels = DEGREE_LEVELS;
  genderRequirements = GENDER_REQUIREMENTS;
  militaryRequirements = JOB_MILITARY_REQUIREMENTS;
  suggestedTraits = SUGGESTED_PERSONAL_TRAITS;
  suggestedBenefits = SUGGESTED_BENEFITS;
  ContractType = ContractType;
  DegreeLevel = DegreeLevel;

  traits: string[] = [];
  benefits: string[] = [];
  skills: { skillId: number; skillName: string }[] = [];
  traitInput = new FormControl('');
  benefitInput = new FormControl('');
  skillInput = new FormControl('');
  skillSuggestions: { skillId: number; skillName: string }[] = [];

  form = new FormGroup({
    jobCategoryId: new FormControl<number | null>(null, Validators.required),
    locationId: new FormControl<string | null>(null, Validators.required),
    salaryRangeId: new FormControl<number | null>(null, Validators.required),
    minimumWorkExperienceYears: new FormControl(0, Validators.required),
    jobDescription: new FormControl('', Validators.required),
    genderRequirement: new FormControl<GenderRequirement>('NoPreference', Validators.required),
    militaryServiceRequirement: new FormControl<JobMilitaryRequirement>('NotImportant', Validators.required),
    minimumDegreeLevel: new FormControl<DegreeLevel>(DegreeLevel.Diploma, Validators.required),
    additionalNotes: new FormControl(''),
  });

  selectedContractTypes = new Set<ContractType>();

  constructor(
    private _route: ActivatedRoute,
    private _router: Router,
    private _jobPostingsService: JobPostingsService,
    private _locationsService: OrganizationLocationsService,
    private _centerProfileService: CenterProfileService,
    private _resumeService: ResumeService,
    private _localization: LocalizationService,
    private _authUtils: AuthUtils,
    private _labPermission: LabPermissionService,
  ) {}

  get canManageJobPostings(): boolean {
    return this.centerProfile?.isApproved ?? false;
  }

  get showProfileStatusMessage(): boolean {
    return this._authUtils.canManageCenterProfile();
  }

  get canSave(): boolean {
    return this._labPermission.can(
      this.entity,
      this.isNew ? 'create' : 'update'
    );
  }

  get canPublish(): boolean {
    return this._labPermission.can(this.entity, 'update');
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

  get jobCategoryOptions(): BaseFormSelectOption[] {
    return this.jobCategories.map((cat) => ({ value: cat.id, label: cat.name }));
  }

  get locationOptions(): BaseFormSelectOption[] {
    return this.locations.map((loc) => ({ value: loc.locationId, label: loc.locationName }));
  }

  get salaryRangeOptions(): BaseFormSelectOption[] {
    return this.salaryRanges.map((range) => ({ value: range.id, label: range.name }));
  }

  get genderOptions(): BaseFormSelectOption[] {
    return this.genderRequirements.map((g) => ({
      value: g,
      label: this._localization.translate(`enum.genderRequirement.${g}`),
    }));
  }

  get militaryOptions(): BaseFormSelectOption[] {
    return this.militaryRequirements.map((m) => ({
      value: m,
      label: this._localization.translate(`enum.jobMilitaryRequirement.${m}`),
    }));
  }

  get degreeLevelOptions(): BaseFormSelectOption[] {
    return this.degreeLevels.map((level) => ({
      value: level,
      label: this._localization.translate(`modules.profile.resume.enums.degreeLevel.${DegreeLevel[level]}`),
    }));
  }

  ngOnInit(): void {
    this.postingId = this._route.snapshot.paramMap.get('id') ?? '';
    this.isNew = !this.postingId || this.postingId === 'new';
    this.skillInput.valueChanges.subscribe(() => {
      void this.searchSkills();
    });
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const profileResult = await this._centerProfileService.getMyProfile();
      if (profileResult.success && profileResult.data) {
        this.centerProfile = profileResult.data;
      }

      if (this.isNew && !this.canManageJobPostings) {
        void this._router.navigate(['/profile/job-postings']);
        return;
      }

      const [refData, locationsResult] = await Promise.all([
        this._resumeService.loadReferenceData(),
        this._locationsService.getMyLocations(),
      ]);
      this.jobCategories = refData.jobCategories;
      this.salaryRanges = refData.salaryRanges;
      if (locationsResult.success && locationsResult.data) {
        this.locations = locationsResult.data;
      }

      if (!this.isNew) {
        const result = await this._jobPostingsService.getById(this.postingId);
        if (!result.success || !result.data) {
          throw new Error(result.message ?? this._localization.translate('modules.profile.jobPostings.errors.loadFailed'));
        }
        this.applyPosting(result.data);
      } else if (!this.locations.length) {
        this.error = this._localization.translate('modules.profile.jobPostings.errors.noLocations');
      }

      if (!this.canManageJobPostings) {
        this.readOnly = true;
        this.form.disable();
      }
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.jobPostings.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  private applyPosting(data: JobPostingDto): void {
    this.posting = data;
    this.readOnly = data.status === 'Closed' || !this.canManageJobPostings;
    this.form.patchValue({
      jobCategoryId: data.jobCategoryId,
      locationId: data.locationId,
      salaryRangeId: data.salaryRangeId,
      minimumWorkExperienceYears: data.minimumWorkExperienceYears,
      jobDescription: data.jobDescription,
      genderRequirement: data.genderRequirement,
      militaryServiceRequirement: data.militaryServiceRequirement,
      minimumDegreeLevel: data.minimumDegreeLevel,
      additionalNotes: data.additionalNotes,
    });
    this.selectedContractTypes = new Set(data.contractTypes);
    this.traits = [...data.requiredPersonalTraits];
    this.benefits = [...data.benefits];
    this.skills = data.essentialSkillIds.map((id, i) => ({
      skillId: id,
      skillName: data.essentialSkillNames[i] ?? String(id),
    }));
    if (this.readOnly) this.form.disable();
  }

  isContractChecked(type: ContractType): boolean {
    return this.selectedContractTypes.has(type);
  }

  toggleContract(type: ContractType, checked: boolean): void {
    if (this.readOnly) return;
    if (checked) this.selectedContractTypes.add(type);
    else this.selectedContractTypes.delete(type);
  }

  addTrait(value?: string): void {
    if (this.readOnly) return;
    const text = (value ?? this.traitInput.value)?.trim();
    if (!text || this.traits.includes(text)) return;
    this.traits.push(text);
    this.traitInput.setValue('');
  }

  removeTrait(index: number): void {
    if (this.readOnly) return;
    this.traits.splice(index, 1);
  }

  addBenefit(value?: string): void {
    if (this.readOnly) return;
    const text = (value ?? this.benefitInput.value)?.trim();
    if (!text || this.benefits.includes(text)) return;
    this.benefits.push(text);
    this.benefitInput.setValue('');
  }

  removeBenefit(index: number): void {
    if (this.readOnly) return;
    this.benefits.splice(index, 1);
  }

  async searchSkills(): Promise<void> {
    const term = this.skillInput.value?.trim() ?? '';
    if (!term) {
      this.skillSuggestions = [];
      return;
    }
    try {
      this.skillSuggestions = await this._resumeService.searchSkills(term);
    } catch {
      this.skillSuggestions = [];
    }
  }

  async addSkill(name?: string): Promise<void> {
    if (this.readOnly) return;
    const skillName = (name ?? this.skillInput.value)?.trim();
    if (!skillName) return;
    if (this.skills.some((s) => s.skillName.toLowerCase() === skillName.toLowerCase())) return;

    let skillId = this.skillSuggestions.find((s) => s.skillName.toLowerCase() === skillName.toLowerCase())?.skillId ?? 0;
    if (!skillId) {
      try {
        const created = await this._resumeService.createSkill(skillName);
        skillId = created.skillId;
      } catch (e: unknown) {
        this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.jobPostings.errors.saveFailed');
        return;
      }
    }
    this.skills.push({ skillId, skillName });
    this.skillInput.setValue('');
    this.skillSuggestions = [];
  }

  removeSkill(index: number): void {
    if (this.readOnly) return;
    this.skills.splice(index, 1);
  }

  statusLabel(status: JobPostingStatus): string {
    return this._localization.translate(`enum.jobPostingStatus.${status}`);
  }

  buildCommand() {
    const value = this.form.getRawValue();
    return {
      jobCategoryId: value.jobCategoryId!,
      locationId: value.locationId!,
      salaryRangeId: value.salaryRangeId!,
      minimumWorkExperienceYears: value.minimumWorkExperienceYears ?? 0,
      jobDescription: value.jobDescription ?? '',
      contractTypes: [...this.selectedContractTypes],
      requiredPersonalTraits: [...this.traits],
      essentialSkillIds: this.skills.map((s) => s.skillId),
      benefits: [...this.benefits],
      genderRequirement: value.genderRequirement ?? 'NoPreference',
      militaryServiceRequirement: value.militaryServiceRequirement ?? 'NotImportant',
      minimumDegreeLevel: value.minimumDegreeLevel ?? DegreeLevel.Diploma,
      additionalNotes: value.additionalNotes ?? '',
    };
  }

  async save(): Promise<void> {
    if (!this.canSave) return;
    if (this.readOnly) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.saving = true;
    this.error = '';
    try {
      const command = this.buildCommand();
      const result = this.isNew
        ? await this._jobPostingsService.create(command)
        : await this._jobPostingsService.update({ ...command, jobPostingId: this.postingId });

      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate('modules.profile.jobPostings.errors.saveFailed'));
      }
      void this._router.navigate(['/profile/job-postings', result.data.jobPostingId]);
      this.isNew = false;
      this.postingId = result.data.jobPostingId;
      this.applyPosting(result.data);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.jobPostings.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }

  async publish(): Promise<void> {
    if (!this.canPublish) return;
    if (!this.postingId || this.isNew) return;
    const result = await this._jobPostingsService.publish(this.postingId);
    if (!result.success || !result.data) {
      this.error = result.message ?? this._localization.translate('modules.profile.jobPostings.errors.publishFailed');
      return;
    }
    this.applyPosting(result.data);
  }
}

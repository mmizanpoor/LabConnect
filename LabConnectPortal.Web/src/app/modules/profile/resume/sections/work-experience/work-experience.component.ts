import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { AbstractControl, FormArray, FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { BaseFormFieldComponent, BaseFormSelectOption } from '@modules/base/components/base-form-field/base-form-field.component';
import { isResumeSectionComplete } from '../../resume-completeness';
import {
  PERSIAN_MONTHS,
  PersianMonth,
  ResumeDto,
  WorkExperienceDto,
  enumTranslateKey,
  resumeEnumOptions,
} from '../../resume.types';
import { ResumeService } from '../../resume.service';
import { ResumeSectionCardComponent } from '../resume-section-card/resume-section-card.component';

@Component({
  selector: 'app-resume-work-experience',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseCheckboxComponent,
    BaseFormFieldComponent,
    ResumeSectionCardComponent,
  ],
  templateUrl: './work-experience.component.html',
})
export class WorkExperienceComponent implements OnChanges {
  @Input({ required: true }) resume!: ResumeDto;
  @Output() saved = new EventEmitter<ResumeDto>();

  editing = false;
  saving = false;
  error = '';
  readonly PersianMonth = PersianMonth;
  readonly enumTranslateKey = enumTranslateKey;
  form = new FormGroup({ items: new FormArray<FormGroup>([]) });

  constructor(
    private _resumeService: ResumeService,
    private _localization: LocalizationService,
  ) {}

  get isComplete(): boolean {
    return isResumeSectionComplete('workExperience', this.resume);
  }

  get items(): FormArray<FormGroup> {
    return this.form.controls.items;
  }

  get monthOptions(): BaseFormSelectOption[] {
    return resumeEnumOptions(
      (key) => this._localization.translate(key),
      'persianMonth',
      PersianMonth,
      PERSIAN_MONTHS,
    );
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['resume'] && !this.editing) this.buildForm(this.resume.workExperiences);
  }

  controlOf(group: AbstractControl, name: string): FormControl {
    return (group as FormGroup).get(name) as FormControl;
  }

  startEdit(): void {
    this.editing = true;
    this.error = '';
    this.buildForm(this.resume.workExperiences);
  }

  cancel(): void {
    this.editing = false;
    this.buildForm(this.resume.workExperiences);
  }

  addItem(): void {
    this.items.push(this.createItemGroup(this.emptyItem()));
  }

  removeItem(index: number): void {
    this.items.removeAt(index);
  }

  async save(): Promise<void> {
    this.saving = true;
    this.error = '';
    try {
      const payload: WorkExperienceDto[] = this.items.controls
        .map((group) => group.getRawValue())
        .filter((v) => (v.jobTitle ?? '').trim() || (v.companyName ?? '').trim())
        .map((v) => ({
          workExperienceId: v.workExperienceId,
          jobTitle: v.jobTitle ?? '',
          companyName: v.companyName ?? '',
          startMonth: v.startMonth,
          startYear: v.startYear ? Number(v.startYear) : null,
          endMonth: v.isCurrentlyEmployed ? null : v.endMonth,
          endYear: v.isCurrentlyEmployed ? null : v.endYear ? Number(v.endYear) : null,
          isCurrentlyEmployed: !!v.isCurrentlyEmployed,
          jobDescription: v.jobDescription ?? '',
        }));
      const updated = await this._resumeService.saveWorkExperiences({ items: payload });
      this.editing = false;
      this.saved.emit(updated);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.resume.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }

  private buildForm(items: WorkExperienceDto[]): void {
    this.form.setControl(
      'items',
      new FormArray(items.length ? items.map((i) => this.createItemGroup(i)) : [this.createItemGroup(this.emptyItem())]),
    );
  }

  private createItemGroup(item: WorkExperienceDto): FormGroup {
    const group = new FormGroup({
      workExperienceId: new FormControl(item.workExperienceId ?? null),
      jobTitle: new FormControl(item.jobTitle),
      companyName: new FormControl(item.companyName),
      startMonth: new FormControl<PersianMonth | null>(item.startMonth),
      startYear: new FormControl(item.startYear),
      endMonth: new FormControl<PersianMonth | null>(item.endMonth),
      endYear: new FormControl(item.endYear),
      isCurrentlyEmployed: new FormControl(item.isCurrentlyEmployed),
      jobDescription: new FormControl(item.jobDescription),
    });

    group.controls.isCurrentlyEmployed.valueChanges.subscribe((employed) => {
      if (employed) {
        group.controls.endMonth.disable({ emitEvent: false });
        group.controls.endYear.disable({ emitEvent: false });
      } else {
        group.controls.endMonth.enable({ emitEvent: false });
        group.controls.endYear.enable({ emitEvent: false });
      }
    });

    if (item.isCurrentlyEmployed) {
      group.controls.endMonth.disable({ emitEvent: false });
      group.controls.endYear.disable({ emitEvent: false });
    }

    return group;
  }

  private emptyItem(): WorkExperienceDto {
    return {
      jobTitle: '',
      companyName: '',
      startMonth: null,
      startYear: null,
      endMonth: null,
      endYear: null,
      isCurrentlyEmployed: true,
      jobDescription: '',
    };
  }
}

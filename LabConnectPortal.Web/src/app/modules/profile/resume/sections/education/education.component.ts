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
  DEGREE_LEVELS,
  DegreeLevel,
  EducationDto,
  ResumeDto,
  enumTranslateKey,
  resumeEnumOptions,
} from '../../resume.types';
import { ResumeService } from '../../resume.service';
import { ResumeSectionCardComponent } from '../resume-section-card/resume-section-card.component';

@Component({
  selector: 'app-resume-education',
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
  templateUrl: './education.component.html',
})
export class EducationComponent implements OnChanges {
  @Input({ required: true }) resume!: ResumeDto;
  @Output() saved = new EventEmitter<ResumeDto>();

  editing = false;
  saving = false;
  error = '';
  readonly DegreeLevel = DegreeLevel;
  readonly enumTranslateKey = enumTranslateKey;
  form = new FormGroup({ items: new FormArray<FormGroup>([]) });

  constructor(
    private _resumeService: ResumeService,
    private _localization: LocalizationService,
  ) {}

  get isComplete(): boolean {
    return isResumeSectionComplete('education', this.resume);
  }

  get items(): FormArray<FormGroup> {
    return this.form.controls.items;
  }

  get degreeLevelOptions(): BaseFormSelectOption[] {
    return resumeEnumOptions(
      (key) => this._localization.translate(key),
      'degreeLevel',
      DegreeLevel,
      DEGREE_LEVELS,
    );
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['resume'] && !this.editing) this.buildForm(this.resume.educations);
  }

  controlOf(group: AbstractControl, name: string): FormControl {
    return (group as FormGroup).get(name) as FormControl;
  }

  startEdit(): void {
    this.editing = true;
    this.error = '';
    this.buildForm(this.resume.educations);
  }

  cancel(): void {
    this.editing = false;
    this.buildForm(this.resume.educations);
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
      const payload: EducationDto[] = this.items.controls
        .map((group) => group.getRawValue())
        .filter((v) => (v.fieldOfStudy ?? '').trim() || (v.institutionName ?? '').trim())
        .map((v) => ({
          educationId: v.educationId,
          fieldOfStudy: v.fieldOfStudy ?? '',
          institutionName: v.institutionName ?? '',
          degreeLevel: v.degreeLevel,
          startYear: v.startYear ? Number(v.startYear) : null,
          endYear: v.isCurrentlyStudying ? null : v.endYear ? Number(v.endYear) : null,
          isCurrentlyStudying: !!v.isCurrentlyStudying,
          description: v.description ?? '',
        }));
      const updated = await this._resumeService.saveEducations({ items: payload });
      this.editing = false;
      this.saved.emit(updated);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.resume.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }

  private buildForm(items: EducationDto[]): void {
    this.form.setControl(
      'items',
      new FormArray(items.length ? items.map((i) => this.createItemGroup(i)) : [this.createItemGroup(this.emptyItem())]),
    );
  }

  private createItemGroup(item: EducationDto): FormGroup {
    const group = new FormGroup({
      educationId: new FormControl(item.educationId ?? null),
      fieldOfStudy: new FormControl(item.fieldOfStudy),
      institutionName: new FormControl(item.institutionName),
      degreeLevel: new FormControl<DegreeLevel | null>(item.degreeLevel),
      startYear: new FormControl(item.startYear),
      endYear: new FormControl(item.endYear),
      isCurrentlyStudying: new FormControl(item.isCurrentlyStudying),
      description: new FormControl(item.description),
    });

    group.controls.isCurrentlyStudying.valueChanges.subscribe((studying) => {
      if (studying) group.controls.endYear.disable({ emitEvent: false });
      else group.controls.endYear.enable({ emitEvent: false });
    });

    if (item.isCurrentlyStudying) group.controls.endYear.disable({ emitEvent: false });
    return group;
  }

  private emptyItem(): EducationDto {
    return {
      fieldOfStudy: '',
      institutionName: '',
      degreeLevel: null,
      startYear: null,
      endYear: null,
      isCurrentlyStudying: true,
      description: '',
    };
  }
}

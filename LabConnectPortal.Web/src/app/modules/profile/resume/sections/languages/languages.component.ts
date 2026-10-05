import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { AbstractControl, FormArray, FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent, BaseFormSelectOption } from '@modules/base/components/base-form-field/base-form-field.component';
import { isResumeSectionComplete } from '../../resume-completeness';
import {
  LANGUAGE_PROFICIENCY_LEVELS,
  LanguageProficiencyLevel,
  ReferenceItemDto,
  ResumeDto,
  UserLanguageDto,
  enumTranslateKey,
  resumeEnumOptions,
} from '../../resume.types';
import { ResumeService } from '../../resume.service';
import { ResumeSectionCardComponent } from '../resume-section-card/resume-section-card.component';

@Component({
  selector: 'app-resume-languages',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    ResumeSectionCardComponent,
  ],
  templateUrl: './languages.component.html',
})
export class LanguagesComponent implements OnChanges {
  @Input({ required: true }) resume!: ResumeDto;
  @Input({ required: true }) languageNames: ReferenceItemDto[] = [];
  @Output() saved = new EventEmitter<ResumeDto>();

  editing = false;
  saving = false;
  error = '';
  readonly LanguageProficiencyLevel = LanguageProficiencyLevel;
  readonly enumTranslateKey = enumTranslateKey;
  form = new FormGroup({ items: new FormArray<FormGroup>([]) });

  constructor(
    private _resumeService: ResumeService,
    private _localization: LocalizationService,
  ) {}

  get isComplete(): boolean {
    return isResumeSectionComplete('languages', this.resume);
  }

  get items(): FormArray<FormGroup> {
    return this.form.controls.items;
  }

  get languageOptions(): BaseFormSelectOption[] {
    return this.languageNames.map((lang) => ({ value: lang.id, label: lang.name }));
  }

  get proficiencyOptions(): BaseFormSelectOption[] {
    return resumeEnumOptions(
      (key) => this._localization.translate(key),
      'languageProficiency',
      LanguageProficiencyLevel,
      LANGUAGE_PROFICIENCY_LEVELS,
    );
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['resume'] && !this.editing) this.buildForm(this.resume.languages);
  }

  controlOf(group: AbstractControl, name: string): FormControl {
    return (group as FormGroup).get(name) as FormControl;
  }

  startEdit(): void {
    this.editing = true;
    this.error = '';
    this.buildForm(this.resume.languages);
  }

  cancel(): void {
    this.editing = false;
    this.buildForm(this.resume.languages);
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
      const payload: UserLanguageDto[] = this.items.controls
        .map((group) => group.getRawValue())
        .filter((v) => v.languageNameId)
        .map((v) => ({
          userLanguageId: v.userLanguageId,
          languageNameId: Number(v.languageNameId),
          languageName: this.languageNames.find((l) => l.id === Number(v.languageNameId))?.name ?? '',
          proficiencyLevel: v.proficiencyLevel,
        }));
      const updated = await this._resumeService.saveLanguages({ items: payload });
      this.editing = false;
      this.saved.emit(updated);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.resume.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }

  private buildForm(items: UserLanguageDto[]): void {
    this.form.setControl(
      'items',
      new FormArray(items.length ? items.map((i) => this.createItemGroup(i)) : [this.createItemGroup(this.emptyItem())]),
    );
  }

  private createItemGroup(item: UserLanguageDto): FormGroup {
    return new FormGroup({
      userLanguageId: new FormControl(item.userLanguageId ?? null),
      languageNameId: new FormControl(item.languageNameId || null),
      proficiencyLevel: new FormControl<LanguageProficiencyLevel | null>(item.proficiencyLevel),
    });
  }

  private emptyItem(): UserLanguageDto {
    return { languageNameId: 0, languageName: '', proficiencyLevel: null };
  }
}

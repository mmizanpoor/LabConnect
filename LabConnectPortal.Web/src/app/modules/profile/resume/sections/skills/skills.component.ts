import { Component, DestroyRef, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { isResumeSectionComplete } from '../../resume-completeness';
import { ResumeDto, UserSkillDto } from '../../resume.types';
import { ResumeService } from '../../resume.service';
import { ResumeSectionCardComponent } from '../resume-section-card/resume-section-card.component';

@Component({
  selector: 'app-resume-skills',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    ResumeSectionCardComponent,
  ],
  templateUrl: './skills.component.html',
})
export class SkillsComponent implements OnChanges {
  private readonly _destroyRef = inject(DestroyRef);

  @Input({ required: true }) resume!: ResumeDto;
  @Output() saved = new EventEmitter<ResumeDto>();

  editing = false;
  saving = false;
  error = '';
  items: UserSkillDto[] = [];
  skillInput = new FormControl('');
  suggestions: { skillId: number; skillName: string }[] = [];

  constructor(
    private _resumeService: ResumeService,
    private _localization: LocalizationService,
  ) {
    this.skillInput.valueChanges
      .pipe(debounceTime(200), distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => {
        void this.searchSkills();
      });
  }

  get isComplete(): boolean {
    return isResumeSectionComplete('skills', this.resume);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['resume']) {
      this.items = this.resume.skills.map((s) => ({ ...s }));
    }
  }

  startEdit(): void {
    this.editing = true;
    this.error = '';
    this.items = this.resume.skills.map((s) => ({ ...s }));
  }

  cancel(): void {
    this.editing = false;
    this.items = this.resume.skills.map((s) => ({ ...s }));
  }

  async searchSkills(): Promise<void> {
    const term = this.skillInput.value?.trim() ?? '';
    if (!term) {
      this.suggestions = [];
      return;
    }
    try {
      this.suggestions = await this._resumeService.searchSkills(term);
    } catch {
      this.suggestions = [];
    }
  }

  async addSkill(name?: string): Promise<void> {
    const skillName = (name ?? this.skillInput.value)?.trim();
    if (!skillName) return;

    const existing = this.items.find((s) => s.skillName.toLowerCase() === skillName.toLowerCase());
    if (existing) return;

    let skillId = this.suggestions.find((s) => s.skillName.toLowerCase() === skillName.toLowerCase())?.skillId ?? 0;
    if (!skillId) {
      try {
        const created = await this._resumeService.createSkill(skillName);
        skillId = created.skillId;
      } catch (e: unknown) {
        this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.resume.errors.saveFailed');
        return;
      }
    }

    this.items.push({ skillId, skillName, proficiencyLevel: null });
    this.skillInput.setValue('');
    this.suggestions = [];
  }

  removeSkill(index: number): void {
    this.items.splice(index, 1);
  }

  async save(): Promise<void> {
    this.saving = true;
    this.error = '';
    try {
      const updated = await this._resumeService.saveSkills({ items: this.items });
      this.editing = false;
      this.saved.emit(updated);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.resume.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }
}

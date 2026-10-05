import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { isResumeSectionComplete } from '../../resume-completeness';
import { ResumeDto } from '../../resume.types';
import { ResumeService } from '../../resume.service';
import { ResumeSectionCardComponent } from '../resume-section-card/resume-section-card.component';

@Component({
  selector: 'app-resume-about-me',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    ResumeSectionCardComponent,
  ],
  templateUrl: './about-me.component.html',
})
export class AboutMeComponent implements OnChanges {
  @Input({ required: true }) resume!: ResumeDto;
  @Output() saved = new EventEmitter<ResumeDto>();

  editing = false;
  saving = false;
  error = '';
  aboutMe = new FormControl('');

  constructor(
    private _resumeService: ResumeService,
    private _localization: LocalizationService,
  ) {}

  get isComplete(): boolean {
    return isResumeSectionComplete('aboutMe', this.resume);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['resume']) {
      this.aboutMe.setValue(this.resume.aboutMe);
    }
  }

  startEdit(): void {
    this.editing = true;
    this.error = '';
    this.aboutMe.setValue(this.resume.aboutMe);
  }

  cancel(): void {
    this.editing = false;
    this.aboutMe.setValue(this.resume.aboutMe);
  }

  async save(): Promise<void> {
    this.saving = true;
    this.error = '';
    try {
      const updated = await this._resumeService.updateAboutMe({ aboutMe: this.aboutMe.value ?? '' });
      this.editing = false;
      this.saved.emit(updated);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.resume.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }
}

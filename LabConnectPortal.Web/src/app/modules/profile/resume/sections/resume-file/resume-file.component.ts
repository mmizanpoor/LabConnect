import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { isResumeSectionComplete } from '../../resume-completeness';
import { ResumeDto } from '../../resume.types';
import { ResumeService } from '../../resume.service';
import { ResumeSectionCardComponent } from '../resume-section-card/resume-section-card.component';

@Component({
  selector: 'app-resume-file',
  standalone: true,
  imports: [MatIconModule, TranslocoPipe, ResumeSectionCardComponent],
  templateUrl: './resume-file.component.html',
})
export class ResumeFileComponent {
  @Input({ required: true }) resume!: ResumeDto;
  @Output() saved = new EventEmitter<ResumeDto>();

  uploading = false;
  deleting = false;
  error = '';

  constructor(
    private _resumeService: ResumeService,
    private _localization: LocalizationService,
  ) {}

  get isComplete(): boolean {
    return isResumeSectionComplete('resumeFile', this.resume);
  }

  async onFileSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.uploading = true;
    this.error = '';
    try {
      const updated = await this._resumeService.uploadResume(file);
      this.saved.emit(updated);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.resume.errors.uploadFailed');
    } finally {
      this.uploading = false;
      input.value = '';
    }
  }

  async deleteFile(): Promise<void> {
    this.deleting = true;
    this.error = '';
    try {
      const updated = await this._resumeService.deleteResume();
      this.saved.emit(updated);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.resume.errors.saveFailed');
    } finally {
      this.deleting = false;
    }
  }

  async download(): Promise<void> {
    try {
      const blob = await this._resumeService.getResumeBlob();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = this.resume.resumeFile?.fileName ?? 'resume';
      a.click();
      URL.revokeObjectURL(url);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.resume.errors.loadFailed');
    }
  }
}

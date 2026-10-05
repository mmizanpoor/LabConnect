import { Component, OnInit } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { ReferenceData, ResumeDto, ResumeSectionAnchor } from './resume.types';
import { getResumeProgress } from './resume-completeness';
import { ResumeService } from './resume.service';
import { BasicInfoComponent } from './sections/basic-info/basic-info.component';
import { AboutMeComponent } from './sections/about-me/about-me.component';
import { PersonalInfoComponent } from './sections/personal-info/personal-info.component';
import { SkillsComponent } from './sections/skills/skills.component';
import { WorkExperienceComponent } from './sections/work-experience/work-experience.component';
import { EducationComponent } from './sections/education/education.component';
import { LanguagesComponent } from './sections/languages/languages.component';
import { JobPreferencesComponent } from './sections/job-preferences/job-preferences.component';
import { ResumeFileComponent } from './sections/resume-file/resume-file.component';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';

@Component({
  selector: 'app-resume',
  standalone: true,
  imports: [
    TranslocoPipe,
    BasicInfoComponent,
    AboutMeComponent,
    PersonalInfoComponent,
    SkillsComponent,
    WorkExperienceComponent,
    EducationComponent,
    LanguagesComponent,
    JobPreferencesComponent,
    ResumeFileComponent,
  ],
  templateUrl: './resume.component.html',
  styleUrl: './resume.component.scss',
})
export class ResumeComponent implements OnInit {
  readonly entity = SystemEntity.Resume;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.Resume);

  loading = false;
  error = '';
  resume: ResumeDto | null = null;
  referenceData: ReferenceData | null = null;

  constructor(
    private _resumeService: ResumeService,
    private _localization: LocalizationService,
  ) {}

  get progress() {
    return this.resume ? getResumeProgress(this.resume) : { completed: 0, total: 0, percent: 0 };
  }

  async ngOnInit(): Promise<void> {
    await this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const [resume, referenceData] = await Promise.all([
        this._resumeService.getMyResume(),
        this._resumeService.loadReferenceData(),
      ]);
      this.resume = resume;
      this.referenceData = referenceData;
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.resume.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  onSectionSaved(updated: ResumeDto): void {
    this.resume = updated;
  }

  scrollToSection(section: ResumeSectionAnchor): void {
    const id = section === 'work-experience' ? 'resume-work-experience' : 'resume-education';
    document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }
}

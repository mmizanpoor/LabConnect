import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { LocalizationService } from '@core/services/localization/localization.service';
import { ContractType } from '../profile/resume/resume.types';
import { formatRelativeTime } from './jobs.service';
import { PublicJobPostingCardDto } from './jobs.types';

@Component({
  selector: 'app-job-card',
  standalone: true,
  imports: [RouterLink, MatIconModule],
  templateUrl: './job-card.component.html',
})
export class JobCardComponent {
  @Input({ required: true }) job!: PublicJobPostingCardDto;

  constructor(private _localization: LocalizationService) {}

  get organizationInitial(): string {
    return this.job.organizationName?.trim().charAt(0) || '?';
  }

  get locationLabel(): string {
    const parts = [this.job.provinceName, this.job.locationName].filter(Boolean);
    return parts.join('، ');
  }

  get contractSummary(): string {
    const parts: string[] = [];
    if (this.job.contractTypes.length) {
      parts.push(this.contractLabel(this.job.contractTypes[0]));
      if (this.job.contractTypes.length > 1) {
        parts[0] += ` +${this.job.contractTypes.length - 1}`;
      }
    }
    if (this.job.salaryRangeName) {
      parts.push(`(${this.job.salaryRangeName})`);
    }
    return parts.join(' ');
  }

  contractLabel(type: ContractType): string {
    return this._localization.translate(`modules.profile.resume.enums.contractType.${type}`);
  }

  relativeTime(): string {
    return formatRelativeTime(this.job.publishedAt);
  }
}

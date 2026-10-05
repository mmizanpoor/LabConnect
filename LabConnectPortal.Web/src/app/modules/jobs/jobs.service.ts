import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  GetActivePostingsQuery,
  GetApplicationsForPostingQuery,
  JobApplicationDto,
  MyJobApplicationListItemDto,
  PagedJobApplications,
  PagedPublicJobs,
  PublicJobPostingDetailDto,
  PublicJobPostingFiltersDto,
  ApproveJobApplicationCommand,
  RejectJobApplicationCommand,
} from './jobs.types';

@Injectable({ providedIn: 'root' })
export class PublicJobsService {
  constructor(private _http: ApiHttpService) {}

  getActivePostings(query: GetActivePostingsQuery) {
    return this._http.post<PagedPublicJobs>('PublicJob', 'GetActivePostings', query);
  }

  getPostingFilters() {
    return this._http.get<PublicJobPostingFiltersDto>('PublicJob', 'GetPostingFilters');
  }

  getById(jobPostingId: string) {
    return this._http.get<PublicJobPostingDetailDto>('PublicJob', 'GetById', { jobPostingId });
  }
}

@Injectable({ providedIn: 'root' })
export class JobApplicationsService {
  constructor(private _http: ApiHttpService) {}

  submit(jobPostingId: string) {
    return this._http.post<JobApplicationDto>('JobApplication', 'Submit', { jobPostingId });
  }

  getMyApplications() {
    return this._http.get<MyJobApplicationListItemDto[]>('JobApplication', 'GetMyApplications');
  }

  getForPosting(query: GetApplicationsForPostingQuery) {
    return this._http.post<PagedJobApplications>('JobApplication', 'GetForPosting', query);
  }

  approve(command: ApproveJobApplicationCommand) {
    return this._http.post<JobApplicationDto>('JobApplication', 'Approve', command);
  }

  reject(command: RejectJobApplicationCommand) {
    return this._http.post<JobApplicationDto>('JobApplication', 'Reject', command);
  }
}

export function formatRelativeTime(isoDate: string | null | undefined, now = new Date()): string {
  if (!isoDate) return '';
  const date = new Date(isoDate);
  const diffMs = now.getTime() - date.getTime();
  const diffMinutes = Math.floor(diffMs / 60000);
  if (diffMinutes < 1) return 'همین الان';
  if (diffMinutes < 60) return `${diffMinutes} دقیقه پیش`;
  const diffHours = Math.floor(diffMinutes / 60);
  if (diffHours < 24) return `${diffHours} ساعت پیش`;
  const diffDays = Math.floor(diffHours / 24);
  if (diffDays < 30) return `${diffDays} روز پیش`;
  const diffMonths = Math.floor(diffDays / 30);
  return `${diffMonths} ماه پیش`;
}

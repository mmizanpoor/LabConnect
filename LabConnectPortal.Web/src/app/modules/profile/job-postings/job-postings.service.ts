import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  GetMyJobPostingsQuery,
  JobPostingDashboardStats,
  JobPostingDto,
  PagedJobPostings,
  SaveJobPostingCommand,
  UpdateJobPostingCommand,
} from './job-postings.types';

@Injectable({ providedIn: 'root' })
export class JobPostingsService {
  constructor(private _http: ApiHttpService) {}

  getMyPostings(query: GetMyJobPostingsQuery) {
    return this._http.post<PagedJobPostings>('JobPosting', 'GetMyPostings', query);
  }

  getById(jobPostingId: string) {
    return this._http.get<JobPostingDto>('JobPosting', 'GetById', { jobPostingId });
  }

  create(command: SaveJobPostingCommand) {
    return this._http.post<JobPostingDto>('JobPosting', 'Create', command);
  }

  update(command: UpdateJobPostingCommand) {
    return this._http.post<JobPostingDto>('JobPosting', 'Update', command);
  }

  publish(jobPostingId: string) {
    return this._http.post<JobPostingDto>('JobPosting', 'Publish', { jobPostingId });
  }

  close(jobPostingId: string) {
    return this._http.post<JobPostingDto>('JobPosting', 'Close', { jobPostingId });
  }

  reopen(jobPostingId: string) {
    return this._http.post<JobPostingDto>('JobPosting', 'Reopen', { jobPostingId });
  }

  delete(jobPostingId: string) {
    return this._http.delete('JobPosting', 'Delete', { jobPostingId });
  }

  async getDashboardStats(): Promise<JobPostingDashboardStats> {
    const result = await this._http.get<JobPostingDashboardStats>('JobPosting', 'GetDashboardStats');
    if (!result.success || !result.data) {
      throw new Error(result.message ?? 'خطا در بارگذاری آمار آگهی‌ها');
    }
    return result.data;
  }

  async getDashboardStatsForCurrentLab(): Promise<JobPostingDashboardStats> {
    const result = await this._http.get<JobPostingDashboardStats>('JobPosting', 'GetDashboardStatsForCurrentLab');
    if (!result.success || !result.data) {
      throw new Error(result.message ?? 'خطا در بارگذاری آمار آگهی‌ها');
    }
    return result.data;
  }
}

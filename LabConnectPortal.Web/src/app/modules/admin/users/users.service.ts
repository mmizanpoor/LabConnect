import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { ResumeDto } from '../../profile/resume/resume.types';
import { GetUsersQuery, PagedResult, UserDto, UserStatsDto } from './users.types';

@Injectable({ providedIn: 'root' })
export class UsersService {
  constructor(private _http: ApiHttpService) {}

  getUsers(query: GetUsersQuery) {
    return this._http.post<PagedResult<UserDto>>('User', 'GetUsers', query);
  }

  getById(id: string) {
    return this._http.get<UserDto>('User', 'GetById', { id });
  }

  getStats() {
    return this._http.get<UserStatsDto>('User', 'GetStats');
  }

  toggleActive(userId: string, isActive: boolean) {
    return this._http.post('User', 'ToggleActive', { userId, isActive });
  }

  deleteUser(id: string) {
    return this._http.delete('User', 'Delete', { id });
  }

  getUserResume(userId: string) {
    return this._http.get<ResumeDto>('User', 'GetResume', { userId });
  }

  getUserResumePhoto(userId: string) {
    return this._http.getBlob('User', 'GetResumePhoto', { userId });
  }

  getUserResumeFile(userId: string) {
    return this._http.getBlob('User', 'GetResumeFile', { userId });
  }
}

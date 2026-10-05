import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  GetLoginAttemptsQuery,
  PagedResult,
  UserLoginLogDto,
} from './login-attempt.types';

@Injectable({ providedIn: 'root' })
export class LoginAttemptService {
  constructor(private _http: ApiHttpService) {}

  getMine(query: GetLoginAttemptsQuery) {
    return this._http.post<PagedResult<UserLoginLogDto>>('LoginAttempt', 'GetMine', query);
  }

  getAll(query: GetLoginAttemptsQuery) {
    return this._http.post<PagedResult<UserLoginLogDto>>('LoginAttempt', 'GetAll', query);
  }
}

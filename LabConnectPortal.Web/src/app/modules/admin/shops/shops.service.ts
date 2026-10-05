import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { CenterProfileDto, CenterProfileListItemDto, GetCenterProfilesQuery, PagedResult } from './shops.types';

@Injectable({ providedIn: 'root' })
export class ShopsService {
  constructor(private _http: ApiHttpService) {}

  getShops(query: GetCenterProfilesQuery) {
    return this._http.post<PagedResult<CenterProfileListItemDto>>('CenterProfile', 'GetShops', query);
  }

  getById(id: string) {
    return this._http.get<CenterProfileDto>('CenterProfile', 'GetById', { id });
  }

  approve(id: string) {
    return this._http.post('CenterProfile', 'Approve', { id });
  }

  enableApiKey(id: string) {
    return this._http.post('CenterProfile', 'EnableApiKey', { id });
  }

  reject(id: string, reason: string) {
    return this._http.post('CenterProfile', 'Reject', { id, reason });
  }

  revokeApproval(id: string) {
    return this._http.post('CenterProfile', 'RevokeApproval', { id });
  }

  getFileBlob(profileId: string, kind: 'Logo' | 'NationalCard' | 'License' | 'OfficialImage' | 'TradeCard') {
    return this._http.getBlob('CenterProfile', 'GetFile', { profileId, kind });
  }
}

import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { ApiKeyDto, CreateApiKeyCommand, UpdateApiKeyCommand } from './api-keys.types';

@Injectable({ providedIn: 'root' })
export class ApiKeysService {
  constructor(private _http: ApiHttpService) {}

  getMine() {
    return this._http.get<ApiKeyDto[]>('ApiKey', 'GetMine');
  }

  getAll() {
    return this._http.get<ApiKeyDto[]>('ApiKey', 'GetAll');
  }

  create(command: CreateApiKeyCommand) {
    return this._http.post<ApiKeyDto>('ApiKey', 'Create', command);
  }

  updateMine(command: UpdateApiKeyCommand) {
    return this._http.post<ApiKeyDto>('ApiKey', 'UpdateMine', command);
  }

  update(command: UpdateApiKeyCommand) {
    return this._http.post<ApiKeyDto>('ApiKey', 'Update', command);
  }

  deleteMine(id: string) {
    return this._http.delete('ApiKey', 'DeleteMine', { id });
  }

  delete(id: string) {
    return this._http.delete('ApiKey', 'Delete', { id });
  }
}

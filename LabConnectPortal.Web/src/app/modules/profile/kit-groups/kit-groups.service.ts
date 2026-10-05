import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { CreateKitGroupCommand, KitGroupDto, UpdateKitGroupCommand } from './kit-groups.types';

@Injectable({ providedIn: 'root' })
export class KitGroupsService {
  constructor(private _http: ApiHttpService) {}

  getAll() {
    return this._http.get<KitGroupDto[]>('KitGroup', 'GetAll');
  }

  create(command: CreateKitGroupCommand) {
    return this._http.post<KitGroupDto>('KitGroup', 'Create', command);
  }

  update(command: UpdateKitGroupCommand) {
    return this._http.post<KitGroupDto>('KitGroup', 'Update', command);
  }

  delete(id: number) {
    return this._http.delete('KitGroup', 'Delete', { id });
  }
}

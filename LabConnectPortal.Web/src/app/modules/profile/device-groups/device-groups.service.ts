import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  CreateDeviceGroupCommand,
  DeviceGroupDto,
  UpdateDeviceGroupCommand,
} from './device-groups.types';

@Injectable({ providedIn: 'root' })
export class DeviceGroupsService {
  constructor(private _http: ApiHttpService) {}

  getAll() {
    return this._http.get<DeviceGroupDto[]>('DeviceGroup', 'GetAll');
  }

  create(command: CreateDeviceGroupCommand) {
    return this._http.post<DeviceGroupDto>('DeviceGroup', 'Create', command);
  }

  update(command: UpdateDeviceGroupCommand) {
    return this._http.post<DeviceGroupDto>('DeviceGroup', 'Update', command);
  }

  delete(id: number) {
    return this._http.delete('DeviceGroup', 'Delete', { id });
  }
}

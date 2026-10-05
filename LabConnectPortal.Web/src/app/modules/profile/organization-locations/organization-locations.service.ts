import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  CreateOrganizationLocationCommand,
  OrganizationLocationDto,
  UpdateOrganizationLocationCommand,
} from './organization-locations.types';

@Injectable({ providedIn: 'root' })
export class OrganizationLocationsService {
  constructor(private _http: ApiHttpService) {}

  getMyLocations() {
    return this._http.get<OrganizationLocationDto[]>('OrganizationLocation', 'GetMyLocations');
  }

  create(command: CreateOrganizationLocationCommand) {
    return this._http.post<OrganizationLocationDto>('OrganizationLocation', 'Create', command);
  }

  update(command: UpdateOrganizationLocationCommand) {
    return this._http.post<OrganizationLocationDto>('OrganizationLocation', 'Update', command);
  }

  delete(locationId: string) {
    return this._http.delete('OrganizationLocation', 'Delete', { locationId });
  }
}

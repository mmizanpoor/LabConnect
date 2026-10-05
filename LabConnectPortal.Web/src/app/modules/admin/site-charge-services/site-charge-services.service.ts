import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  SaveSiteChargeServiceCommand,
  SiteChargeServiceAdminDto,
} from './site-charge-services.types';

@Injectable({ providedIn: 'root' })
export class SiteChargeServicesService {
  constructor(private _http: ApiHttpService) {}

  getAll() {
    return this._http.get<SiteChargeServiceAdminDto[]>('SiteChargeService', 'GetAll');
  }

  save(command: SaveSiteChargeServiceCommand) {
    return this._http.post<SiteChargeServiceAdminDto>('SiteChargeService', 'Save', command);
  }

  delete(siteChargeServiceId: number) {
    return this._http.post<void>('SiteChargeService', 'Delete', { siteChargeServiceId });
  }
}

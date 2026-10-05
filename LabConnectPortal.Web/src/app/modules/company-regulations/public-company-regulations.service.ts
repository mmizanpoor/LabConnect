import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { CompanyRegulationDto } from '@modules/admin/company-regulations/company-regulations.types';

@Injectable({ providedIn: 'root' })
export class PublicCompanyRegulationsService {
  constructor(private _http: ApiHttpService) {}

  getAll() {
    return this._http.get<CompanyRegulationDto[]>('PublicCompanyRegulation', 'GetAll');
  }
}

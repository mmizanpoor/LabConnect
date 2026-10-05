import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  CompanyRegulationDto,
  SaveCompanyRegulationCommand,
  UpdateCompanyRegulationCommand,
} from './company-regulations.types';

@Injectable({ providedIn: 'root' })
export class CompanyRegulationsService {
  constructor(private _http: ApiHttpService) {}

  getAll() {
    return this._http.get<CompanyRegulationDto[]>('CompanyRegulation', 'GetAll');
  }

  getById(companyRegulationId: string) {
    return this._http.get<CompanyRegulationDto>('CompanyRegulation', 'GetById', {
      companyRegulationId,
    });
  }

  create(command: SaveCompanyRegulationCommand) {
    return this._http.post<CompanyRegulationDto>('CompanyRegulation', 'Create', command);
  }

  update(command: UpdateCompanyRegulationCommand) {
    return this._http.post<CompanyRegulationDto>('CompanyRegulation', 'Update', command);
  }

  delete(companyRegulationId: string) {
    return this._http.delete('CompanyRegulation', 'Delete', { companyRegulationId });
  }
}

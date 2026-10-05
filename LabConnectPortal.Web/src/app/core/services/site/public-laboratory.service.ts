import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';

export interface LaboratoryLookupDto {
  name: string;
  labCode?: number | null;
  labCodeNew: number;
  isApproved: boolean;
  status: string;
}

export interface SrLabNameDto {
  intLabId: number;
  intLabIdNew?: number | null;
  vchLabName?: string | null;
}

@Injectable({ providedIn: 'root' })
export class PublicLaboratoryService {
  constructor(private _http: ApiHttpService) {}

  getByLabCodeNew(labCodeNew: number) {
    return this._http.get<LaboratoryLookupDto | null>('PublicLaboratory', 'GetByLabCodeNew', {
      labCodeNew,
    });
  }

  getLabDetail(labCodeNew: number) {
    return this._http.get<SrLabNameDto | null>('PublicLaboratory', 'GetLabDetailAsync', {
      labCodeNew,
    });
  }
}

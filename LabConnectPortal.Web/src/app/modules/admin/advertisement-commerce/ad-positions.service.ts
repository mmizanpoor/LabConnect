import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  AdvertisementPositionDto,
  SaveAdvertisementPositionCommand,
  UpdateAdvertisementPositionCommand,
} from './advertisement-commerce.types';

@Injectable({ providedIn: 'root' })
export class AdPositionsService {
  constructor(private _http: ApiHttpService) {}

  getAll(activeOnly = false) {
    return this._http.get<AdvertisementPositionDto[]>(
      'AdvertisementPosition',
      'GetAll',
      activeOnly ? { activeOnly: 'true' } : undefined,
    );
  }

  getById(id: number) {
    return this._http.get<AdvertisementPositionDto>('AdvertisementPosition', 'GetById', { id });
  }

  create(command: SaveAdvertisementPositionCommand) {
    return this._http.post<AdvertisementPositionDto>('AdvertisementPosition', 'Create', command);
  }

  update(command: UpdateAdvertisementPositionCommand) {
    return this._http.post<AdvertisementPositionDto>('AdvertisementPosition', 'Update', command);
  }

  delete(id: number) {
    return this._http.delete('AdvertisementPosition', 'Delete', { id });
  }
}

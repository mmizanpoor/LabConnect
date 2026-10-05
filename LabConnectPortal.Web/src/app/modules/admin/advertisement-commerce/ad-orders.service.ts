import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  AdvertisementOrderDto,
  SaveAdvertisementOrderCommand,
  UpdateAdvertisementOrderStatusCommand,
} from './advertisement-commerce.types';

@Injectable({ providedIn: 'root' })
export class AdOrdersService {
  constructor(private _http: ApiHttpService) {}

  getAll() {
    return this._http.get<AdvertisementOrderDto[]>('AdvertisementOrder', 'GetAll');
  }

  getById(id: string) {
    return this._http.get<AdvertisementOrderDto>('AdvertisementOrder', 'GetById', { id });
  }

  create(command: SaveAdvertisementOrderCommand) {
    return this._http.post<AdvertisementOrderDto>('AdvertisementOrder', 'Create', command);
  }

  updateStatus(command: UpdateAdvertisementOrderStatusCommand) {
    return this._http.post<AdvertisementOrderDto>('AdvertisementOrder', 'UpdateStatus', command);
  }

  delete(id: string) {
    return this._http.delete('AdvertisementOrder', 'Delete', { id });
  }
}

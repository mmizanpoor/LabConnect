import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  AdvertisementDurationDto,
  AdvertisementPriceDto,
  AdvertisementPriceMatrixDto,
  SetAdvertisementPriceCommand,
} from './advertisement-commerce.types';

@Injectable({ providedIn: 'root' })
export class AdPricesService {
  constructor(private _http: ApiHttpService) {}

  getMatrix() {
    return this._http.get<AdvertisementPriceMatrixDto>('AdvertisementPrice', 'GetMatrix');
  }

  getHistory(positionId: number, durationId: number) {
    return this._http.get<AdvertisementPriceDto[]>('AdvertisementPrice', 'GetHistory', {
      advertisementPositionId: positionId,
      advertisementDurationId: durationId,
    });
  }

  setPrice(command: SetAdvertisementPriceCommand) {
    return this._http.post<AdvertisementPriceDto>('AdvertisementPrice', 'SetPrice', command);
  }
}

@Injectable({ providedIn: 'root' })
export class AdDurationsService {
  constructor(private _http: ApiHttpService) {}

  getAll() {
    return this._http.get<AdvertisementDurationDto[]>('AdvertisementDuration', 'GetAll');
  }
}

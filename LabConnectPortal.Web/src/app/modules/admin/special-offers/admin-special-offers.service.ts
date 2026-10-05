import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { AdminSpecialOfferDetail, AdminSpecialOfferListItem } from './admin-special-offers.types';

@Injectable({ providedIn: 'root' })
export class AdminSpecialOffersService {
  constructor(private _http: ApiHttpService) {}

  getAll() {
    return this._http.get<AdminSpecialOfferListItem[]>('SpecialOffer', 'GetAllForAdmin');
  }

  getById(id: number) {
    return this._http.get<AdminSpecialOfferDetail>('SpecialOffer', 'GetByIdForAdmin', { id });
  }
}

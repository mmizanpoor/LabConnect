import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { environment } from '@env/environment';
import { PublicSpecialOfferCardDto, PublicSpecialOfferDetailDto, SubmitSpecialOfferRequestCommand } from '../profile/special-offers/special-offers.types';

@Injectable({ providedIn: 'root' })
export class PublicSpecialOffersService {
  constructor(private _http: ApiHttpService) {}

  getActiveForHome() {
    return this._http.get<PublicSpecialOfferCardDto[]>('PublicSpecialOffer', 'GetActiveForHome');
  }

  getDetail(id: number) {
    return this._http.get<PublicSpecialOfferDetailDto>('PublicSpecialOffer', 'GetDetail', { id });
  }

  submitRequest(id: number, command: SubmitSpecialOfferRequestCommand) {
    return this._http.post('PublicSpecialOffer', 'SubmitRequest', command, { id });
  }

  static labLogoUrl(profileId: string): string {
    return `${environment.apiUrl}PublicSpecialOffer/GetLabLogo?profileId=${profileId}`;
  }
}

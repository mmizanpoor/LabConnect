import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  CreateSpecialOfferCommand,
  RejectSpecialOfferRequestCommand,
  SpecialOfferDashboardStats,
  SpecialOfferDetailDto,
  SpecialOfferListItemDto,
  SpecialOfferRequestDto,
  SpecialOfferRequestForAgreementDto,
  UpdateSpecialOfferCommand,
} from './special-offers.types';

@Injectable({ providedIn: 'root' })
export class SpecialOffersService {
  constructor(private _http: ApiHttpService) {}

  getAll() {
    return this._http.get<SpecialOfferListItemDto[]>('SpecialOffer', 'GetAll');
  }

  getById(id: number) {
    return this._http.get<SpecialOfferDetailDto>('SpecialOffer', 'GetById', { id });
  }

  create(command: CreateSpecialOfferCommand) {
    return this._http.post<SpecialOfferDetailDto>('SpecialOffer', 'Create', command);
  }

  update(command: UpdateSpecialOfferCommand) {
    return this._http.post<SpecialOfferDetailDto>('SpecialOffer', 'Update', command);
  }

  delete(id: number) {
    return this._http.delete('SpecialOffer', 'Delete', { id });
  }

  getRequests(specialOfferId: number) {
    return this._http.get<SpecialOfferRequestDto[]>('SpecialOffer', 'GetRequests', { specialOfferId });
  }

  approveRequest(requestId: number) {
    return this._http.post('SpecialOffer', 'ApproveRequest', null, { requestId });
  }

  rejectRequest(command: RejectSpecialOfferRequestCommand) {
    return this._http.post('SpecialOffer', 'RejectRequest', command);
  }

  getRequestForAgreement(requestId: number) {
    return this._http.get<SpecialOfferRequestForAgreementDto>('SpecialOffer', 'GetRequestForAgreement', { requestId });
  }

  async getDashboardStats(): Promise<SpecialOfferDashboardStats> {
    const result = await this._http.get<SpecialOfferDashboardStats>('SpecialOffer', 'GetDashboardStats');
    if (!result.success || !result.data) {
      throw new Error(result.message ?? 'خطا در بارگذاری آمار پیشنهادات');
    }
    return result.data;
  }
}

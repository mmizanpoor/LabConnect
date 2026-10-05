import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  GetMyOrdersQuery,
  PagedMyOrders,
  ProductOrderDto,
} from './orders.types';

@Injectable({ providedIn: 'root' })
export class OrdersService {
  constructor(private _http: ApiHttpService) {}

  getMyOrders(query: GetMyOrdersQuery) {
    return this._http.post<PagedMyOrders>('ProductOrder', 'GetMyOrders', query);
  }

  getMyOrderById(orderId: string) {
    return this._http.get<ProductOrderDto>('ProductOrder', 'GetMyOrderById', { orderId });
  }

  cancel(orderId: string) {
    return this._http.post<ProductOrderDto>('ProductOrder', 'Cancel', { orderId });
  }

  confirmDelivery(orderId: string) {
    return this._http.post<ProductOrderDto>('ProductOrder', 'ConfirmDelivery', { orderId });
  }
}

import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  AdminProductOrderListItemDto,
  GetAdminOrdersQuery,
  PagedAdminOrders,
  ProductOrderDto,
} from './admin-product-orders.types';

@Injectable({ providedIn: 'root' })
export class AdminProductOrdersService {
  constructor(private _http: ApiHttpService) {}

  getAdminOrders(query: GetAdminOrdersQuery) {
    return this._http.post<PagedAdminOrders>('ProductOrder', 'GetAdminOrders', query);
  }

  getAdminOrderById(orderId: string) {
    return this._http.get<ProductOrderDto>('ProductOrder', 'GetAdminOrderById', { orderId });
  }
}

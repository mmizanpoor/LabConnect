import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  GetCenterOrdersQuery,
  GetShipmentItemsQuery,
  PagedCenterOrders,
  PagedShipmentItems,
  ProductOrderDto,
  ProductOrderShipmentItemDto,
} from './product-orders.types';

@Injectable({ providedIn: 'root' })
export class ProductOrdersService {
  constructor(private _http: ApiHttpService) {}

  getCenterOrders(query: GetCenterOrdersQuery) {
    return this._http.post<PagedCenterOrders>('ProductOrder', 'GetCenterOrders', query);
  }

  getCenterOrderById(orderId: string) {
    return this._http.get<ProductOrderDto>('ProductOrder', 'GetCenterOrderById', { orderId });
  }

  markPaid(orderId: string) {
    return this._http.post<ProductOrderDto>('ProductOrder', 'MarkPaid', { orderId });
  }

  complete(orderId: string) {
    return this._http.post<ProductOrderDto>('ProductOrder', 'Complete', { orderId });
  }

  cancelCenterOrder(orderId: string) {
    return this._http.post<ProductOrderDto>('ProductOrder', 'CancelCenterOrder', { orderId });
  }

  getShipmentItems(query: GetShipmentItemsQuery) {
    return this._http.post<PagedShipmentItems>('ProductOrder', 'GetShipmentItems', query);
  }

  markOrderItemShipped(orderItemId: string) {
    return this._http.post<ProductOrderShipmentItemDto>('ProductOrder', 'MarkOrderItemShipped', {
      orderItemId,
    });
  }

  markOrderShipped(command: {
    orderId: string;
    trackingCode?: string | null;
    shippingCompany?: string | null;
  }) {
    return this._http.post<ProductOrderDto>('ProductOrder', 'MarkOrderShipped', command);
  }

  markOrderDelivered(command: { orderId: string; notes?: string | null }) {
    return this._http.post<ProductOrderDto>('ProductOrder', 'MarkOrderDelivered', command);
  }
}

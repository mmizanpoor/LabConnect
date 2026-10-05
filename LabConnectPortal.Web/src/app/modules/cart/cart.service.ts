import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  AddToCartCommand,
  CartDto,
  CheckoutCommand,
  CheckoutResultDto,
  ProductOrderDto,
  UpdateCartItemCommand,
} from './cart.types';

@Injectable({ providedIn: 'root' })
export class CartService {
  constructor(private _http: ApiHttpService) {}

  getMyCart() {
    return this._http.get<CartDto>('Cart', 'GetMyCart');
  }

  addItem(command: AddToCartCommand) {
    return this._http.post<CartDto>('Cart', 'AddItem', command);
  }

  updateItem(command: UpdateCartItemCommand) {
    return this._http.post<CartDto>('Cart', 'UpdateItem', command);
  }

  removeItem(cartItemId: string) {
    return this._http.post<CartDto>('Cart', 'RemoveItem', { cartItemId });
  }

  clear() {
    return this._http.post<CartDto>('Cart', 'Clear', {});
  }

  checkout(command: CheckoutCommand) {
    return this._http.post<ProductOrderDto>('ProductOrder', 'Checkout', command);
  }

  checkoutCart(command: CheckoutCommand) {
    return this._http.post<CheckoutResultDto>('ProductOrder', 'CheckoutCart', command);
  }

  confirmPayment(orderId: string) {
    return this._http.post<ProductOrderDto>('ProductOrder', 'ConfirmPayment', { orderId });
  }
}

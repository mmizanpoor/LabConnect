import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { CartService } from './cart.service';
import { GuestCartService } from './guest-cart.service';

@Injectable({ providedIn: 'root' })
export class CartStateService {
  private _itemCount$ = new BehaviorSubject<number>(0);
  readonly itemCount$ = this._itemCount$.asObservable();

  constructor(
    private _guestCartService: GuestCartService,
    private _cartService: CartService,
    private _authUtils: AuthUtils,
  ) {
    this.refresh();
  }

  get itemCount(): number {
    return this._itemCount$.value;
  }

  refresh(): void {
    if (this._authUtils.isAuthenticated) {
      void this._cartService
        .getMyCart()
        .then((result) => {
          if (result.success && result.data) {
            this._itemCount$.next(result.data.items.reduce((sum, item) => sum + item.quantity, 0));
            return;
          }
          this._itemCount$.next(0);
        })
        .catch(() => this._itemCount$.next(0));
      return;
    }

    this._itemCount$.next(this._guestCartService.itemCount());
  }

  notifyChanged(): void {
    this.refresh();
  }
}

import { Component, OnInit, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { DecimalPipe } from '@angular/common';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { PublicProductsService } from '../products/products.service';
import { CartStateService } from './cart-state.service';
import { CartService } from './cart.service';
import { CartDto, CartItemDto } from './cart.types';
import { GuestCartService } from './guest-cart.service';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [
    RouterLink,
    MatButtonModule,
    MatIconModule,
    DecimalPipe,
    TranslocoPipe,
    BaseButtonComponent,
  ],
  templateUrl: './cart.component.html',
})
export class CartComponent implements OnInit {
  private _authUtils = inject(AuthUtils);
  private _cartState = inject(CartStateService);

  loading = false;
  error = '';
  cart: CartDto | null = null;
  isGuestCart = false;
  imageUrl = PublicProductsService.productImageUrl;

  constructor(
    private _cartService: CartService,
    private _guestCartService: GuestCartService,
    private _router: Router,
    private _localization: LocalizationService,
  ) {}

  get isAuthenticated(): boolean {
    return this._authUtils.isAuthenticated;
  }

  get itemCount(): number {
    return this.cart?.items?.reduce((sum, item) => sum + item.quantity, 0) ?? 0;
  }

  get itemsOriginalTotal(): number {
    if (!this.cart?.items?.length) return 0;
    return this.cart.items.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0);
  }

  get savingsAmount(): number {
    if (!this.cart) return 0;
    return Math.max(0, this.itemsOriginalTotal - this.cart.totalAmount);
  }

  get savingsPercent(): number {
    if (this.itemsOriginalTotal <= 0) return 0;
    return Math.round((this.savingsAmount / this.itemsOriginalTotal) * 100);
  }

  ngOnInit(): void {
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      if (this._authUtils.isAuthenticated) {
        this.isGuestCart = false;
        if (this._guestCartService.hasItems()) {
          try {
            await this._guestCartService.syncToServer(this._cartService);
          } catch (e: unknown) {
            this.error = e instanceof Error ? e.message : this._localization.translate('modules.cart.errors.loadFailed');
          }
        }
        const result = await this._cartService.getMyCart();
        if (!result.success || !result.data) {
          throw new Error(result.message ?? this._localization.translate('modules.cart.errors.loadFailed'));
        }
        this.cart = result.data;
      } else {
        this.isGuestCart = true;
        this.cart = this._guestCartService.getCartDto();
      }
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.cart.errors.loadFailed');
    } finally {
      this.loading = false;
      this._cartState.notifyChanged();
    }
  }

  async updateQuantity(item: CartItemDto, quantity: number): Promise<void> {
    if (quantity < 1) return;

    if (quantity > item.stockQuantity) {
      this.error = this._localization.translate('modules.cart.errors.stockExceeded');
      return;
    }

    this.error = '';

    if (this.isGuestCart) {
      const updated = this._guestCartService.updateQuantity(item.id, quantity);
      if (!updated) {
        this.error = this._localization.translate('modules.cart.errors.stockExceeded');
        return;
      }
      this.cart = this._guestCartService.getCartDto();
      this._cartState.notifyChanged();
      return;
    }

    const result = await this._cartService.updateItem({ cartItemId: item.id, quantity });
    if (result.success && result.data) {
      this.cart = result.data;
      this._cartState.notifyChanged();
    } else {
      this.error = result.message ?? this._localization.translate('modules.cart.errors.stockExceeded');
    }
  }

  async removeItem(item: CartItemDto): Promise<void> {
    if (this.isGuestCart) {
      this._guestCartService.removeItem(item.id);
      this.cart = this._guestCartService.getCartDto();
      this._cartState.notifyChanged();
      return;
    }

    const result = await this._cartService.removeItem(item.id);
    if (result.success && result.data) {
      this.cart = result.data;
      this._cartState.notifyChanged();
    }
  }

  goToCheckout(): void {
    void this._router.navigate(['/checkout']);
  }

  loginReturnUrl(): string {
    return '/cart';
  }

  isMaxQuantity(item: CartItemDto): boolean {
    return item.stockQuantity > 0 && item.quantity >= item.stockQuantity;
  }
}

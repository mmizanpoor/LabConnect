import { Component, OnInit, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { DecimalPipe } from '@angular/common';
import { TranslocoPipe } from '@jsverse/transloco';
import { ProfileDto } from '@core/services/auth/auth.types';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { ProfileService } from '../profile/profile.service';
import { PublicProductsService } from '../products/products.service';
import { CartStateService } from './cart-state.service';
import { CartService } from './cart.service';
import {
  CartDto,
  ProductOrderDto,
} from './cart.types';
import { GuestCartService } from './guest-cart.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatIconModule,
    DecimalPipe,
    TranslocoPipe,
    BaseButtonComponent,
  ],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.scss',
})
export class CheckoutComponent implements OnInit {
  private _guestCartService = inject(GuestCartService);
  private _cartState = inject(CartStateService);

  loading = false;
  submitting = false;
  paying = false;
  error = '';
  success = '';
  addressError = '';
  replaceExistingCenterItems = false;
  cart: CartDto | null = null;
  profile: ProfileDto | null = null;
  order: ProductOrderDto | null = null;
  imageUrl = PublicProductsService.productImageUrl;

  constructor(
    private _cartService: CartService,
    private _profileService: ProfileService,
    private _router: Router,
    private _localization: LocalizationService,
  ) {}

  get itemCount(): number {
    return this.cart?.items?.reduce((sum, item) => sum + item.quantity, 0) ?? 0;
  }

  get deliveryAddress(): string {
    return this.profile?.address?.trim() ?? '';
  }

  get deliveryPhone(): string {
    return this.profile?.phone?.trim() || this.profile?.mobileNumber?.trim() || '';
  }

  get deliveryRecipient(): string {
    const name = `${this.profile?.firstName ?? ''} ${this.profile?.lastName ?? ''}`.trim();
    return name || this.profile?.username || this.deliveryPhone;
  }

  get shippingCost(): number {
    return this.cart?.defaultShippingCost ?? 0;
  }

  get payableTotal(): number {
    return (this.cart?.totalAmount ?? 0) + this.shippingCost;
  }

  ngOnInit(): void {
    void this.initialize();
  }

  async initialize(): Promise<void> {
    this.loading = true;
    this.error = '';
    this.addressError = '';
    try {
      if (this._guestCartService.hasItems()) {
        try {
          await this._guestCartService.syncToServer(this._cartService);
          this._cartState.notifyChanged();
        } catch (e: unknown) {
          this.error = e instanceof Error ? e.message : this._localization.translate('modules.checkout.errors.checkoutFailed');
        }
      }

      const [cartResult, profile] = await Promise.all([
        this._cartService.getMyCart(),
        this._profileService.getProfile(),
      ]);

      if (!cartResult.success || !cartResult.data) {
        throw new Error(cartResult.message ?? this._localization.translate('modules.cart.errors.loadFailed'));
      }

      if (!cartResult.data.items.length) {
        void this._router.navigate(['/cart']);
        return;
      }

      this.cart = cartResult.data;
      this.profile = profile;
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.checkout.errors.checkoutFailed');
    } finally {
      this.loading = false;
    }
  }

  async submitOrder(): Promise<void> {
    if (!this.cart?.items?.length) return;

    this.addressError = '';
    this.error = '';

    const address = this.deliveryAddress;
    const phone = this.deliveryPhone;
    const recipient = this.deliveryRecipient;

    if (!address) {
      this.addressError = this._localization.translate('modules.checkout.addressRequired');
      return;
    }

    this.submitting = true;
    try {
      const result = await this._cartService.checkoutCart({
        replaceExistingCenterItems: this.replaceExistingCenterItems,
        shippingRecipientName: recipient,
        shippingAddress: address,
        shippingPhone: phone,
        shippingMethod: 'Post',
      });
      if (!result.success || !result.data?.order) {
        throw new Error(result.message ?? this._localization.translate('modules.checkout.errors.checkoutFailed'));
      }
      this.order = result.data.order;
      this._cartState.notifyChanged();
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.checkout.errors.checkoutFailed');
    } finally {
      this.submitting = false;
    }
  }

  async confirmPayment(): Promise<void> {
    if (!this.order) return;
    this.paying = true;
    this.error = '';
    try {
      const result = await this._cartService.confirmPayment(this.order.id);
      if (!result.success) {
        throw new Error(result.message ?? this._localization.translate('modules.checkout.errors.paymentFailed'));
      }
      this.success = this._localization.translate('modules.checkout.paymentSuccess');
      void this._router.navigate(['/orders', this.order.id]);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.checkout.errors.paymentFailed');
    } finally {
      this.paying = false;
    }
  }
}

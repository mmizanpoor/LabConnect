import { Component, Input } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { PublicProductsService } from './products.service';
import { PublicProductListingCardDto } from './products.types';

export type ProductCardLayout = 'default' | 'storefront' | 'home';

const MS_PER_DAY = 24 * 60 * 60 * 1000;

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [
    RouterLink,
    MatCardModule,
    MatIconModule,
    DecimalPipe,
    TranslocoPipe,
  ],
  templateUrl: './product-card.component.html',
  styleUrl: './product-card.component.scss',
  host: {
    '[class.product-card--storefront]': 'layout === "storefront"',
    '[class.product-card--home]': 'layout === "home"',
  },
})
export class ProductCardComponent {
  @Input({ required: true }) product!: PublicProductListingCardDto;
  @Input() layout: ProductCardLayout = 'default';

  imageUrl = PublicProductsService.productImageUrl;
  centerLogoUrl = PublicProductsService.centerLogoUrl;

  cardImageUrl(): string {
    const width =
      this.layout === 'home' ? 400 : this.layout === 'storefront' ? 480 : 640;
    return PublicProductsService.productImageUrl(this.product.imagePath, width);
  }

  constructor(private _localization: LocalizationService) {}

  get badgeLabel(): string {
    if (this.product.isNegotiablePrice) return '';
    const discount = this.product.discountPercent ?? 0;
    if (discount <= 0) return '';
    if (discount >= 15) {
      return this._localization.translate('modules.home.products.specialSale');
    }
    return this._localization.translate('modules.home.products.discountBadge', {
      percent: this.formatPrice(discount),
    });
  }

  get storefrontDiscountLabel(): string {
    if (this.product.isNegotiablePrice) return '';
    const discount = this.product.discountPercent ?? 0;
    if (discount <= 0) return '';
    return this._localization.translate('modules.products.discountPercentBadge', {
      percent: this.formatPrice(discount),
    });
  }

  get showOriginalPrice(): boolean {
    if (this.product.isNegotiablePrice) return false;
    const discount = this.product.discountPercent ?? 0;
    return discount > 0 && (this.product.price ?? 0) > (this.product.finalPrice ?? 0);
  }

  get categoryLabel(): string {
    const group = this.product.categoryGroupTitle?.trim() ?? '';
    const category = this.product.categoryTitle?.trim() ?? '';
    if (group && category) return `${group} / ${category}`;
    return group || category;
  }

  get relativePublishedLabel(): string {
    const publishedAt = this.product.publishedAt;
    if (!publishedAt) return '';

    const published = new Date(publishedAt);
    if (Number.isNaN(published.getTime())) return '';

    const days = Math.max(
      0,
      Math.floor((Date.now() - published.getTime()) / MS_PER_DAY)
    );

    if (days < 30) {
      return this._localization.translate(
        'modules.products.relativeTime.daysAgo',
        {
          count: this.formatPrice(Math.max(days, 1)),
        }
      );
    }

    if (days < 365) {
      const months = Math.max(1, Math.floor(days / 30));
      return this._localization.translate(
        'modules.products.relativeTime.monthsAgo',
        {
          count: this.formatPrice(months),
        }
      );
    }

    const years = Math.max(1, Math.floor(days / 365));
    if (years === 1) {
      return this._localization.translate(
        'modules.products.relativeTime.oneYearAgo'
      );
    }
    return this._localization.translate(
      'modules.products.relativeTime.yearsAgo',
      {
        count: this.formatPrice(years),
      }
    );
  }

  get isStorefrontLike(): boolean {
    return this.layout === 'storefront' || this.layout === 'home';
  }

  get showCenterLogo(): boolean {
    return !!this.product.hasCenterLogo && !!this.product.centerProfileId;
  }

  formatPrice(value: number | null | undefined): string {
    return new Intl.NumberFormat('fa-IR', {
      useGrouping: true,
      maximumFractionDigits: 0,
    }).format(value ?? 0);
  }

  formatPriceLabel(value: number | null | undefined): string {
    return `${this.formatPrice(value)} ${this._localization.translate(
      'shared.currency'
    )}`;
  }
}

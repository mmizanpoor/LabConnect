import { Component, Input, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { PublicAdvertisementCardDto } from '../../admin/advertisements/advertisements.types';
import { PublicAdvertisementsService } from '../../admin/advertisements/advertisements.service';
import { PublicProductsService } from '../../products/products.service';
import { AdvertisementPreviewDialogComponent } from '../advertisement-preview-dialog.component';

@Component({
  selector: 'app-home-promotion-posts',
  standalone: true,
  imports: [MatIconModule, TranslocoPipe],
  templateUrl: './home-promotion-posts.component.html',
  styles: [
    `
      .promo-grid {
        display: grid;
        grid-template-columns: 1fr;
        gap: 1.15rem;
      }

      .promo-card {
        --promo-accent: #198754;
        --promo-accent-hover: #157347;
        --promo-accent-soft: #f3faf6;
        --promo-accent-ink: #ffffff;
        --promo-accent-ring: rgb(25 135 84 / 0.18);
        position: relative;
        display: flex;
        align-items: stretch;
        width: 100%;
        min-height: 16.5rem;
        padding: 0;
        overflow: hidden;
        appearance: none;
        cursor: pointer;
        text-align: start;
        font: inherit;
        color: #0f172a;
        background: #fff;
        border: 1px solid #edf1f6;
        border-radius: 1rem;
        box-shadow: 0 8px 24px rgb(15 23 42 / 0.05);
        transition:
          transform 0.22s ease,
          box-shadow 0.22s ease,
          border-color 0.22s ease;
      }

      .promo-card::before {
        content: '';
        position: absolute;
        inset-block: 0;
        inset-inline-start: 0;
        width: 4px;
        background: var(--promo-accent);
      }

      .promo-card:hover {
        transform: translateY(-2px);
        border-color: var(--promo-accent-ring);
        box-shadow: 0 14px 32px rgb(15 23 42 / 0.08);
      }

      .promo-card--yellow {
        --promo-accent: #f4b400;
        --promo-accent-hover: #d9a200;
        --promo-accent-soft: #fffbeb;
        --promo-accent-ink: #1e293b;
        --promo-accent-ring: rgb(244 180 0 / 0.28);
      }

      .promo-card__body {
        display: flex;
        flex: 1 1 auto;
        min-width: 0;
        flex-direction: column;
        align-items: flex-start;
        justify-content: flex-start;
        gap: 0.7rem;
        padding: 1.5rem 1.5rem 1.4rem 1.25rem;
      }

      .promo-card__badge {
        display: inline-flex;
        align-items: center;
        gap: 0.3rem;
        min-height: 1.5rem;
        padding: 0 0.65rem 0 0.45rem;
        border-radius: 999px;
        background: var(--promo-accent-soft);
        color: var(--promo-accent);
        font-size: 0.75rem;
        font-weight: 700;
        letter-spacing: 0.01em;
        line-height: 1;
      }

      .promo-card__badge mat-icon {
        width: 0.95rem;
        height: 0.95rem;
        font-size: 0.95rem;
        line-height: 1;
      }

      .promo-card--yellow .promo-card__badge {
        color: #9a7200;
      }

      .promo-card__title,
      .promo-card__desc {
        margin: 0;
        max-width: 100%;
        display: -webkit-box;
        -webkit-box-orient: vertical;
        overflow: hidden;
      }

      .promo-card__title {
        color: #0f172a;
        font-size: clamp(1.0625rem, 1.6vw, 1.25rem);
        font-weight: 850;
        line-height: 1.45;
        -webkit-line-clamp: 2;
        line-clamp: 2;
      }

      .promo-card__desc {
        color: #94a3b8;
        font-size: 0.875rem;
        font-weight: 400;
        line-height: 1.7;
        -webkit-line-clamp: 2;
        line-clamp: 2;
      }

      .promo-card__cta {
        display: inline-flex;
        align-items: center;
        gap: 0.2rem;
        min-height: 2.25rem;
        margin-top: auto;
        padding: 0 0.85rem 0 0.55rem;
        border-radius: 0.65rem;
        background: var(--promo-accent);
        color: var(--promo-accent-ink);
        font-size: 0.8125rem;
        font-weight: 600;
        line-height: 1;
        transition: background-color 0.2s ease;
      }

      .promo-card__cta mat-icon {
        width: 1.1rem;
        height: 1.1rem;
        font-size: 1.1rem;
        line-height: 1;
      }

      .promo-card:hover .promo-card__cta {
        background: var(--promo-accent-hover);
      }

      .promo-card__media {
        display: flex;
        flex: 0 0 16.5rem;
        align-items: center;
        justify-content: center;
        width: 16.5rem;
        min-height: 16.5rem;
        padding: 1.15rem;
        background: var(--promo-accent-soft);
        border-inline-start: 1px solid #f1f5f9;
      }

      .promo-card__media img {
        display: block;
        width: 100%;
        height: 12.5rem;
        object-fit: contain;
        object-position: center;
      }

      .promo-card__placeholder {
        display: flex;
        width: 3.5rem;
        height: 3.5rem;
        align-items: center;
        justify-content: center;
        border-radius: 0.9rem;
        background: #fff;
        color: var(--promo-accent);
        box-shadow: 0 6px 16px rgb(15 23 42 / 0.06);
      }

      .promo-card__placeholder mat-icon {
        width: 1.75rem;
        height: 1.75rem;
        font-size: 1.75rem;
        line-height: 1;
      }

      @media (min-width: 900px) {
        .promo-grid {
          grid-template-columns: repeat(2, minmax(0, 1fr));
        }
      }

      @media (min-width: 640px) and (max-width: 1023px) {
        .promo-card {
          min-height: 14.5rem;
        }

        .promo-card__body {
          padding: 1.25rem 1.25rem 1.2rem 1rem;
        }

        .promo-card__media {
          flex: 0 0 42%;
          width: 42%;
          min-height: 14.5rem;
          padding: 0.9rem;
        }

        .promo-card__media img {
          height: 10.5rem;
        }
      }

      @media (max-width: 639px) {
        .promo-grid {
          gap: 0.875rem;
        }

        .promo-card {
          flex-direction: column-reverse;
          min-height: 0;
        }

        .promo-card__body {
          padding: 1.1rem 1rem 1.15rem;
        }

        .promo-card__media {
          flex: none;
          width: 100%;
          min-height: 13.25rem;
          border-inline-start: 0;
          border-block-end: 1px solid #f1f5f9;
        }

        .promo-card__media img {
          height: 11rem;
        }
      }

      @media (prefers-reduced-motion: reduce) {
        .promo-card,
        .promo-card__cta {
          transition: none;
        }

        .promo-card:hover {
          transform: none;
        }
      }
    `,
  ],
})
export class HomePromotionPostsComponent {
  private readonly _dialog = inject(MatDialog);

  @Input({ required: true }) ads: PublicAdvertisementCardDto[] = [];
  @Input() themeSet: 'default' | 'vivid' = 'default';

  imageUrl(ad: PublicAdvertisementCardDto): string {
    if (ad.productImagePath) {
      return PublicProductsService.productImageUrl(ad.productImagePath, 640);
    }
    return PublicAdvertisementsService.imageUrl(ad.imagePath, 640);
  }

  accentAt(index: number): 'green' | 'yellow' {
    const isEven = index % 2 === 0;
    if (this.themeSet === 'vivid') return isEven ? 'yellow' : 'green';
    return isEven ? 'green' : 'yellow';
  }

  openPreview(ad: PublicAdvertisementCardDto, event?: Event): void {
    event?.preventDefault();
    event?.stopPropagation();
    this._dialog.open(AdvertisementPreviewDialogComponent, {
      data: ad,
      width: '640px',
      maxWidth: '92vw',
      autoFocus: false,
      disableClose: false,
      panelClass: 'ad-preview-dialog-panel',
    });
  }
}

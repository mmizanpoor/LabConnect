import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { PublicAdvertisementCardDto } from '../admin/advertisements/advertisements.types';
import { PublicAdvertisementsService } from '../admin/advertisements/advertisements.service';
import { PublicProductsService } from '../products/products.service';

export type AdvertisementPreviewDialogData = PublicAdvertisementCardDto;

@Component({
  selector: 'app-advertisement-preview-dialog',
  standalone: true,
  imports: [MatDialogModule, MatIconModule, TranslocoPipe],
  host: { class: 'block overflow-hidden' },
  template: `
    <div class="ad-preview relative overflow-hidden bg-white" dir="rtl">
      <div
        class="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,rgba(82,123,170,0.16),transparent_52%),radial-gradient(ellipse_at_bottom_left,rgba(53,80,124,0.08),transparent_48%),linear-gradient(180deg,#f4f7fb_0%,#ffffff_46%)]"
      ></div>

      <button
        type="button"
        class="ad-preview__close absolute left-3 top-3 z-20 flex h-10 w-10 items-center justify-center rounded-full border-0 bg-white/85 text-slate-500 shadow-[0_8px_22px_rgba(15,23,42,0.14)] backdrop-blur-md transition duration-200 hover:scale-105 hover:bg-white hover:text-slate-800"
        (click)="close()"
        [attr.aria-label]="'shared.close' | transloco"
      >
        <mat-icon class="!h-5 !w-5 !text-[20px]">close</mat-icon>
      </button>

      <div class="relative">
        <div class="ad-preview__stage relative overflow-hidden px-5 pb-2 pt-8 sm:px-8 sm:pt-10">
          <div
            class="pointer-events-none absolute -end-16 -top-16 h-52 w-52 rounded-full bg-[radial-gradient(circle,rgba(82,123,170,0.22),transparent_68%)] blur-2xl"
          ></div>
          <div
            class="pointer-events-none absolute -bottom-20 -start-10 h-56 w-56 rounded-full bg-[radial-gradient(circle,rgba(148,163,184,0.28),transparent_70%)] blur-2xl"
          ></div>
          <div
            class="pointer-events-none absolute inset-x-8 top-6 h-px bg-gradient-to-l from-transparent via-[#35507c]/25 to-transparent"
          ></div>

          <div class="ad-preview__media relative z-[1] mx-auto flex min-h-[14rem] max-w-xl items-center justify-center sm:min-h-[17rem]">
            <span
              class="ad-preview__badge absolute start-3 top-3 z-10 inline-flex items-center gap-1.5 rounded-full border border-[#35507c]/15 bg-white/92 px-3 py-1.5 text-xs font-bold tracking-wide text-[#35507c] shadow-[0_8px_18px_rgba(15,23,42,0.12)] backdrop-blur-md"
            >
              <mat-icon class="!h-5 !w-5 !text-[20px] opacity-90">campaign</mat-icon>
              {{ 'modules.admin.layout.contentAds' | transloco }}
            </span>

            @if (imageUrl) {
              <div class="ad-preview__frame relative w-full">
                <div
                  class="pointer-events-none absolute -inset-3 rounded-[1.75rem] bg-gradient-to-br from-[#35507c]/15 via-white/40 to-[#527baa]/10 blur-[1px]"
                ></div>
                <div
                  class="relative overflow-hidden rounded-[1.35rem] border border-white/80 bg-gradient-to-br from-[#eef2f7] via-white to-[#e4ebf4] shadow-[0_22px_48px_-18px_rgba(53,80,124,0.45)]"
                >
                  <img
                    class="ad-preview__image relative z-[1] mx-auto block max-h-[min(52vh,26rem)] w-full object-contain px-4 py-5 sm:px-6 sm:py-6"
                    [src]="imageUrl"
                    [alt]="data.title"
                  />
                </div>
              </div>
            } @else {
              <div
                class="ad-preview__placeholder flex h-32 w-32 items-center justify-center rounded-[1.75rem] bg-gradient-to-br from-[#35507c] to-[#527baa] text-white shadow-[0_18px_40px_-12px_rgba(53,80,124,0.55)]"
              >
                <mat-icon class="!h-14 !w-14 !text-[3.5rem]">campaign</mat-icon>
              </div>
            }
          </div>
        </div>

        <div class="ad-preview__body relative px-6 pb-8 pt-5 text-center sm:px-9 sm:pb-9 sm:pt-6">
          <h2
            class="ad-preview__title m-0 text-[1.55rem] font-extrabold leading-snug tracking-tight text-slate-900 sm:text-[1.75rem]"
          >
            {{ data.title }}
          </h2>

          @if (data.shortDescription) {
            <p
              class="ad-preview__desc mx-auto mt-3.5 max-w-xl text-[1.02rem] leading-8 text-slate-600 whitespace-pre-line"
            >
              {{ data.shortDescription }}
            </p>
          }
        </div>
      </div>
    </div>
  `,
  styles: `
    :host {
      display: block;
    }

    .ad-preview__close {
      animation: ad-preview-fade 420ms ease-out both;
    }

    .ad-preview__frame,
    .ad-preview__placeholder {
      animation: ad-preview-rise 520ms cubic-bezier(0.22, 1, 0.36, 1) both;
    }

    .ad-preview__image {
      animation: ad-preview-zoom 640ms cubic-bezier(0.22, 1, 0.36, 1) both;
    }

    .ad-preview__badge {
      animation: ad-preview-rise 560ms cubic-bezier(0.22, 1, 0.36, 1) 80ms both;
    }

    .ad-preview__title {
      animation: ad-preview-rise 560ms cubic-bezier(0.22, 1, 0.36, 1) 140ms both;
    }

    .ad-preview__desc {
      animation: ad-preview-rise 560ms cubic-bezier(0.22, 1, 0.36, 1) 200ms both;
    }

    @keyframes ad-preview-fade {
      from {
        opacity: 0;
      }
      to {
        opacity: 1;
      }
    }

    @keyframes ad-preview-rise {
      from {
        opacity: 0;
        transform: translateY(14px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    @keyframes ad-preview-zoom {
      from {
        opacity: 0;
        transform: scale(0.94);
      }
      to {
        opacity: 1;
        transform: scale(1);
      }
    }

    @media (prefers-reduced-motion: reduce) {
      .ad-preview__close,
      .ad-preview__frame,
      .ad-preview__placeholder,
      .ad-preview__image,
      .ad-preview__badge,
      .ad-preview__title,
      .ad-preview__desc {
        animation: none;
      }
    }
  `,
})
export class AdvertisementPreviewDialogComponent {
  readonly imageUrl: string;

  constructor(
    private _dialogRef: MatDialogRef<AdvertisementPreviewDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AdvertisementPreviewDialogData,
  ) {
    this.imageUrl = data.productImagePath
      ? PublicProductsService.productImageUrl(data.productImagePath)
      : PublicAdvertisementsService.imageUrl(data.imagePath);
  }

  close(): void {
    this._dialogRef.close();
  }
}

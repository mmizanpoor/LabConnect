import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { PublicProductsService } from './products.service';

export interface ProductImagePreviewDialogData {
  imagePath: string;
  title: string;
}

@Component({
  selector: 'app-product-image-preview-dialog',
  standalone: true,
  imports: [MatDialogModule, MatIconModule, TranslocoPipe],
  host: { class: 'block overflow-hidden' },
  template: `
    <div class="product-image-preview relative overflow-hidden bg-white" dir="rtl">
      <button
        type="button"
        class="absolute left-3 top-3 z-20 flex h-10 w-10 items-center justify-center rounded-full border-0 bg-white/85 text-slate-500 shadow-[0_8px_22px_rgba(15,23,42,0.14)] backdrop-blur-md transition duration-200 hover:scale-105 hover:bg-white hover:text-slate-800"
        (click)="close()"
        [attr.aria-label]="'shared.close' | transloco"
      >
        <mat-icon class="!h-5 !w-5 !text-[20px]">close</mat-icon>
      </button>

      <div class="px-4 pb-6 pt-10 sm:px-6 sm:pb-8 sm:pt-12">
        <div class="mx-auto flex min-h-[12rem] max-w-3xl items-center justify-center sm:min-h-[16rem]">
          <img
            class="mx-auto block max-h-[min(72vh,40rem)] w-full object-contain"
            [src]="imageUrl"
            [alt]="data.title"
          />
        </div>
        <p class="m-0 mt-4 text-center text-sm font-semibold text-slate-800">
          {{ data.title }}
        </p>
      </div>
    </div>
  `,
})
export class ProductImagePreviewDialogComponent {
  readonly imageUrl: string;

  constructor(
    private _dialogRef: MatDialogRef<ProductImagePreviewDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ProductImagePreviewDialogData,
  ) {
    this.imageUrl = PublicProductsService.productImageUrl(data.imagePath);
  }

  close(): void {
    this._dialogRef.close();
  }
}

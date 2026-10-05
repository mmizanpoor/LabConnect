import { NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { ICellRendererAngularComp } from 'ag-grid-angular';
import { ICellRendererParams } from 'ag-grid-community';

export type ImageThumbCellRendererParams<T = any> = ICellRendererParams<T> & {
  src?: (params: ICellRendererParams<T>) => string;
  alt?: (params: ICellRendererParams<T>) => string;
  className?: string;
};

@Component({
  selector: 'image-thumb-cell-renderer',
  standalone: true,
  imports: [NgIf],
  template: `
    <ng-container *ngIf="url; else empty">
      <img [src]="url" [alt]="altText" [class]="className" />
    </ng-container>
    <ng-template #empty>
      <span class="text-xs text-gray-400">—</span>
    </ng-template>
  `,
})
export class ImageThumbCellRenderer implements ICellRendererAngularComp {
  params!: ImageThumbCellRendererParams;
  url = '';
  altText = '';
  className = 'h-10 w-10 rounded-full border border-gray-200 bg-gray-100 object-contain';

  agInit(params: ImageThumbCellRendererParams): void {
    this.params = params;
    this.className = params.className ?? this.className;
    this.compute();
  }

  refresh(params: ImageThumbCellRendererParams): boolean {
    this.params = params;
    this.className = params.className ?? this.className;
    this.compute();
    return true;
  }

  private compute(): void {
    const srcFn = this.params.src;
    const altFn = this.params.alt;
    this.url = srcFn ? srcFn(this.params) : '';
    this.altText = altFn ? altFn(this.params) : '';
  }
}


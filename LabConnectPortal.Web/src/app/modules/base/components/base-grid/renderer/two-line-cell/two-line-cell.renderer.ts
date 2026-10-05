import { NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { ICellRendererAngularComp } from 'ag-grid-angular';
import { ICellRendererParams } from 'ag-grid-community';

export type TwoLineCellRendererParams<T = any> = ICellRendererParams<T> & {
  title: (params: ICellRendererParams<T>) => string;
  subtitle?: (params: ICellRendererParams<T>) => string;
};

@Component({
  selector: 'two-line-cell-renderer',
  standalone: true,
  imports: [NgIf],
  template: `
    <div class="flex w-full flex-col">
      <span class="font-medium text-slate-800">{{ titleText }}</span>
      <p *ngIf="subtitleText" class="m-0 mt-0.5 line-clamp-1 text-xs text-slate-500">
        {{ subtitleText }}
      </p>
    </div>
  `,
})
export class TwoLineCellRenderer implements ICellRendererAngularComp {
  params!: TwoLineCellRendererParams;
  titleText = '';
  subtitleText = '';

  agInit(params: TwoLineCellRendererParams): void {
    this.params = params;
    this.compute();
  }

  refresh(params: TwoLineCellRendererParams): boolean {
    this.params = params;
    this.compute();
    return true;
  }

  private compute(): void {
    this.titleText = this.params.title ? this.params.title(this.params) : '';
    this.subtitleText = this.params.subtitle ? this.params.subtitle(this.params) : '';
  }
}


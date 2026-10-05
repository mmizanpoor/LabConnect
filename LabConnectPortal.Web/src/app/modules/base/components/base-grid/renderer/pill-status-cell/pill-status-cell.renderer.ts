import { Component } from '@angular/core';
import { ICellRendererAngularComp } from 'ag-grid-angular';
import { ICellRendererParams } from 'ag-grid-community';

export type PillStatusCellRendererParams<T = any> = ICellRendererParams<T> & {
  label: (params: ICellRendererParams<T>) => string;
  className: (params: ICellRendererParams<T>) => string;
};

@Component({
  selector: 'pill-status-cell-renderer',
  standalone: true,
  template: `
    <span class="inline-flex rounded-full px-2.5 py-0.5 text-xs font-semibold" [class]="pillClass">
      {{ pillLabel }}
    </span>
  `,
})
export class PillStatusCellRenderer implements ICellRendererAngularComp {
  params!: PillStatusCellRendererParams;
  pillLabel = '';
  pillClass = '';

  agInit(params: PillStatusCellRendererParams): void {
    this.params = params;
    this.compute();
  }

  refresh(params: PillStatusCellRendererParams): boolean {
    this.params = params;
    this.compute();
    return true;
  }

  private compute(): void {
    this.pillLabel = this.params.label ? this.params.label(this.params) : '';
    this.pillClass = this.params.className ? this.params.className(this.params) : '';
  }
}


import { NgFor, NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ICellRendererAngularComp } from 'ag-grid-angular';
import { ICellRendererParams } from 'ag-grid-community';

export type IconActionItem<T = any> = {
  icon: string;
  tooltip?: string;
  iconClass?: string;
  hidden?: (params: ICellRendererParams<T>) => boolean;
  action: (params: ICellRendererParams<T>) => void;
};

export type IconActionsCellRendererParams<T = any> = ICellRendererParams<T> & {
  actions: IconActionItem<T>[];
};

@Component({
  selector: 'icon-actions-cell-renderer',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatTooltipModule, NgFor, NgIf],
  template: `
    <div class="flex w-full items-center justify-center gap-1">
      <ng-container *ngFor="let a of actions">
        <button
          *ngIf="!a.hidden || !a.hidden(params)"
          mat-icon-button
          type="button"
          class="scale-[0.85]"
          [matTooltip]="a.tooltip"
          (click)="onClick($event, a)"
        >
          <mat-icon [class]="a.iconClass" [svgIcon]="a.icon"></mat-icon>
        </button>
      </ng-container>
    </div>
  `,
})
export class IconActionsCellRenderer implements ICellRendererAngularComp {
  params!: IconActionsCellRendererParams;
  actions: IconActionItem[] = [];

  agInit(params: IconActionsCellRendererParams): void {
    this.params = params;
    this.actions = params.actions ?? [];
  }

  refresh(params: IconActionsCellRendererParams): boolean {
    this.params = params;
    this.actions = params.actions ?? [];
    return true;
  }

  onClick(event: MouseEvent, item: IconActionItem): void {
    event.preventDefault();
    event.stopImmediatePropagation();
    item.action(this.params);
  }
}


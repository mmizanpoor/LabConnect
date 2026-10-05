import { NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ICellRendererAngularComp } from 'ag-grid-angular';
import { ICellRendererParams } from 'ag-grid-community';

@Component({
  selector: 'btn-cell-renderer',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatTooltipModule, NgIf],
  template: `
    @if (!params.haveText) {
      <button
        *ngIf="!params.hidden"
        mat-icon-button
        class="scale-[0.85]"
        [matTooltip]="params?.tooltip"
        (click)="buttonClicked($event)"
      >
        <mat-icon [svgIcon]="params.icon" [class]="params?.iconClass"></mat-icon>
      </button>
    } @else {
      <button
        *ngIf="!params.hidden"
        class="scale-[0.85]"
        [matTooltip]="params?.tooltip"
        (click)="buttonClicked($event)"
      >
        <span [class]="params?.iconClass">{{ params.icon }}</span>
      </button>
    }
  `,
  styles: `
    :host {
      display: flex;
      align-items: center;
      justify-content: center;
      height: 100%;
    }
  `,
})
export class ButtonCellRenderer implements ICellRendererAngularComp {
  protected params: any;

  refresh(params: ICellRendererParams): boolean {
    this.params = params;
    return true;
  }

  agInit(params: ICellRendererParams): void {
    this.params = params;
  }

  buttonClicked(event: Event): void {
    event.preventDefault();
    event.stopImmediatePropagation();
    this.params.action(this.params);
  }
}

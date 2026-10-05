import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';

@Component({
  selector: 'base-back-button',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatIconModule, TranslocoPipe],
  template: `
    <a
      mat-icon-button
      [routerLink]="link"
      [attr.aria-label]="ariaLabelKey | transloco"
      class="base-back-button !text-slate-600 hover:!bg-slate-100"
    >
      <mat-icon class="base-back-button__icon">arrow_forward</mat-icon>
    </a>
  `,
  styles: `
    :host {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      line-height: 0;
    }

    :host ::ng-deep .base-back-button.mat-mdc-icon-button {
      position: relative;
      display: inline-flex !important;
      align-items: center;
      justify-content: center;
      width: 2.5rem;
      height: 2.5rem;
      padding: 0;
      line-height: 0;

      .mat-mdc-button-touch-target {
        width: 100%;
        height: 100%;
      }

      .mdc-icon-button__focus-ring,
      .mat-mdc-button-persistent-ripple {
        inset: 0;
      }
    }

    :host ::ng-deep .base-back-button__icon.mat-icon {
      position: absolute;
      top: 50%;
      left: 50%;
      display: block !important;
      width: 1.25rem !important;
      height: 1.25rem !important;
      margin: 0 !important;
      font-size: 1.25rem !important;
      line-height: 1.25rem !important;
      transform: translate(-50%, -50%);
    }
  `,
})
export class BaseBackButtonComponent {
  @Input({ required: true }) link!: string | any[];
  @Input() ariaLabelKey = 'shared.backToList';
}

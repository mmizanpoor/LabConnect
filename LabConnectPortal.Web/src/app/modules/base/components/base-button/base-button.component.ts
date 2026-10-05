import { Component, Input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';

export type BaseButtonColor = 'primary' | 'soft' | 'blue' | 'green' | 'accent' | 'warn' | 'orange';

@Component({
  selector: 'base-button',
  standalone: true,
  imports: [MatButtonModule, MatIcon],
  template: `
    <button
      mat-flat-button
      [color]="matColor"
      [disabled]="disabled || loading"
      [type]="type"
      class="base-button-root w-full rounded-md font-medium text-white transition duration-150
             hover:brightness-95 active:brightness-90 disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:brightness-100"
      [class.base-button--md]="size === 'md'"
      [class.base-button--sm]="size === 'sm'"
      [class.base-button--primary]="color === 'primary'"
      [class.base-button--soft]="color === 'soft'"
      [class.base-button--blue]="color === 'blue'"
      [class.base-button--green]="color === 'green'"
      [class.base-button--accent]="color === 'accent'"
      [class.base-button--warn]="color === 'warn'"
      [class.base-button--orange]="color === 'orange'"
    >
      <div class="flex items-center justify-center gap-2">
        @if (loading) {
          <mat-icon
            svgIcon="heroicons_outline:arrow-path"
            class="h-4 w-4 animate-spin text-white"
          ></mat-icon>
        }
        <ng-content></ng-content>
      </div>
    </button>
  `,
  styleUrl: './base-button.component.scss',
})
export class BaseButtonComponent {
  @Input() color: BaseButtonColor = 'primary';
  @Input() size: 'sm' | 'md' = 'md';
  @Input() disabled = false;
  @Input() loading = false;
  @Input() type: 'button' | 'submit' = 'button';

  get matColor(): 'primary' | 'accent' | 'warn' | undefined {
    if (this.color === 'warn') return 'warn';
    if (this.color === 'accent') return 'accent';
    return undefined;
  }
}

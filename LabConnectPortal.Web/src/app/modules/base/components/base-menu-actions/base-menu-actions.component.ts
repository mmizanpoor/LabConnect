import { Component, Input } from '@angular/core';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';

export type BaseMenuActionItem = {
  label: string;
  icon?: string;
  iconClass?: string;
  danger?: boolean;
  hidden?: boolean;
  action: () => void;
};

export type BaseMenuActionsTrigger = 'label' | 'icon';

@Component({
  selector: 'base-menu-actions',
  standalone: true,
  imports: [MatDividerModule, MatIconModule, MatMenuModule],
  template: `
    @if (visibleActions.length) {
      @if (trigger === 'icon') {
        <button
          type="button"
          class="user-actions-icon-trigger"
          [matMenuTriggerFor]="menu"
          [attr.aria-label]="triggerLabel || null"
        >
          <mat-icon>more_vert</mat-icon>
        </button>
      } @else {
        <button type="button" class="user-actions-trigger" [matMenuTriggerFor]="menu">
          <span>{{ triggerLabel }}</span>
          <mat-icon class="user-actions-trigger__icon">expand_more</mat-icon>
        </button>
      }

      <!--
        Icon trigger sits on the outer edge of RTL cards.
        xPosition="after" opens toward the page edge (not over title/price).
        MatMenu CDK also flips above when space below is insufficient.
      -->
      <mat-menu
        #menu="matMenu"
        [class]="menuPanelClass"
        [xPosition]="trigger === 'icon' ? 'after' : 'before'"
        yPosition="below"
        [overlapTrigger]="false"
      >
        @for (a of visibleActions; track a.label; let i = $index) {
          @if (a.danger && i > 0) {
            <mat-divider class="user-actions-menu__divider"></mat-divider>
          }
          <button
            mat-menu-item
            type="button"
            [class.user-actions-menu__edit]="!a.danger"
            [class.user-actions-menu__delete]="a.danger"
            (click)="onClick($event, a)"
          >
            @if (a.icon) {
              <mat-icon [class]="a.iconClass" [svgIcon]="a.icon"></mat-icon>
            }
            <span>{{ a.label }}</span>
          </button>
        }
      </mat-menu>
    }
  `,
})
export class BaseMenuActionsComponent {
  @Input() triggerLabel = '';
  @Input() trigger: BaseMenuActionsTrigger = 'label';
  @Input() actions: BaseMenuActionItem[] = [];

  get visibleActions(): BaseMenuActionItem[] {
    return (this.actions ?? []).filter((a) => !a.hidden);
  }

  get menuPanelClass(): string {
    return this.trigger === 'icon'
      ? 'user-actions-menu user-actions-menu--compact'
      : 'user-actions-menu';
  }

  onClick(event: MouseEvent, item: BaseMenuActionItem): void {
    event.preventDefault();
    event.stopImmediatePropagation();
    item.action();
  }
}

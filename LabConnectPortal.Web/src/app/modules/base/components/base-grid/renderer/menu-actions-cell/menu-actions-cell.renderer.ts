import { Component } from '@angular/core';
import { ICellRendererAngularComp } from 'ag-grid-angular';
import { ICellRendererParams } from 'ag-grid-community';
import {
  BaseMenuActionItem,
  BaseMenuActionsComponent,
} from '@modules/base/components/base-menu-actions/base-menu-actions.component';

export type MenuActionItem<T = any> = {
  label: string;
  icon?: string;
  iconClass?: string;
  danger?: boolean;
  hidden?: (params: ICellRendererParams<T>) => boolean;
  action: (params: ICellRendererParams<T>) => void;
};

export type MenuActionsCellRendererParams<T = any> = ICellRendererParams<T> & {
  triggerLabel: string;
  actions: MenuActionItem<T>[];
};

@Component({
  selector: 'menu-actions-cell-renderer',
  standalone: true,
  imports: [BaseMenuActionsComponent],
  styles: [
    `
      :host {
        display: flex;
        width: 100%;
        height: 100%;
        align-items: center;
        justify-content: center;
      }
    `,
  ],
  template: `
    <base-menu-actions [triggerLabel]="triggerLabel" [actions]="mappedActions" />
  `,
})
export class MenuActionsCellRenderer implements ICellRendererAngularComp {
  params!: MenuActionsCellRendererParams;
  actions: MenuActionItem[] = [];
  triggerLabel = '';
  mappedActions: BaseMenuActionItem[] = [];

  /** Row-aware params for hidden/action callbacks (falls back to node.data). */
  get rowParams(): MenuActionsCellRendererParams {
    const data = this.params?.data ?? this.params?.node?.data;
    if (data === this.params?.data) return this.params;
    return { ...this.params, data };
  }

  agInit(params: MenuActionsCellRendererParams): void {
    this.applyParams(params);
  }

  refresh(params: MenuActionsCellRendererParams): boolean {
    this.applyParams(params);
    return true;
  }

  private applyParams(params: MenuActionsCellRendererParams): void {
    this.params = params;
    this.actions = params.actions ?? [];
    this.triggerLabel = params.triggerLabel ?? '';
    const rowParams = this.rowParams;
    this.mappedActions = this.actions.map((a) => ({
      label: a.label,
      icon: a.icon,
      iconClass: a.iconClass,
      danger: a.danger,
      hidden: !!a.hidden?.(rowParams),
      action: () => a.action(rowParams),
    }));
  }
}

import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { DeviceGroupDto } from './device-groups.types';

export type DeviceGroupAction =
  | { type: 'edit'; row: DeviceGroupDto }
  | { type: 'delete'; row: DeviceGroupDto };

@Injectable({ providedIn: 'root' })
export class DeviceGroupsColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<DeviceGroupAction>();

  get(canUpdate = true, canDelete = true): ColDef[] {
    return [
      {
        headerName: this._localization.translate('modules.profile.deviceGroups.columns.id'),
        field: 'id',
        minWidth: 120,
      },
      {
        headerName: this._localization.translate('modules.profile.deviceGroups.columns.title'),
        field: 'title',
        minWidth: 240,
        flex: 1,
        cellClass: 'font-medium text-gray-900',
      },
      {
        headerName: this._localization.translate('shared.actions'),
        suppressHeaderMenuButton: true,
        sortable: false,
        headerClass: 'ag-header-center base-grid-header-cell',
        width: 160,
        minWidth: 160,
        maxWidth: 160,
        flex: 0,
        cellRenderer: MenuActionsCellRenderer,
        cellRendererParams: (p: ICellRendererParams<DeviceGroupDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            ...(canUpdate
              ? [
                  {
                    label: this._localization.translate('shared.edit'),
                    icon: 'heroicons_outline:pencil-square',
                    iconClass: 'text-blue-500',
                    action: (x: ICellRendererParams<DeviceGroupDto>) =>
                      x.data ? this.actionClicked.emit({ type: 'edit', row: x.data }) : undefined,
                  },
                ]
              : []),
            ...(canDelete
              ? [
                  {
                    label: this._localization.translate('shared.delete'),
                    icon: 'heroicons_outline:trash',
                    iconClass: 'text-red-500',
                    danger: true,
                    action: (x: ICellRendererParams<DeviceGroupDto>) =>
                      x.data ? this.actionClicked.emit({ type: 'delete', row: x.data }) : undefined,
                  },
                ]
              : []),
          ],
        }),
      },
    ];
  }
}


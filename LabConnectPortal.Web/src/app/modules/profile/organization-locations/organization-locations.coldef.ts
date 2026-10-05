import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { OrganizationLocationDto } from './organization-locations.types';

export type LocationAction =
  | { type: 'edit'; row: OrganizationLocationDto }
  | { type: 'delete'; row: OrganizationLocationDto };

@Injectable({ providedIn: 'root' })
export class OrganizationLocationsColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<LocationAction>();

  get(canUpdate = true, canDelete = true): ColDef[] {
    return [
      {
        headerName: this._localization.translate('modules.profile.locations.columns.name'),
        field: 'locationName',
        minWidth: 200,
        flex: 1,
        cellClass: 'font-medium text-gray-900',
        valueGetter: (p) => p.data?.locationName || '—',
      },
      {
        headerName: this._localization.translate('modules.profile.locations.columns.province'),
        field: 'provinceName',
        minWidth: 160,
        valueGetter: (p) => p.data?.provinceName || '—',
      },
      {
        headerName: this._localization.translate('modules.profile.locations.columns.address'),
        field: 'address',
        minWidth: 260,
        flex: 2,
        valueGetter: (p) => p.data?.address || '—',
      },
      {
        headerName: this._localization.translate('modules.profile.locations.columns.phone'),
        field: 'phoneNumber',
        minWidth: 160,
        valueGetter: (p) => p.data?.phoneNumber || '—',
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
        cellRendererParams: (p: ICellRendererParams<OrganizationLocationDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            ...(canUpdate
              ? [
                  {
                    label: this._localization.translate('shared.edit'),
                    icon: 'heroicons_outline:pencil-square',
                    iconClass: 'text-blue-500',
                    action: (x: ICellRendererParams<OrganizationLocationDto>) =>
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
                    action: (x: ICellRendererParams<OrganizationLocationDto>) =>
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


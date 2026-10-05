import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { environment } from '@env/environment';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { SiteServiceDto } from '../dashboard/site-services.types';

export type SiteServiceAction =
  | { type: 'edit'; row: SiteServiceDto }
  | { type: 'delete'; row: SiteServiceDto };

@Injectable({ providedIn: 'root' })
export class SiteServicesColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<SiteServiceAction>();

  get(): ColDef[] {
    return [
      {
        field: 'sortOrder',
        headerName: this._localization.translate('modules.admin.dashboard.services.columns.sortOrder'),
        width: 80,
        flex: 0,
      },
      {
        field: 'imagePath',
        headerName: this._localization.translate('modules.admin.dashboard.services.columns.image'),
        width: 120,
        flex: 0,
        cellRenderer: (p: ICellRendererParams<SiteServiceDto>) => {
          const path = p.data?.imagePath?.trim();
          if (!path) return '';
          const src = `${environment.apiUrl}SiteService/GetImage?path=${encodeURIComponent(path)}`;
          return `<img src="${src}" alt="" style="width:40px;height:40px;object-fit:cover;border-radius:8px;" />`;
        },
      },
      {
        field: 'title',
        headerName: this._localization.translate('modules.admin.dashboard.services.columns.title'),
        minWidth: 180,
        flex: 1,
        cellClass: 'font-medium text-gray-900',
      },
      {
        field: 'linkUrl',
        headerName: this._localization.translate('modules.admin.dashboard.services.columns.linkUrl'),
        minWidth: 160,
        flex: 1,
      },
      {
        field: 'isActive',
        headerName: this._localization.translate('modules.admin.dashboard.services.columns.isActive'),
        width: 100,
        flex: 0,
        valueGetter: (p) =>
          p.data?.isActive
            ? this._localization.translate('shared.yes')
            : this._localization.translate('shared.no'),
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
        cellRendererParams: (p: ICellRendererParams<SiteServiceDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate('shared.edit'),
              icon: 'heroicons_outline:pencil-square',
              iconClass: 'text-blue-500',
              action: (params: ICellRendererParams<SiteServiceDto>) =>
                params.data ? this.actionClicked.emit({ type: 'edit', row: params.data }) : undefined,
            },
            {
              label: this._localization.translate('shared.delete'),
              icon: 'heroicons_outline:trash',
              iconClass: 'text-red-500',
              danger: true,
              action: (params: ICellRendererParams<SiteServiceDto>) =>
                params.data ? this.actionClicked.emit({ type: 'delete', row: params.data }) : undefined,
            },
          ],
        }),
      },
    ];
  }
}

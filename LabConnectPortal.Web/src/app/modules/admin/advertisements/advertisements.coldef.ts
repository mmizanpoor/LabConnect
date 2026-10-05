import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { ImageThumbCellRenderer } from '@modules/base/components/base-grid/renderer/image-thumb-cell/image-thumb-cell.renderer';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { AdvertisementsService } from './advertisements.service';
import { AdvertisementDto } from './advertisements.types';

export type AdvertisementAction =
  | { type: 'edit'; row: AdvertisementDto }
  | { type: 'delete'; row: AdvertisementDto };

@Injectable({ providedIn: 'root' })
export class AdvertisementsColDef {
  constructor(
    private _localization: LocalizationService,
    private _service: AdvertisementsService,
  ) {}

  actionClicked = new EventEmitter<AdvertisementAction>();

  get(): ColDef[] {
    return [
      {
        headerName: this._localization.translate('modules.admin.advertisements.columns.image'),
        width: 110,
        minWidth: 110,
        maxWidth: 110,
        sortable: false,
        cellRenderer: ImageThumbCellRenderer,
        cellRendererParams: (p: ICellRendererParams<AdvertisementDto>) => ({
          ...p,
          className: 'h-10 w-14 rounded border border-gray-200 bg-slate-50 object-cover',
          src: (params: ICellRendererParams<AdvertisementDto>) =>
            params.data?.imagePath ? this._service.getImageUrl(params.data.imagePath) : '',
          alt: (params: ICellRendererParams<AdvertisementDto>) => params.data?.title ?? '',
        }),
      },
      {
        field: 'title',
        headerName: this._localization.translate('modules.admin.advertisements.columns.title'),
        minWidth: 220,
        flex: 1,
        cellClass: 'font-medium text-gray-900',
      },
      {
        field: 'startAt',
        headerName: this._localization.translate('modules.admin.advertisements.columns.startAt'),
        minWidth: 140,
        valueGetter: (p) => this.formatDate(p.data?.startAt),
      },
      {
        field: 'endAt',
        headerName: this._localization.translate('modules.admin.advertisements.columns.endAt'),
        minWidth: 140,
        valueGetter: (p) => this.formatDate(p.data?.endAt),
      },
      {
        field: 'isActive',
        headerName: this._localization.translate('modules.admin.advertisements.columns.isActive'),
        minWidth: 110,
        valueGetter: (p) =>
          p.data?.isActive
            ? this._localization.translate('shared.yes')
            : this._localization.translate('shared.no'),
      },
      {
        field: 'createdByUserName',
        headerName: this._localization.translate('modules.admin.advertisements.columns.createdBy'),
        minWidth: 150,
      },
      {
        field: 'createdAt',
        headerName: this._localization.translate('modules.admin.advertisements.columns.createdAt'),
        minWidth: 140,
        valueGetter: (p) => this.formatDate(p.data?.createdAt),
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
        cellRendererParams: (p: ICellRendererParams<AdvertisementDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate('shared.edit'),
              icon: 'heroicons_outline:pencil-square',
              iconClass: 'text-blue-500',
              action: (params: ICellRendererParams<AdvertisementDto>) =>
                params.data
                  ? this.actionClicked.emit({ type: 'edit', row: params.data })
                  : undefined,
            },
            {
              label: this._localization.translate('shared.delete'),
              icon: 'heroicons_outline:trash',
              iconClass: 'text-red-500',
              danger: true,
              action: (params: ICellRendererParams<AdvertisementDto>) =>
                params.data
                  ? this.actionClicked.emit({ type: 'delete', row: params.data })
                  : undefined,
            },
          ],
        }),
      },
    ];
  }

  private formatDate(value?: string | null): string {
    if (!value) return '';
    return new Intl.DateTimeFormat('fa-IR', { dateStyle: 'medium' }).format(new Date(value));
  }
}

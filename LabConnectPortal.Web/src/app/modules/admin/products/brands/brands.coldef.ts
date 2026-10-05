import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ImageThumbCellRenderer } from '@modules/base/components/base-grid/renderer/image-thumb-cell/image-thumb-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { ProductCatalogService } from '../product-catalog.service';
import { BrandDto } from '../product-catalog.types';

export type BrandAction =
  | { type: 'edit'; row: BrandDto }
  | { type: 'delete'; row: BrandDto };

@Injectable({ providedIn: 'root' })
export class BrandsColDef {
  constructor(
    private _localization: LocalizationService,
    private _catalogService: ProductCatalogService,
  ) {}

  actionClicked = new EventEmitter<BrandAction>();

  get(): ColDef[] {
    return [
      {
        headerName: this._localization.translate('modules.admin.brands.columns.image'),
        width: 100,
        minWidth: 100,
        maxWidth: 100,
        sortable: false,
        cellRenderer: ImageThumbCellRenderer,
        cellRendererParams: (p: ICellRendererParams<BrandDto>) => ({
          ...p,
          className: 'h-8 w-12 rounded border border-gray-200 bg-white object-contain',
          src: (params: ICellRendererParams<BrandDto>) =>
            params.data?.imagePath
              ? this._catalogService.getBrandImageUrl(params.data.imagePath)
              : '',
          alt: (params: ICellRendererParams<BrandDto>) => params.data?.title ?? '',
        }),
      },
      {
        field: 'title',
        headerName: this._localization.translate('modules.admin.brands.columns.title'),
        minWidth: 220,
        flex: 1,
        cellClass: 'font-medium text-gray-900',
      },
      {
        field: 'showOnHomePage',
        headerName: this._localization.translate('modules.admin.brands.columns.showOnHomePage'),
        minWidth: 160,
        valueGetter: (p) =>
          p.data?.showOnHomePage
            ? this._localization.translate('modules.admin.brands.showOnHomePageYes')
            : this._localization.translate('modules.admin.brands.showOnHomePageNo'),
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
        cellRendererParams: (p: ICellRendererParams<BrandDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate('shared.edit'),
              icon: 'heroicons_outline:pencil-square',
              iconClass: 'text-blue-500',
              action: (params: ICellRendererParams<BrandDto>) =>
                params.data
                  ? this.actionClicked.emit({ type: 'edit', row: params.data })
                  : undefined,
            },
            {
              label: this._localization.translate('shared.delete'),
              icon: 'heroicons_outline:trash',
              iconClass: 'text-red-500',
              danger: true,
              action: (params: ICellRendererParams<BrandDto>) =>
                params.data
                  ? this.actionClicked.emit({ type: 'delete', row: params.data })
                  : undefined,
            },
          ],
        }),
      },
    ];
  }
}

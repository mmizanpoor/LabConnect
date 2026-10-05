import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { ProductCategoryDto } from '../product-catalog.types';

export type ProductCategoryAction =
  | { type: 'edit'; row: ProductCategoryDto }
  | { type: 'delete'; row: ProductCategoryDto };

@Injectable({ providedIn: 'root' })
export class ProductCategoriesColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<ProductCategoryAction>();

  get(): ColDef[] {
    return [
      {
        field: 'title',
        headerName: this._localization.translate(
          'modules.admin.productCategories.columns.title',
        ),
        minWidth: 200,
        flex: 1,
        cellClass: 'font-medium text-gray-900',
      },
      {
        field: 'acceptsResume',
        headerName: this._localization.translate(
          'modules.admin.productCategories.columns.acceptsResume',
        ),
        width: 160,
        minWidth: 140,
        flex: 0,
        cellRenderer: (p: ICellRendererParams<ProductCategoryDto>) =>
          p.data?.acceptsResume
            ? this._localization.translate(
                'modules.admin.productCategories.showOnHomePageYes',
              )
            : this._localization.translate(
                'modules.admin.productCategories.showOnHomePageNo',
              ),
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
        cellRendererParams: (p: ICellRendererParams<ProductCategoryDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate('shared.edit'),
              icon: 'heroicons_outline:pencil-square',
              iconClass: 'text-blue-500',
              action: (params: ICellRendererParams<ProductCategoryDto>) =>
                params.data
                  ? this.actionClicked.emit({ type: 'edit', row: params.data })
                  : undefined,
            },
            {
              label: this._localization.translate('shared.delete'),
              icon: 'heroicons_outline:trash',
              iconClass: 'text-red-500',
              danger: true,
              action: (params: ICellRendererParams<ProductCategoryDto>) =>
                params.data
                  ? this.actionClicked.emit({
                      type: 'delete',
                      row: params.data,
                    })
                  : undefined,
            },
          ],
        }),
      },
    ];
  }
}

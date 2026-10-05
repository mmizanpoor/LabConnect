import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { ProductAttributeDto } from '../product-catalog.types';

export type ProductAttributeAction =
  | { type: 'edit'; row: ProductAttributeDto }
  | { type: 'delete'; row: ProductAttributeDto };

@Injectable({ providedIn: 'root' })
export class ProductAttributesColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<ProductAttributeAction>();

  get(): ColDef[] {
    return [
      {
        field: 'categoryTitle',
        headerName: this._localization.translate(
          'modules.admin.productAttributes.columns.category',
        ),
        minWidth: 160,
      },
      {
        field: 'title',
        headerName: this._localization.translate(
          'modules.admin.productAttributes.columns.title',
        ),
        minWidth: 220,
        flex: 1,
        cellClass: 'font-medium text-gray-900',
      },
      {
        field: 'fieldType',
        headerName: this._localization.translate(
          'modules.admin.productAttributes.columns.fieldType',
        ),
        minWidth: 140,
        valueFormatter: (params) => {
          const value = params.value as string | number | null | undefined;
          if (value === 'Date' || value === 1 || value === '1') {
            return this._localization.translate(
              'modules.admin.productAttributes.fieldTypes.date',
            );
          }
          if (value === 'ExpiryDate' || value === 2 || value === '2') {
            return this._localization.translate(
              'modules.admin.productAttributes.fieldTypes.expiryDate',
            );
          }
          return this._localization.translate(
            'modules.admin.productAttributes.fieldTypes.none',
          );
        },
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
        cellRendererParams: (p: ICellRendererParams<ProductAttributeDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate('shared.edit'),
              icon: 'heroicons_outline:pencil-square',
              iconClass: 'text-blue-500',
              action: (params: ICellRendererParams<ProductAttributeDto>) =>
                params.data
                  ? this.actionClicked.emit({ type: 'edit', row: params.data })
                  : undefined,
            },
            {
              label: this._localization.translate('shared.delete'),
              icon: 'heroicons_outline:trash',
              iconClass: 'text-red-500',
              danger: true,
              action: (params: ICellRendererParams<ProductAttributeDto>) =>
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

import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { ProductReviewListItemDto } from '@modules/products/products.types';

export type ProductReviewAction =
  | { type: 'approve'; row: ProductReviewListItemDto }
  | { type: 'reject'; row: ProductReviewListItemDto };

@Injectable({ providedIn: 'root' })
export class ProductReviewsAdminColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<ProductReviewAction>();

  get(): ColDef[] {
    return [
      {
        field: 'authorName',
        headerName: this._localization.translate(
          'modules.admin.productReviews.columns.author',
        ),
        minWidth: 160,
        cellClass: 'font-medium text-gray-900',
      },
      {
        field: 'productTitle',
        headerName: this._localization.translate(
          'modules.admin.productReviews.columns.product',
        ),
        minWidth: 200,
        flex: 1,
      },
      {
        field: 'rating',
        headerName: this._localization.translate(
          'modules.admin.productReviews.columns.rating',
        ),
        minWidth: 110,
        valueFormatter: (p) => `${p.value ?? 0}/5`,
      },
      {
        field: 'comment',
        headerName: this._localization.translate(
          'modules.admin.productReviews.columns.comment',
        ),
        minWidth: 260,
        flex: 2,
        cellClass: 'whitespace-pre-line',
      },
      {
        field: 'createdAt',
        headerName: this._localization.translate(
          'modules.admin.productReviews.columns.createdAt',
        ),
        minWidth: 170,
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
        cellRendererParams: (p: ICellRendererParams<ProductReviewListItemDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate('modules.admin.productReviews.approve'),
              icon: 'heroicons_outline:check-circle',
              iconClass: 'text-green-600',
              action: (params: ICellRendererParams<ProductReviewListItemDto>) =>
                params.data
                  ? this.actionClicked.emit({
                      type: 'approve',
                      row: params.data,
                    })
                  : undefined,
            },
            {
              label: this._localization.translate('modules.admin.productReviews.reject'),
              icon: 'heroicons_outline:x-circle',
              iconClass: 'text-red-500',
              danger: true,
              action: (params: ICellRendererParams<ProductReviewListItemDto>) =>
                params.data
                  ? this.actionClicked.emit({
                      type: 'reject',
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

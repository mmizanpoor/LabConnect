import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ImageThumbCellRenderer } from '@modules/base/components/base-grid/renderer/image-thumb-cell/image-thumb-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { ProductCatalogService } from '../product-catalog.service';
import { ProductListItemDto, ProductStatus } from '../product-catalog.types';

export type ProductAction =
  | { type: 'review'; row: ProductListItemDto }
  | { type: 'edit'; row: ProductListItemDto }
  | { type: 'delete'; row: ProductListItemDto }
  | { type: 'resumes'; row: ProductListItemDto };

@Injectable({ providedIn: 'root' })
export class ProductsColDef {
  constructor(
    private _localization: LocalizationService,
    private _catalogService: ProductCatalogService,
  ) {}

  actionClicked = new EventEmitter<ProductAction>();

  get(
    formatJalaliDate: (value?: string | null) => string,
    options?: {
      showReviewAction?: boolean;
      showResumeAction?: boolean;
      canUpdate?: boolean;
      canDelete?: boolean;
      /**
       * Who may get edit/delete row actions.
       * - siteAdminDefined: admin grid (default)
       * - centerOwned: profile catalog of own products
       * - all: show whenever canUpdate/canDelete allow
       */
      mutateScope?: 'siteAdminDefined' | 'centerOwned' | 'all';
    }
  ): ColDef[] {
    const mutateScope = options?.mutateScope ?? 'siteAdminDefined';
    return [
      {
        headerName: this._localization.translate(
          'modules.admin.products.columns.image'
        ),
        width: 100,
        minWidth: 100,
        maxWidth: 100,
        sortable: false,
        cellClass: 'ag-header-center',
        headerClass: 'ag-header-center',
        cellRenderer: ImageThumbCellRenderer,
        cellRendererParams: (p: ICellRendererParams<ProductListItemDto>) => ({
          ...p,
          className:
            'h-10 w-14 rounded border border-gray-200 bg-slate-50 object-contain',
          src: (params: ICellRendererParams<ProductListItemDto>) =>
            params.data?.featuredImagePath
              ? this._catalogService.getProductImageUrl(
                  params.data.featuredImagePath
                )
              : '',
          alt: (params: ICellRendererParams<ProductListItemDto>) =>
            params.data?.title ?? '',
        }),
      },
      {
        field: 'title',
        headerName: this._localization.translate(
          'modules.admin.products.columns.title'
        ),
        minWidth: 220,
        flex: 1,
        cellClass: 'font-medium text-gray-900 ag-header-center',
        headerClass: 'ag-header-center',
      },
      {
        field: 'brandTitle',
        headerName: this._localization.translate(
          'modules.admin.products.columns.brand'
        ),
        width: 100,
        headerClass: 'ag-header-center',
        cellClass: 'ag-header-center',
      },
      {
        field: 'createdByCenterName',
        headerName: this._localization.translate(
          'modules.admin.products.columns.center'
        ),
        width: 150,
        cellClass: 'ag-header-center',
        headerClass: 'ag-header-center',
        valueGetter: (p) => p.data?.createdByCenterName?.trim() || '—',
      },
      {
        field: 'categoriesText',
        headerName: this._localization.translate(
          'modules.admin.products.columns.categories'
        ),
        width: 150,
        cellClass: 'ag-header-center',
        headerClass: 'ag-header-center',
        hide: true,
      },
      {
        field: 'status',
        headerName: this._localization.translate(
          'modules.admin.products.columns.status'
        ),
        width: 100,
        valueGetter: (p) => this.statusLabel(p.data?.status),
        cellClass: 'ag-header-center',
        headerClass: 'ag-header-center',
      },
      {
        headerName: this._localization.translate(
          'modules.admin.products.columns.updatedAt'
        ),
        width: 140,
        valueGetter: (p) => formatJalaliDate(p.data?.updatedAt),
        cellClass: 'ag-header-center',
        headerClass: 'ag-header-center',
      },
      {
        headerName: this._localization.translate('shared.actions'),
        suppressHeaderMenuButton: true,
        sortable: false,
        headerClass: 'ag-header-center base-grid-header-cell',
        width: 100,
        minWidth: 100,
        maxWidth: 100,
        flex: 0,
        cellRenderer: MenuActionsCellRenderer,
        cellRendererParams: (p: ICellRendererParams<ProductListItemDto>) => {
          const actions = [];
          const canMutateRow = this.canMutateRow(p.data, mutateScope);

          if (options?.showReviewAction) {
            actions.push({
              label: this._localization.translate(
                'modules.admin.products.review'
              ),
              icon: 'heroicons_outline:eye',
              iconClass: 'text-teal-600',
              action: (params: ICellRendererParams<ProductListItemDto>) =>
                params.data
                  ? this.actionClicked.emit({
                      type: 'review',
                      row: params.data,
                    })
                  : undefined,
            });
          }

          if (options?.showResumeAction && p.data?.acceptsResume) {
            actions.push({
              label: this._localization.translate(
                'modules.profile.productResumeApplications.view'
              ),
              icon: 'heroicons_outline:document-text',
              iconClass: 'text-teal-600',
              action: (params: ICellRendererParams<ProductListItemDto>) =>
                params.data
                  ? this.actionClicked.emit({
                      type: 'resumes',
                      row: params.data,
                    })
                  : undefined,
            });
          }

          if (options?.canUpdate !== false && canMutateRow) {
            actions.push({
              label: this._localization.translate('shared.edit'),
              icon: 'heroicons_outline:pencil-square',
              iconClass: 'text-blue-500',
              action: (params: ICellRendererParams<ProductListItemDto>) =>
                params.data
                  ? this.actionClicked.emit({ type: 'edit', row: params.data })
                  : undefined,
            });
          }

          if (options?.canDelete !== false && canMutateRow) {
            actions.push({
              label: this._localization.translate('shared.delete'),
              icon: 'heroicons_outline:trash',
              iconClass: 'text-red-500',
              danger: true,
              action: (params: ICellRendererParams<ProductListItemDto>) =>
                params.data
                  ? this.actionClicked.emit({
                      type: 'delete',
                      row: params.data,
                    })
                  : undefined,
            });
          }

          return {
            ...p,
            triggerLabel: this._localization.translate('shared.actions'),
            actions,
          };
        },
      },
    ];
  }

  private canMutateRow(
    row: ProductListItemDto | undefined,
    mutateScope: 'siteAdminDefined' | 'centerOwned' | 'all'
  ): boolean {
    if (!row) return false;
    if (mutateScope === 'all') return true;
    if (mutateScope === 'centerOwned') return row.createdBySiteAdmin === false;
    return row.createdBySiteAdmin !== false;
  }

  private statusLabel(status?: ProductStatus): string {
    switch (status) {
      case 'PendingApproval':
      case 1:
        return this._localization.translate(
          'modules.admin.products.status.pendingApproval'
        );
      case 'Approved':
      case 2:
        return this._localization.translate(
          'modules.admin.products.status.approved'
        );
      case 'Rejected':
      case 3:
        return this._localization.translate(
          'modules.admin.products.status.rejected'
        );
      case 'Unpublished':
      case 4:
        return this._localization.translate(
          'modules.admin.products.status.unpublished'
        );
      default:
        return this._localization.translate(
          'modules.admin.products.status.draft'
        );
    }
  }
}

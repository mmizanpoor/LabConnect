import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ImageThumbCellRenderer } from '@modules/base/components/base-grid/renderer/image-thumb-cell/image-thumb-cell.renderer';
import { ColDef, ICellRendererParams, ITooltipParams, ValueGetterParams } from 'ag-grid-community';
import { ProductCatalogService } from '../product-catalog.service';
import { ProductCategoryGroupDto } from '../product-catalog.types';

export type ProductCategoryGroupAction =
  | { type: 'edit'; row: ProductCategoryGroupDto }
  | { type: 'delete'; row: ProductCategoryGroupDto };

@Injectable({ providedIn: 'root' })
export class ProductCategoryGroupsColDef {
  constructor(
    private _localization: LocalizationService,
    private _catalogService: ProductCatalogService,
  ) {}

  actionClicked = new EventEmitter<ProductCategoryGroupAction>();

  get(): ColDef<ProductCategoryGroupDto>[] {
    return [
      {
        headerName: this._localization.translate(
          'modules.admin.productCategories.categoryGroups.columns.image',
        ),
        width: 90,
        minWidth: 90,
        maxWidth: 90,
        sortable: false,
        cellRenderer: ImageThumbCellRenderer,
        cellRendererParams: (p: ICellRendererParams<ProductCategoryGroupDto>) => ({
          ...p,
          className:
            'h-10 w-10 rounded-full border border-gray-200 bg-gray-100 object-contain',
          src: (params: ICellRendererParams<ProductCategoryGroupDto>) =>
            params.data?.homePageImagePath
              ? this._catalogService.getCategoryGroupHomePageImageUrl(
                  params.data.homePageImagePath,
                )
              : '',
          alt: (params: ICellRendererParams<ProductCategoryGroupDto>) =>
            params.data?.name ?? '',
        }),
      },
      {
        field: 'showOnHomePage',
        headerName: this._localization.translate(
          'modules.admin.productCategories.categoryGroups.columns.showOnHomePage',
        ),
        minWidth: 140,
        valueGetter: (p) =>
          p.data?.showOnHomePage
            ? this._localization.translate(
                'modules.admin.productCategories.showOnHomePageYes',
              )
            : this._localization.translate(
                'modules.admin.productCategories.showOnHomePageNo',
              ),
      },
      {
        field: 'name',
        headerName: this._localization.translate(
          'modules.admin.productCategories.categoryGroups.columns.name',
        ),
        minWidth: 180,
        flex: 1,
        cellClass: 'font-medium text-gray-900',
      },
      {
        field: 'categoryCount',
        headerName: this._localization.translate(
          'modules.admin.productCategories.categoryGroups.columns.categoryCount',
        ),
        width: 120,
        minWidth: 100,
        maxWidth: 140,
      },
      {
        field: 'categories',
        headerName: this._localization.translate(
          'modules.admin.productCategories.categoryGroups.columns.categories',
        ),
        minWidth: 240,
        flex: 2,
        valueGetter: (p: ValueGetterParams<ProductCategoryGroupDto>) =>
          this.formatCategoryTitles(p.data),
        tooltipValueGetter: (p: ITooltipParams<ProductCategoryGroupDto>) =>
          this.formatCategoryTitles(p.data),
        cellStyle: {
          overflow: 'hidden',
          textOverflow: 'ellipsis',
          whiteSpace: 'nowrap',
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
        cellRendererParams: (p: ICellRendererParams<ProductCategoryGroupDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate('shared.edit'),
              icon: 'heroicons_outline:pencil-square',
              iconClass: 'text-blue-500',
              action: (params: ICellRendererParams<ProductCategoryGroupDto>) =>
                params.data
                  ? this.actionClicked.emit({ type: 'edit', row: params.data })
                  : undefined,
            },
            {
              label: this._localization.translate('shared.delete'),
              icon: 'heroicons_outline:trash',
              iconClass: 'text-red-500',
              danger: true,
              action: (params: ICellRendererParams<ProductCategoryGroupDto>) =>
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

  private formatCategoryTitles(group?: ProductCategoryGroupDto | null): string {
    return (group?.categories ?? []).map((c) => c.title).join('، ');
  }
}

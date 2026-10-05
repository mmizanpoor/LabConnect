import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ImageThumbCellRenderer } from '@modules/base/components/base-grid/renderer/image-thumb-cell/image-thumb-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { ContentService } from '../content.service';
import { ContentPostListItemDto } from '../content.types';

export type PostAction =
  | { type: 'edit'; row: ContentPostListItemDto }
  | { type: 'delete'; row: ContentPostListItemDto };

@Injectable({ providedIn: 'root' })
export class PostsColDef {
  constructor(
    private _localization: LocalizationService,
    private _contentService: ContentService,
  ) {}

  actionClicked = new EventEmitter<PostAction>();

  get(): ColDef[] {
    return this.build({ showType: false, showGroup: true });
  }

  getForScope(options: { showType: boolean; showGroup: boolean }): ColDef[] {
    return this.build(options);
  }

  private build(options: { showType: boolean; showGroup: boolean }): ColDef[] {
    const cols: ColDef[] = [
      {
        headerName: this._localization.translate('modules.admin.posts.columns.image'),
        width: 110,
        minWidth: 110,
        maxWidth: 110,
        sortable: false,
        cellRenderer: ImageThumbCellRenderer,
        cellRendererParams: (p: ICellRendererParams<ContentPostListItemDto>) => ({
          ...p,
          className: 'h-10 w-14 rounded border border-gray-200 bg-slate-50 object-cover',
          src: (params: ICellRendererParams<ContentPostListItemDto>) =>
            params.data?.featuredImagePath
              ? this._contentService.getFeaturedImageUrl(params.data.featuredImagePath)
              : '',
          alt: (params: ICellRendererParams<ContentPostListItemDto>) =>
            params.data?.title ?? '',
        }),
      },
      {
        field: 'title',
        headerName: this._localization.translate('modules.admin.posts.columns.title'),
        minWidth: 260,
        flex: 1,
        cellClass: 'font-medium text-gray-900',
      },
    ];

    if (options.showType) {
      cols.push({
        field: 'typeTitle',
        headerName: this._localization.translate('modules.admin.posts.columns.type'),
        minWidth: 140,
      });
    }

    if (options.showGroup) {
      cols.push({
        field: 'groupTitle',
        headerName: this._localization.translate('modules.admin.posts.columns.group'),
        minWidth: 160,
      });
    }

    cols.push(
      {
        field: 'isActive',
        headerName: this._localization.translate('modules.admin.posts.fields.isActive'),
        minWidth: 120,
        valueGetter: (p) =>
          p.data?.isActive
            ? this._localization.translate('shared.yes')
            : this._localization.translate('shared.no'),
      },
      {
        field: 'showOnHomePage',
        headerName: this._localization.translate('modules.admin.posts.columns.showOnHomePage'),
        minWidth: 150,
        valueGetter: (p) =>
          p.data?.showOnHomePage
            ? this._localization.translate('shared.yes')
            : this._localization.translate('shared.no'),
      },
      {
        field: 'viewCount',
        headerName: this._localization.translate('modules.admin.posts.columns.viewCount'),
        minWidth: 120,
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
        cellRendererParams: (p: ICellRendererParams<ContentPostListItemDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate('shared.edit'),
              icon: 'heroicons_outline:pencil-square',
              iconClass: 'text-blue-500',
              action: (params: ICellRendererParams<ContentPostListItemDto>) =>
                params.data
                  ? this.actionClicked.emit({ type: 'edit', row: params.data })
                  : undefined,
            },
            {
              label: this._localization.translate('shared.delete'),
              icon: 'heroicons_outline:trash',
              iconClass: 'text-red-500',
              danger: true,
              action: (params: ICellRendererParams<ContentPostListItemDto>) =>
                params.data
                  ? this.actionClicked.emit({ type: 'delete', row: params.data })
                  : undefined,
            },
          ],
        }),
      },
    );

    return cols;
  }
}

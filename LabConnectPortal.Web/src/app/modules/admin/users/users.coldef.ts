import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { UserDto } from './users.types';

export type UserAction =
  | { type: 'resume'; row: any }
  | { type: 'detail'; row: any }
  | { type: 'delete'; row: any };

@Injectable({ providedIn: 'root' })
export class UsersColDef {
  constructor(private _localization: LocalizationService) {}

  public actionClicked = new EventEmitter<UserAction>();

  get(): ColDef[] {
    return [
      {
        field: 'fullName',
        headerName: this._localization.translate(
          'modules.admin.users.columns.fullName'
        ),
        minWidth: 180,
        flex: 1,
        cellClass: 'font-medium text-gray-900 ag-header-center',
        headerClass: 'ag-header-center',
      },
      {
        field: 'mobileNumber',
        headerName: this._localization.translate('shared.mobile'),
        width: 100,
        cellClass: 'ag-header-center',
        headerClass: 'ag-header-center',
      },
      {
        field: 'email',
        headerName: this._localization.translate('shared.email'),
        minWidth: 200,
        flex: 1,
        cellClass: 'ag-header-center',
        headerClass: 'ag-header-center',
      },
      {
        field: 'userTypeText',
        headerName: this._localization.translate(
          'modules.admin.users.columns.userType'
        ),
        width: 120,
        cellClass: 'ag-header-center',
        headerClass: 'ag-header-center',
      },
      {
        field: 'isActiveText',
        headerName: this._localization.translate(
          'modules.admin.users.columns.status'
        ),
        width: 120,
        cellClass: 'ag-header-center',
        headerClass: 'ag-header-center',
      },
      {
        headerName: this._localization.translate('modules.admin.users.actions'),
        suppressHeaderMenuButton: true,
        sortable: false,
        headerClass: 'ag-header-center base-grid-header-cell',
        width: 160,
        minWidth: 160,
        maxWidth: 160,
        flex: 0,
        cellRenderer: MenuActionsCellRenderer,
        cellRendererParams: (params: ICellRendererParams<any>) => ({
          ...params,
          triggerLabel: this._localization.translate(
            'modules.admin.users.actions'
          ),
          actions: [
            {
              label: this._localization.translate(
                'modules.admin.users.viewResume'
              ),
              icon: 'heroicons_outline:document-text',
              iconClass: 'text-teal-600',
              hidden: (p: ICellRendererParams<UserDto>) =>
                !this.canViewResume(p.data),
              action: (p: ICellRendererParams<UserDto>) =>
                p.data
                  ? this.actionClicked.emit({ type: 'resume', row: p.data })
                  : undefined,
            },
            {
              label: this._localization.translate(
                'modules.admin.users.details'
              ),
              icon: 'heroicons_outline:eye',
              iconClass: 'text-blue-500',
              action: (p: ICellRendererParams<UserDto>) =>
                p.data
                  ? this.actionClicked.emit({ type: 'detail', row: p.data })
                  : undefined,
            },
            {
              label: this._localization.translate('modules.admin.users.delete'),
              icon: 'heroicons_outline:trash',
              iconClass: 'text-red-500',
              danger: true,
              hidden: (p: ICellRendererParams<UserDto>) =>
                !this.canDelete(p.data),
              action: (p: ICellRendererParams<UserDto>) =>
                p.data
                  ? this.actionClicked.emit({ type: 'delete', row: p.data })
                  : undefined,
            },
          ],
        }),
      },
    ];
  }

  private canViewResume(user?: UserDto): boolean {
    return !!user && user.userType === 'User';
  }

  private canDelete(user?: UserDto): boolean {
    if (!user) return false;
    return user.userType !== 'Administrator' && user.userType !== 'AdminLab';
  }
}

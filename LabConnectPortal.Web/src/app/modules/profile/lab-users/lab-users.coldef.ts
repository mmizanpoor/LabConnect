import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { LabMemberDto } from './lab-users.types';

export type LabUserAction =
  | { type: 'remove'; row: LabMemberDto }
  | { type: 'permissions'; row: LabMemberDto };

@Injectable({ providedIn: 'root' })
export class LabUsersColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<LabUserAction>();

  get(
    translateUserType: (userType: string) => string,
    formatJalaliDate: (value: string) => string,
    isBusy: (row: LabMemberDto) => boolean,
    canDelete = true,
    canManagePermissions = true,
  ): ColDef[] {
    return [
      {
        headerName: this._localization.translate('shared.mobile'),
        field: 'mobileNumber',
        width: 120,
        minWidth: 120,
        maxWidth: 120,
        headerClass: 'ag-header-center',
        cellClass: 'font-medium text-gray-900 ag-cell-center justify-center',
      },
      {
        headerName: this._localization.translate('shared.type'),
        width: 130,
        valueGetter: (p) => translateUserType(p.data?.userType ?? ''),
        headerClass: 'ag-header-center',
        cellClass: 'text-gray-900 ag-cell-center justify-center',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.labUsers.columns.mobileConfirmed'
        ),
        width: 120,
        minWidth: 120,
        maxWidth: 120,
        headerClass: 'ag-header-center',
        cellClass: 'text-gray-900 ag-cell-center justify-center',
        valueGetter: (p) =>
          p.data?.mobileConfirmed
            ? this._localization.translate('shared.yes')
            : this._localization.translate('shared.no'),
      },
      {
        headerName: this._localization.translate('shared.status'),
        headerClass: 'ag-header-center',
        cellClass: 'text-gray-900 ag-cell-center justify-center',
        valueGetter: (p) =>
          p.data?.isActive
            ? this._localization.translate('shared.active')
            : this._localization.translate('shared.inactive'),
        cellClassRules: {
          'text-emerald-700 font-semibold': (p) => !!p.data?.isActive,
          'text-slate-500 font-semibold': (p) => !p.data?.isActive,
        },
        minWidth: 180,
        flex: 1,
      },
      {
        headerName: this._localization.translate(
          'modules.profile.labUsers.columns.createdAt'
        ),
        minWidth: 120,
        maxWidth: 120,
        headerClass: 'ag-header-center',
        cellClass: 'text-gray-900 ag-cell-center justify-center',
        valueGetter: (p) => formatJalaliDate(p.data?.createdAt ?? ''),
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
        cellRendererParams: (p: ICellRendererParams<LabMemberDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate(
                'modules.profile.labUsers.permissions.manage'
              ),
              icon: 'heroicons_outline:key',
              iconClass: 'text-violet-600',
              hidden: (x: ICellRendererParams<LabMemberDto>) =>
                !canManagePermissions || !x.data || x.data.userType !== 'UserLab',
              action: (x: ICellRendererParams<LabMemberDto>) =>
                x.data
                  ? this.actionClicked.emit({ type: 'permissions', row: x.data })
                  : undefined,
            },
            {
              label: this._localization.translate('shared.delete'),
              icon: 'heroicons_outline:trash',
              iconClass: 'text-red-500',
              danger: true,
              hidden: (x: ICellRendererParams<LabMemberDto>) =>
                !canDelete || !x.data || this.isAdminLab(x.data) || isBusy(x.data),
              action: (x: ICellRendererParams<LabMemberDto>) =>
                x.data
                  ? this.actionClicked.emit({ type: 'remove', row: x.data })
                  : undefined,
            },
          ],
        }),
      },
    ];
  }

  private isAdminLab(member: LabMemberDto): boolean {
    return member.userType === 'AdminLab';
  }
}

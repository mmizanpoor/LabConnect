import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import {
  CompanyRegulationDto,
  CompanyRegulationType,
  normalizeCompanyRegulationType,
} from './company-regulations.types';

export type CompanyRegulationAction =
  | { type: 'edit'; row: CompanyRegulationDto }
  | { type: 'delete'; row: CompanyRegulationDto };

@Injectable({ providedIn: 'root' })
export class CompanyRegulationsColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<CompanyRegulationAction>();

  get(): ColDef[] {
    return [
      {
        field: 'type',
        headerName: this._localization.translate(
          'modules.admin.companyRegulations.columns.type',
        ),
        minWidth: 220,
        flex: 1,
        valueGetter: (p) => this.typeLabel(p.data?.type),
        cellClass: 'font-medium text-gray-900',
      },
      {
        field: 'createdAt',
        headerName: this._localization.translate(
          'modules.admin.companyRegulations.columns.createdAt',
        ),
        minWidth: 160,
        valueGetter: (p) => this.formatDate(p.data?.createdAt),
      },
      {
        field: 'modifiedAt',
        headerName: this._localization.translate(
          'modules.admin.companyRegulations.columns.modifiedAt',
        ),
        minWidth: 160,
        valueGetter: (p) => this.formatDate(p.data?.modifiedAt),
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
        cellRendererParams: (p: ICellRendererParams<CompanyRegulationDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate('shared.edit'),
              icon: 'heroicons_outline:pencil-square',
              iconClass: 'text-blue-500',
              action: (params: ICellRendererParams<CompanyRegulationDto>) =>
                params.data
                  ? this.actionClicked.emit({ type: 'edit', row: params.data })
                  : undefined,
            },
            {
              label: this._localization.translate('shared.delete'),
              icon: 'heroicons_outline:trash',
              iconClass: 'text-red-500',
              danger: true,
              action: (params: ICellRendererParams<CompanyRegulationDto>) =>
                params.data
                  ? this.actionClicked.emit({ type: 'delete', row: params.data })
                  : undefined,
            },
          ],
        }),
      },
    ];
  }

  private typeLabel(type?: CompanyRegulationDto['type']): string {
    switch (normalizeCompanyRegulationType(type)) {
      case CompanyRegulationType.RulesAndRegulations:
        return this._localization.translate(
          'modules.admin.companyRegulations.types.rulesAndRegulations',
        );
      case CompanyRegulationType.CompanyPolicy:
        return this._localization.translate(
          'modules.admin.companyRegulations.types.companyPolicy',
        );
      default:
        return '';
    }
  }

  private formatDate(value?: string): string {
    if (!value) return '';
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return value;
    return date.toLocaleDateString('fa-IR');
  }
}

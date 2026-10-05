import { Injectable } from '@angular/core';
import { FormControl } from '@angular/forms';
import { LocalizationService } from '@core/services/localization/localization.service';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { BehaviorSubject } from 'rxjs';
import {
  SpecialOfferBulkDiscountHeaderRenderer,
  SpecialOfferNumberCellRenderer,
  SpecialOfferSelectCellRenderer,
  SpecialOfferSelectHeaderRenderer,
  SpecialOfferSelectionState,
} from './special-offer-form-tests.renderer';
import { SpecialOfferTestRow } from './special-offers.types';

export type SpecialOfferFormColDefContext = {
  selectionState$: BehaviorSubject<SpecialOfferSelectionState>;
  bulkDiscountControl: FormControl<number | null>;
  onToggleAll: (checked: boolean) => void;
  onToggleRow: (row: SpecialOfferTestRow, checked: boolean) => void;
  onDiscountChange: (row: SpecialOfferTestRow, value: string | number | null) => void;
  onMaxSamplesChange: (row: SpecialOfferTestRow, value: string | number | null) => void;
  onBulkDiscountInput: (value: string) => void;
};

@Injectable({ providedIn: 'root' })
export class SpecialOfferFormColDef {
  constructor(private _localization: LocalizationService) {}

  get(ctx: SpecialOfferFormColDefContext): ColDef<SpecialOfferTestRow>[] {
    return [
      {
        colId: 'select',
        headerName: '',
        width: 56,
        minWidth: 56,
        maxWidth: 56,
        flex: 0,
        sortable: false,
        suppressHeaderMenuButton: true,
        suppressKeyboardEvent: () => true,
        headerClass: 'ag-header-center base-grid-header-cell',
        cellClass: 'base-grid-cell-center',
        headerComponent: SpecialOfferSelectHeaderRenderer,
        headerComponentParams: {
          selectionState$: ctx.selectionState$,
          onToggleAll: ctx.onToggleAll,
        },
        cellRenderer: SpecialOfferSelectCellRenderer,
        cellRendererParams: (p: ICellRendererParams<SpecialOfferTestRow>) => ({
          ...p,
          onToggle: ctx.onToggleRow,
        }),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.testInfos.columns.cpnCode'
        ),
        field: 'cpnCode',
        minWidth: 100,
        width: 110,
        valueGetter: (p) => p.data?.cpnCode || '—',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.testInfos.columns.nationalCode'
        ),
        field: 'nationalCode',
        minWidth: 110,
        width: 120,
        valueGetter: (p) => p.data?.nationalCode || '—',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.testInfos.columns.fullName'
        ),
        field: 'fullName',
        minWidth: 180,
        flex: 1,
        cellClass: 'font-medium text-slate-800',
        valueGetter: (p) => p.data?.fullName || '—',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.testInfos.columns.shortName'
        ),
        field: 'shortName',
        minWidth: 140,
        flex: 1,
        valueGetter: (p) => p.data?.shortName || '—',
      },
      {
        colId: 'discount',
        headerName: this._localization.translate(
          'modules.profile.specialOffers.columns.discount'
        ),
        field: 'discount',
        minWidth: 120,
        width: 130,
        sortable: false,
        suppressHeaderMenuButton: true,
        suppressKeyboardEvent: () => true,
        headerComponent: SpecialOfferBulkDiscountHeaderRenderer,
        headerComponentParams: {
          label: this._localization.translate(
            'modules.profile.specialOffers.columns.discount'
          ),
          placeholder: this._localization.translate(
            'modules.profile.specialOffers.bulkDiscount'
          ),
          bulkDiscountControl: ctx.bulkDiscountControl,
          onBulkDiscountInput: ctx.onBulkDiscountInput,
        },
        cellRenderer: SpecialOfferNumberCellRenderer,
        cellRendererParams: (p: ICellRendererParams<SpecialOfferTestRow>) => ({
          ...p,
          field: 'discount',
          min: 0,
          max: 100,
          inputClass: 'w-20',
          onChange: ctx.onDiscountChange,
        }),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.specialOffers.columns.maxSamples'
        ),
        field: 'maxSamples',
        minWidth: 130,
        width: 140,
        sortable: false,
        suppressHeaderMenuButton: true,
        suppressKeyboardEvent: () => true,
        cellRenderer: SpecialOfferNumberCellRenderer,
        cellRendererParams: (p: ICellRendererParams<SpecialOfferTestRow>) => ({
          ...p,
          field: 'maxSamples',
          min: 0,
          inputClass: 'w-24',
          onChange: ctx.onMaxSamplesChange,
        }),
      },
    ];
  }
}

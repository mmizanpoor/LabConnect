import { NgClass } from '@angular/common';
import { Component, OnDestroy } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ICellRendererAngularComp, IHeaderAngularComp } from 'ag-grid-angular';
import { ICellRendererParams, IHeaderParams } from 'ag-grid-community';
import { BehaviorSubject, Subscription } from 'rxjs';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { SpecialOfferTestRow } from './special-offers.types';

export type SpecialOfferSelectionState = {
  allSelected: boolean;
  someSelected: boolean;
};

export type SpecialOfferSelectCellParams = ICellRendererParams<SpecialOfferTestRow> & {
  onToggle: (row: SpecialOfferTestRow, checked: boolean) => void;
};

@Component({
  selector: 'special-offer-select-cell-renderer',
  standalone: true,
  imports: [BaseCheckboxComponent],
  template: `
    <div class="flex h-full w-full items-center justify-center" (click)="stop($event)">
      <base-checkbox [checked]="checked" (checkedChange)="onChange($event)" />
    </div>
  `,
  styles: `
    :host {
      display: flex;
      width: 100%;
      height: 100%;
    }
  `,
})
export class SpecialOfferSelectCellRenderer implements ICellRendererAngularComp {
  params!: SpecialOfferSelectCellParams;
  checked = false;

  agInit(params: SpecialOfferSelectCellParams): void {
    this.params = params;
    this.checked = !!params.data?.selected;
  }

  refresh(params: SpecialOfferSelectCellParams): boolean {
    this.params = params;
    this.checked = !!params.data?.selected;
    return true;
  }

  onChange(checked: boolean): void {
    const row = this.params.data;
    if (!row) return;
    this.checked = checked;
    this.params.onToggle(row, checked);
  }

  stop(event: Event): void {
    event.stopPropagation();
  }
}

export type SpecialOfferSelectHeaderParams = IHeaderParams & {
  selectionState$: BehaviorSubject<SpecialOfferSelectionState>;
  onToggleAll: (checked: boolean) => void;
};

@Component({
  selector: 'special-offer-select-header-renderer',
  standalone: true,
  imports: [BaseCheckboxComponent],
  template: `
    <div class="flex h-full w-full items-center justify-center" (click)="stop($event)">
      <base-checkbox
        [checked]="allSelected"
        [indeterminate]="someSelected"
        (checkedChange)="onChange($event)"
      />
    </div>
  `,
  styles: `
    :host {
      display: flex;
      width: 100%;
      height: 100%;
    }
  `,
})
export class SpecialOfferSelectHeaderRenderer implements IHeaderAngularComp, OnDestroy {
  params!: SpecialOfferSelectHeaderParams;
  allSelected = false;
  someSelected = false;
  private _sub?: Subscription;

  agInit(params: SpecialOfferSelectHeaderParams): void {
    this.bind(params);
  }

  refresh(params: SpecialOfferSelectHeaderParams): boolean {
    this.bind(params);
    return true;
  }

  ngOnDestroy(): void {
    this._sub?.unsubscribe();
  }

  onChange(checked: boolean): void {
    this.params.onToggleAll(checked);
  }

  stop(event: Event): void {
    event.stopPropagation();
  }

  private bind(params: SpecialOfferSelectHeaderParams): void {
    this.params = params;
    this._sub?.unsubscribe();
    this._sub = params.selectionState$.subscribe((state) => {
      this.allSelected = state.allSelected;
      this.someSelected = state.someSelected;
    });
  }
}

export type SpecialOfferNumberCellParams = ICellRendererParams<SpecialOfferTestRow> & {
  field: 'discount' | 'maxSamples';
  min?: number;
  max?: number;
  inputClass?: string;
  onChange: (row: SpecialOfferTestRow, value: string | number | null) => void;
};

@Component({
  selector: 'special-offer-number-cell-renderer',
  standalone: true,
  imports: [NgClass],
  template: `
    <input
      type="number"
      class="rounded border border-slate-200 px-2 py-1 text-sm"
      [ngClass]="inputClass"
      [attr.min]="min"
      [attr.max]="max == null ? null : max"
      [value]="displayValue"
      [disabled]="disabled"
      (click)="stop($event)"
      (input)="onInput($any($event.target).value)"
    />
  `,
  styles: `
    :host {
      display: flex;
      align-items: center;
      width: 100%;
      height: 100%;
    }
  `,
})
export class SpecialOfferNumberCellRenderer implements ICellRendererAngularComp {
  params!: SpecialOfferNumberCellParams;
  min: number | null = 0;
  max: number | null = null;
  inputClass = '';
  displayValue: number | '' = '';
  disabled = true;

  agInit(params: SpecialOfferNumberCellParams): void {
    this.apply(params);
  }

  refresh(params: SpecialOfferNumberCellParams): boolean {
    this.apply(params);
    return true;
  }

  onInput(value: string): void {
    const row = this.params.data;
    if (!row) return;
    this.params.onChange(row, value);
  }

  stop(event: Event): void {
    event.stopPropagation();
  }

  private apply(params: SpecialOfferNumberCellParams): void {
    this.params = params;
    this.min = params.min ?? 0;
    this.max = params.max ?? null;
    this.inputClass = params.inputClass ?? '';
    this.disabled = !params.data?.selected;
    const raw = params.data?.[params.field];
    this.displayValue = raw == null ? '' : raw;
  }
}

export type SpecialOfferBulkDiscountHeaderParams = IHeaderParams & {
  label: string;
  placeholder: string;
  bulkDiscountControl: FormControl<number | null>;
  onBulkDiscountInput: (value: string) => void;
};

@Component({
  selector: 'special-offer-bulk-discount-header-renderer',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div class="flex h-full w-full flex-col items-end justify-center gap-1 py-1">
      <span class="text-sm font-bold text-slate-800">{{ label }}</span>
      <input
        type="number"
        min="0"
        max="100"
        class="w-20 rounded border border-slate-200 px-2 py-1 text-xs"
        [formControl]="control"
        [placeholder]="placeholder"
        (click)="stop($event)"
        (input)="onInput($any($event.target).value)"
      />
    </div>
  `,
  styles: `
    :host {
      display: flex;
      width: 100%;
      height: 100%;
    }
  `,
})
export class SpecialOfferBulkDiscountHeaderRenderer implements IHeaderAngularComp {
  params!: SpecialOfferBulkDiscountHeaderParams;
  label = '';
  placeholder = '';
  control = new FormControl<number | null>(null);

  agInit(params: SpecialOfferBulkDiscountHeaderParams): void {
    this.apply(params);
  }

  refresh(params: SpecialOfferBulkDiscountHeaderParams): boolean {
    this.apply(params);
    return true;
  }

  onInput(value: string): void {
    this.params.onBulkDiscountInput(value);
  }

  stop(event: Event): void {
    event.stopPropagation();
  }

  private apply(params: SpecialOfferBulkDiscountHeaderParams): void {
    this.params = params;
    this.label = params.label;
    this.placeholder = params.placeholder;
    this.control = params.bulkDiscountControl;
  }
}

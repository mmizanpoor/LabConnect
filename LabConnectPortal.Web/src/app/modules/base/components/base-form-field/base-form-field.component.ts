import {
  booleanAttribute,
  Component,
  DestroyRef,
  ElementRef,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  ViewChild,
  inject,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE } from '@angular/material/core';
import { MatDatepicker, MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { TranslocoPipe } from '@jsverse/transloco';
import { JalaliDateAdapter } from '@core/datetime/jalali-date.adapter';
import { JALALI_DATE_FORMATS } from '@core/datetime/jalali-date-formats';
import { debounceTime, distinctUntilChanged } from 'rxjs';

export interface BaseFormSelectOption {
  value: string | number | boolean | null;
  label: string;
}

@Component({
  selector: 'base-form-field',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatSelectModule,
    MatDatepickerModule,
    TranslocoPipe,
  ],
  providers: [
    { provide: DateAdapter, useClass: JalaliDateAdapter },
    { provide: MAT_DATE_FORMATS, useValue: JALALI_DATE_FORMATS },
    { provide: MAT_DATE_LOCALE, useValue: 'fa' },
  ],
  templateUrl: './base-form-field.component.html',
  styleUrl: './base-form-field.component.scss',
})
export class BaseFormFieldComponent implements OnChanges {
  private _host = inject(ElementRef<HTMLElement>);
  private _destroyRef = inject(DestroyRef);

  @ViewChild('floatJalaliPickerRef') floatJalaliPickerRef?: MatDatepicker<any>;

  @Input({ required: true }) control!: FormControl;
  @Input({ required: true }) label!: string;
  @Input() placeholder = '';
  @Input() type = 'text';
  @Input() jalaliDate = false;
  @Input() icon = '';
  /** Material outline field with floating label (notched border). */
  @Input({ transform: booleanAttribute }) floatLabel = false;
  @Input({ transform: booleanAttribute }) readOnly = false;
  @Input() layout: 'floating' | 'stacked' = 'stacked';
  @Input() required = false;
  @Input() multiline = false;
  @Input() select = false;
  @Input() multiple = false;
  /** Show a search box inside the select panel and filter options locally. */
  @Input({ transform: booleanAttribute }) searchable = false;
  /** When true, search only emits for the parent (no local option filtering). */
  @Input({ transform: booleanAttribute }) remoteSearch = false;
  @Input() searchPlaceholder = '';
  @Input() options: BaseFormSelectOption[] = [];
  @Input() rows = 3;
  @Input() maxLength?: number;
  @Input() errorMessage = 'message.validation.required';
  @Input() maxLengthErrorMessage = 'message.validation.maxLength';

  /** Emits the select search term (debounced). Parent may reload remote options. */
  @Output() searchChange = new EventEmitter<string>();

  readonly selectSearchControl = new FormControl('', { nonNullable: true });
  filteredOptions: BaseFormSelectOption[] = [];

  constructor() {
    this.selectSearchControl.valueChanges
      .pipe(debounceTime(200), distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe((term) => {
        this.applySelectFilter(term);
        this.searchChange.emit(term.trim());
      });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['options'] || changes['searchable'] || changes['remoteSearch']) {
      this.applySelectFilter(this.selectSearchControl.value);
    }
  }

  get useFloatLabel(): boolean {
    return this.floatLabel || this.layout === 'floating';
  }

  get selectedValues(): Array<string | number | boolean | null> {
    const value = this.control?.value;
    if (!this.multiple) return [];
    return Array.isArray(value) ? value : [];
  }

  get selectedCount(): number {
    return this.selectedValues.length;
  }

  get selectedSingleLabel(): string {
    const first = this.selectedValues[0];
    return this.options.find((option) => option.value === first)?.label ?? '';
  }

  openFloatJalaliPicker(): void {
    this.floatJalaliPickerRef?.open();
  }

  onSelectOpened(isOpen: boolean): void {
    if (isOpen && this.searchable) {
      this.selectSearchControl.setValue('', { emitEvent: false });
      this.applySelectFilter('');
    }

    if (!isOpen || !this.select) return;

    const syncPanelPosition = (): void => {
      const combo =
        (this._host.nativeElement.querySelector('.base-form-field__control') as HTMLElement | null) ||
        (this._host.nativeElement.querySelector(
          '.base-form-field-float .mat-mdc-text-field-wrapper',
        ) as HTMLElement | null) ||
        (this._host.nativeElement.querySelector(
          '.base-form-field__select .mat-mdc-select-trigger',
        ) as HTMLElement | null) ||
        (this._host.nativeElement.querySelector(
          '.base-form-field-float .mat-mdc-select-trigger',
        ) as HTMLElement | null) ||
        (this._host.nativeElement.querySelector('.mat-mdc-select-trigger') as HTMLElement | null);

      const panels = document.querySelectorAll(
        '.cdk-overlay-container .mat-mdc-select-panel.base-form-field-select-panel',
      );
      const panel = panels.item(panels.length - 1) as HTMLElement | null;
      const pane = panel?.closest('.cdk-overlay-pane') as HTMLElement | null;

      if (!combo || !pane || !panel) return;

      const rect = combo.getBoundingClientRect();
      const gap = 5;
      const width = Math.max(0, Math.round(rect.width));
      const top = Math.round(rect.bottom + gap);
      const left = Math.round(rect.left);

      pane.classList.add('base-form-field-select-overlay-pane');
      pane.style.position = 'fixed';
      pane.style.top = `${top}px`;
      pane.style.left = `${left}px`;
      pane.style.right = 'auto';
      pane.style.bottom = 'auto';
      pane.style.width = `${width}px`;
      pane.style.minWidth = `${width}px`;
      pane.style.maxWidth = `${width}px`;
      pane.style.transform = 'none';
      pane.style.margin = '0';
      pane.style.padding = '0';
      pane.style.overflow = 'hidden';
      pane.style.boxSizing = 'border-box';
      pane.style.zIndex = '1300';

      panel.style.width = '100%';
      panel.style.minWidth = '100%';
      panel.style.maxWidth = '100%';
      panel.style.boxSizing = 'border-box';
      panel.style.margin = '0';
      panel.style.overflowX = 'hidden';
    };

    requestAnimationFrame(() => {
      syncPanelPosition();
      requestAnimationFrame(() => {
        syncPanelPosition();
        // CDK may finalize flexible connected position one tick later.
        setTimeout(syncPanelPosition, 0);
      });
    });
  }

  stopSelectSearchEvent(event: Event): void {
    event.stopPropagation();
  }

  private applySelectFilter(term: string): void {
    if (!this.searchable || this.remoteSearch) {
      this.filteredOptions = this.options;
      return;
    }

    const q = term.trim().toLowerCase();
    if (!q) {
      this.filteredOptions = this.options;
      return;
    }

    this.filteredOptions = this.options.filter((option) =>
      option.label.toLowerCase().includes(q),
    );
  }
}

import {
  Component,
  EventEmitter,
  HostBinding,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  Output,
  SimpleChanges,
  booleanAttribute,
  inject,
} from '@angular/core';
import { AgGridAngular } from 'ag-grid-angular';
import {
  ColDef,
  GridApi,
  GridOptions,
  GridReadyEvent,
  ModuleRegistry,
  RowClickedEvent,
  AllCommunityModule,
} from 'ag-grid-community';
import { BehaviorSubject, Subscription } from 'rxjs';
import { LocalizationService } from '@core/services/localization/localization.service';

ModuleRegistry.registerModules([AllCommunityModule]);

@Component({
  selector: 'base-grid',
  standalone: true,
  imports: [AgGridAngular],
  templateUrl: './base-grid.component.html',
  styleUrl: './base-grid.component.scss',
})
export class BaseGridComponent implements OnInit, OnChanges, OnDestroy {
  private _localization = inject(LocalizationService);

  @Input({ required: true }) columnDefs!: ColDef[];
  @Input({ required: true }) gridId!: string;
  @Input() dataSource$: BehaviorSubject<any[]> | null | undefined =
    new BehaviorSubject<any[]>([]);
  @Input({ transform: booleanAttribute }) visibleOrder = true;
  @Input({ transform: booleanAttribute }) disabled = false;
  @Input({ transform: booleanAttribute }) toolbarDisabled = false;
  @Input({ transform: booleanAttribute }) autoHeight = false;
  @Input() headerHeight?: number;
  /** When true, wraps the grid in a bordered panel with cool gray background. */
  @Input({ transform: booleanAttribute }) framed = true;

  @HostBinding('class.base-grid--framed')
  get isFramed(): boolean {
    return this.framed;
  }
  /** Optional override for the empty-grid overlay text. */
  @Input() noRowsMessage = '';
  /** Shows a simple row-count footer at the bottom-left of the grid. */
  @Input({ transform: booleanAttribute }) showRowCount = false;
  /**
   * i18n key for the count label, e.g. `modules.profile.receptions.resultCount`
   * (`{{count}} پذیرش`). Defaults to `shared.rowCount`.
   */
  @Input() rowCountKey = 'shared.rowCount';
  @Input() set forceDirection(value: 'rtl' | 'ltr') {
    this.gridOptions.enableRtl = value === 'rtl';
  }

  get rowCountText(): string {
    return this._localization.translate(this.rowCountKey || 'shared.rowCount', {
      count: this.rowData?.length ?? 0,
    });
  }

  @Output() rowClicked = new EventEmitter<RowClickedEvent>();

  rowData: any[] = [];
  gridOptions: GridOptions = {
    // We're using legacy CSS themes (ag-theme-alpine + ag-grid.css). New theming API would conflict.
    theme: 'legacy',
    enableRtl: true,
    suppressCellFocus: true,
    rowSelection: 'single',
    animateRows: true,
    // 'autoHeight' renders all rows into DOM; can freeze lists. Use 'normal' by default.
    domLayout: 'normal',
    headerHeight: 38,
    rowHeight: 44,
    suppressMovableColumns: true,
    overlayNoRowsTemplate: '',
    localeText: {
      noRowsToShow: '',
    },
  };

  defaultColDef: ColDef = {
    sortable: true,
    resizable: true,
    minWidth: 120,
    suppressHeaderMenuButton: true,
    headerClass: 'base-grid-header-cell',
    cellClass: 'base-grid-cell',
  };

  private _gridApi?: GridApi;
  private _dataSubscription?: Subscription;

  ngOnInit(): void {
    if (this.autoHeight) {
      this.gridOptions.domLayout = 'autoHeight';
    }
    this.applyHeaderHeight();
    this.applyNoRowsOverlayTemplate();
    this.bindToDataSource();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['dataSource$']) {
      this.bindToDataSource();
    }
    if (changes['noRowsMessage']) {
      this.applyNoRowsOverlayTemplate();
    }
    if (changes['headerHeight']) {
      this.applyHeaderHeight();
    }
    if (changes['disabled'] && this._gridApi) {
      this._gridApi.setGridOption('loading', this.disabled);
      this.refreshNoRowsOverlay();
    }
  }

  ngOnDestroy(): void {
    this._dataSubscription?.unsubscribe();
  }

  onGridReady(event: GridReadyEvent): void {
    this._gridApi = event.api;
    this.applyHeaderHeight();
    this.applyNoRowsOverlayTemplate();
    this._gridApi.setGridOption('loading', this.disabled);
    this._gridApi.refreshHeader();
    this._gridApi.setGridOption('rowData', this.rowData);
    this.refreshNoRowsOverlay();
  }

  onRowClicked(event: RowClickedEvent): void {
    this.rowClicked.emit(event);
  }

  private applyHeaderHeight(): void {
    if (this.headerHeight == null) {
      return;
    }

    this.gridOptions.headerHeight = this.headerHeight;
    this._gridApi?.setGridOption('headerHeight', this.headerHeight);
  }

  private bindToDataSource(): void {
    this._dataSubscription?.unsubscribe();

    // Ensure initial value renders even if grid initializes later
    this.rowData = this.dataSource$?.value ?? [];
    this._gridApi?.setGridOption('rowData', this.rowData);
    this.refreshNoRowsOverlay();

    this._dataSubscription = this.dataSource$?.subscribe((rows) => {
      this.rowData = rows ?? [];
      this._gridApi?.setGridOption('rowData', this.rowData);
      this.refreshNoRowsOverlay();
    });
  }

  private applyNoRowsOverlayTemplate(): void {
    const message =
      this.noRowsMessage?.trim() ||
      this._localization.translate('shared.noRows');
    const template = `
      <div class="base-grid-empty" dir="rtl">
        <div class="base-grid-empty__icon" aria-hidden="true">
          <svg viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 22h40v26a6 6 0 0 1-6 6H18a6 6 0 0 1-6-6V22z" fill="#eef3f9" stroke="#527baa" stroke-width="2"/>
            <path d="M12 22l6-10h28l6 10" stroke="#527baa" stroke-width="2" stroke-linejoin="round" fill="#dce6f2"/>
            <path d="M12 22h40" stroke="#527baa" stroke-width="2"/>
            <circle cx="32" cy="38" r="7" stroke="#527baa" stroke-width="2" fill="#fff"/>
            <path d="M32 35v6M29 38h6" stroke="#527baa" stroke-width="2" stroke-linecap="round" opacity="0.35"/>
            <path d="M37 43l5 5" stroke="#527baa" stroke-width="2.5" stroke-linecap="round"/>
          </svg>
        </div>
        <span class="base-grid-empty__text">${this.escapeHtml(message)}</span>
      </div>
    `;
    this.gridOptions.overlayNoRowsTemplate = template;
    this._gridApi?.setGridOption('overlayNoRowsTemplate', template);
  }

  private refreshNoRowsOverlay(): void {
    if (!this._gridApi || this.disabled) return;

    if (!this.rowData.length) {
      this._gridApi.showNoRowsOverlay();
    } else {
      this._gridApi.hideOverlay();
    }
  }

  private escapeHtml(value: string): string {
    return value
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }
}

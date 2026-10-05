import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { TestInfosService } from './test-infos.service';
import { TestInfoListItemDto } from './test-infos.types';
import { TestInfoDetailDialogComponent } from './test-info-detail-dialog/test-info-detail-dialog.component';
import { TestInfoEditPriceDialogComponent } from './test-info-edit-price-dialog/test-info-edit-price-dialog.component';
import { TestInfosColDef, TestInfoAction } from './test-infos.coldef';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';

@Component({
  selector: 'app-test-infos-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatSidenavModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BaseGridComponent,
  ],
  templateUrl: './test-infos-list.component.html',
  styleUrl: './test-infos-list.component.scss',
})
export class TestInfosListComponent implements OnInit, AfterViewInit {
  readonly entity = SystemEntity.TestInfo;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.TestInfo);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;

  loading = false;
  error = '';
  allRows: TestInfoListItemDto[] = [];
  filteredRows: TestInfoListItemDto[] = [];
  searchControl = new FormControl('', { nonNullable: true });
  appliedSearch = '';
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<TestInfoListItemDto[]>([]);

  constructor(
    private _service: TestInfosService,
    private _dialog: MatDialog,
    private _localization: LocalizationService,
    private _colDef: TestInfosColDef,
    private _labPermission: LabPermissionService,
  ) {}

  get canUpdate(): boolean {
    return this._labPermission.can(this.entity, 'update');
  }

  ngOnInit(): void {
    this.colDef = this._colDef.get((v) => this.formatPrice(v), this.canUpdate);
    this._colDef.actionClicked.subscribe((evt: TestInfoAction) => {
      if (!evt?.row) return;
      if (evt.type === 'detail') this.openDetailDialog(evt.row);
      if (evt.type === 'editPrice') this.openEditPriceDialog(evt.row);
    });
    void this.load();
  }

  ngAfterViewInit(): void {
    queueMicrotask(() => this.filterDrawer?.open());
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const result = await this._service.getAll();
      if (!result.success || !result.data) {
        throw new Error(
          result.message ?? this._localization.translate('modules.profile.testInfos.errors.loadFailed'),
        );
      }
      this.allRows = result.data.map((row) => this.normalizeRow(row));
      this.applyFilter(this.appliedSearch);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.testInfos.errors.loadFailed');
      this.allRows = [];
      this.filteredRows = [];
      this.list$.next([]);
    } finally {
      this.loading = false;
    }
  }

  applyFilters(): void {
    this.appliedSearch = this.searchControl.value;
    this.applyFilter(this.appliedSearch);
    void this.filterDrawer?.close();
  }

  formatPrice(value: number | null | undefined): string {
    if (value === null || value === undefined) return '—';
    return `${value.toLocaleString('fa-IR')} ${this._localization.translate('shared.currency')}`;
  }

  openDetailDialog(row: TestInfoListItemDto): void {
    this._dialog.open(TestInfoDetailDialogComponent, {
      width: 'min(95vw, 72rem)',
      maxWidth: '95vw',
      data: { id: row.id },
    });
  }

  openEditPriceDialog(row: TestInfoListItemDto): void {
    const ref = this._dialog.open(TestInfoEditPriceDialogComponent, {
      width: 'min(95vw, 36rem)',
      maxWidth: '95vw',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: { row },
    });
    ref.afterClosed().subscribe((saved) => {
      if (saved) void this.load();
    });
  }

  private applyFilter(searchValue: string): void {
    const term = this.normalizeText(searchValue);
    if (!term) {
      this.filteredRows = [...this.allRows];
      this.list$.next(this.filteredRows);
      return;
    }

    this.filteredRows = this.allRows.filter((row) => this.matchesSearch(row, term));
    this.list$.next(this.filteredRows);
  }

  private matchesSearch(row: TestInfoListItemDto, term: string): boolean {
    const fields = this.searchFields(row);

    if (fields.some((field) => field.includes(term))) {
      return true;
    }

    const words = term.split(' ').filter(Boolean);
    if (words.length <= 1) {
      return false;
    }

    return words.every((word) => fields.some((field) => field.includes(word)));
  }

  private searchFields(row: TestInfoListItemDto): string[] {
    return [row.fullName, row.shortName, row.sectionName, row.cpnCode, row.nationalCode].map((value) =>
      this.normalizeText(value),
    );
  }

  private normalizeText(value: string | null | undefined): string {
    return (value ?? '')
      .trim()
      .toLowerCase()
      .normalize('NFKC')
      .replace(/\u200c/g, '')
      .replace(/\s+/g, ' ');
  }

  private normalizeRow(row: TestInfoListItemDto): TestInfoListItemDto {
    const raw = row as TestInfoListItemDto & {
      ShortName?: string | null;
      shortname?: string | null;
    };

    return {
      ...row,
      shortName: row.shortName ?? raw.ShortName ?? raw.shortname ?? null,
    };
  }
}

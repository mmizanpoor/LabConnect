import { Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { Router } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { BasePagingComponent } from '@modules/base/components/base-paging/base-paging.component';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseConfirmDialogComponent } from '@modules/base/components/base-dialog/base-confirm-dialog.component';
import { ShopsColDef } from './shops.coldef';
import { ShopsService } from './shops.service';
import {
  CenterProfileListItemDto,
  ProfileCompletionFilter,
} from './shops.types';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import { ColDef, RowClickedEvent } from 'ag-grid-community';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';

interface ShopRow extends CenterProfileListItemDto {
  completionText: string;
  approvalText: string;
}

@Component({
  selector: 'app-shops',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatIconModule,
    MatSidenavModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BasePagingComponent,
    BaseGridComponent,
  ],
  templateUrl: './shops.component.html',
  styles: `
    :host {
      display: block;
      height: 100%;
      min-height: 0;
    }

    :host ::ng-deep .mat-drawer-container {
      height: 100%;
    }
  `,
})
export class ShopsComponent implements OnInit {
  readonly entity = SystemEntity.Shop;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.Shop);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<ShopRow[]>([]);
  rows: ShopRow[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;
  loading = false;

  nameControl = new FormControl('');
  mobileControl = new FormControl('');
  completionControl = new FormControl<ProfileCompletionFilter>('All');

  constructor(
    private _shopsService: ShopsService,
    private _router: Router,
    private _localization: LocalizationService,
    private _colDef: ShopsColDef,
    private _dialog: MatDialog,
  ) {
    this._colDef.actionClicked.pipe(takeUntilDestroyed()).subscribe((evt) => {
      if (evt?.type === 'detail' && evt.row) {
        this.openDetail(evt.row);
      }
      if (evt?.type === 'enableApiKey' && evt.row) {
        void this.enableApiKey(evt.row);
      }
    });
  }

  ngOnInit(): void {
    this.colDef = this._colDef.get();
    this.load();
  }

  get completionFilterOptions(): BaseFormSelectOption[] {
    return [
      { value: 'All', label: this._localization.translate('shared.all') },
      {
        value: 'Complete',
        label: this._localization.translate('modules.admin.shops.complete'),
      },
      {
        value: 'Incomplete',
        label: this._localization.translate('modules.admin.shops.incomplete'),
      },
    ];
  }

  applyFilters(): void {
    this.page = 1;
    void this.filterDrawer?.close();
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    const result = await this._shopsService.getShops({
      name: this.nameControl.value || undefined,
      mobile: this.mobileControl.value || undefined,
      completionStatus: this.completionControl.value || 'All',
      page: this.page,
      pageSize: this.pageSize,
    });

    if (result.success && result.data) {
      this.rows = result.data.items.map((item) => ({
        ...item,
        completionText: item.isComplete
          ? this._localization.translate('modules.admin.shops.complete')
          : this._localization.translate('modules.admin.shops.incomplete'),
        approvalText: item.isApproved
          ? this._localization.translate('modules.admin.shops.approved')
          : this._localization.translate('modules.admin.shops.notApproved'),
      }));
      this.totalCount = result.data.totalCount;
      this.list$.next(this.rows);
    } else {
      this.rows = [];
      this.list$.next([]);
    }
    this.loading = false;
  }

  onPageChange(page: number): void {
    this.page = page;
    this.load();
  }

  openDetail(row: CenterProfileListItemDto): void {
    void this._router.navigate(['/admin/shops', row.id]);
  }

  private async enableApiKey(row: CenterProfileListItemDto): Promise<void> {
    const ref = this._dialog.open(BaseConfirmDialogComponent, {
      width: '440px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: {
        title: this._localization.translate('shared.enableApiKey'),
        message: this._localization.translate('shared.confirmEnableApiKey'),
        confirmLabel: this._localization.translate('shared.confirm'),
        warnConfirm: false,
        premium: true,
        icon: 'key',
      },
    });
    if (!(await firstValueFrom(ref.afterClosed()))) return;

    try {
      const result = await this._shopsService.enableApiKey(row.id);
      if (!result.success) {
        alert(
          result.message ??
            this._localization.translate('shared.enableApiKeyFailed')
        );
        return;
      }

      await this.load();
    } catch {
      alert(this._localization.translate('shared.enableApiKeyFailed'));
    }
  }
}

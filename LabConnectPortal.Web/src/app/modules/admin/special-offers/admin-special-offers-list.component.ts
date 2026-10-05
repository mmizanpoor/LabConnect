import { Component, DestroyRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { AdminSpecialOffersService } from './admin-special-offers.service';
import { AdminSpecialOfferListItem } from './admin-special-offers.types';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AdminSpecialOffersColDef } from './admin-special-offers.coldef';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';

type SpecialOfferStatusFilter = 'all' | 'active' | 'inactive' | 'expired';

@Component({
  selector: 'app-admin-special-offers-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatIconModule,
    MatSidenavModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BaseGridComponent,
  ],
  templateUrl: './admin-special-offers-list.component.html',
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
export class AdminSpecialOffersListComponent implements OnInit {
  readonly entity = SystemEntity.SpecialOffer;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.SpecialOffer);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;
  private readonly _destroyRef = inject(DestroyRef);
  loading = false;
  error = '';
  rows: AdminSpecialOfferListItem[] = [];
  private _allRows: AdminSpecialOfferListItem[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<AdminSpecialOfferListItem[]>([]);

  titleFilter = new FormControl('');
  labNameFilter = new FormControl('');
  statusFilter = new FormControl<SpecialOfferStatusFilter>('all');

  constructor(
    private _service: AdminSpecialOffersService,
    private _router: Router,
    private _localization: LocalizationService,
    private _colDef: AdminSpecialOffersColDef,
  ) {}

  get statusFilterOptions(): BaseFormSelectOption[] {
    return [
      { value: 'all', label: this._localization.translate('shared.all') },
      {
        value: 'active',
        label: this._localization.translate('modules.profile.specialOffers.status.active'),
      },
      {
        value: 'inactive',
        label: this._localization.translate('modules.profile.specialOffers.status.inactive'),
      },
      {
        value: 'expired',
        label: this._localization.translate('modules.profile.specialOffers.status.expired'),
      },
    ];
  }

  ngOnInit(): void {
    this.colDef = this._colDef.get(
      (v) => this.formatDate(v),
      (r) => this.statusLabel(r),
    );
    this._colDef.actionClicked
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe((evt) => {
        if (evt?.row) this.openDetail(evt.row);
      });
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const result = await this._service.getAll();
      if (!result.success || !result.data) {
        throw new Error(
          result.message ?? this._localization.translate('modules.admin.specialOffers.errors.loadFailed'),
        );
      }
      this._allRows = result.data;
      this.applyClientFilters();
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.specialOffers.errors.loadFailed');
      this._allRows = [];
      this.rows = [];
      this.list$.next([]);
    } finally {
      this.loading = false;
    }
  }

  applyFilters(): void {
    void this.filterDrawer?.close();
    this.applyClientFilters();
  }

  private applyClientFilters(): void {
    const title = this.titleFilter.value?.trim().toLowerCase() ?? '';
    const labName = this.labNameFilter.value?.trim().toLowerCase() ?? '';
    const status = this.statusFilter.value ?? 'all';

    this.rows = this._allRows.filter((row) => {
      if (title && !row.title.toLowerCase().includes(title)) return false;
      if (labName && !row.labName.toLowerCase().includes(labName)) return false;
      if (status === 'active' && (!row.isActive || row.isExpired)) return false;
      if (status === 'inactive' && (row.isActive || row.isExpired)) return false;
      if (status === 'expired' && !row.isExpired) return false;
      return true;
    });
    this.list$.next(this.rows);
  }

  formatDate(value: string): string {
    const parsed = jMoment(value).locale('fa');
    return parsed.isValid() ? parsed.format('jYYYY/jMM/jDD') : '—';
  }

  statusLabel(row: AdminSpecialOfferListItem): string {
    if (row.isExpired) {
      return this._localization.translate('modules.profile.specialOffers.status.expired');
    }
    if (row.isActive) {
      return this._localization.translate('modules.profile.specialOffers.status.active');
    }
    return this._localization.translate('modules.profile.specialOffers.status.inactive');
  }

  openDetail(row: AdminSpecialOfferListItem): void {
    void this._router.navigate(['/admin/special-offers', row.id]);
  }
}

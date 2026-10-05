import { AfterViewInit, Component, DestroyRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment from 'moment-jalaali';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LocalizationService } from '@core/services/localization/localization.service';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { SpecialOffersService } from './special-offers.service';
import { SpecialOfferListItemDto } from './special-offers.types';
import {
  SpecialOfferRequestsDialogCloseResult,
  SpecialOfferRequestsDialogComponent,
} from './special-offer-requests-dialog/special-offer-requests-dialog.component';
import { SpecialOfferListAction, SpecialOffersListColDef } from './special-offers-list.coldef';

type SpecialOfferStatusFilter = '' | 'active' | 'inactive' | 'expired';

@Component({
  selector: 'app-special-offers-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatIconModule,
    MatDialogModule,
    MatSidenavModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BaseGridComponent,
  ],
  templateUrl: './special-offers-list.component.html',
  styleUrl: './special-offers-list.component.scss',
})
export class SpecialOffersListComponent implements OnInit, AfterViewInit {
  readonly entity = SystemEntity.SpecialOffer;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.SpecialOffer);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;

  private readonly _destroyRef = inject(DestroyRef);
  loading = false;
  error = '';
  private _allRows: SpecialOfferListItemDto[] = [];
  rows: SpecialOfferListItemDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<SpecialOfferListItemDto[]>([]);

  titleFilter = new FormControl('', { nonNullable: true });
  statusFilter = new FormControl<SpecialOfferStatusFilter>('', { nonNullable: true });

  constructor(
    private _service: SpecialOffersService,
    private _dialog: MatDialog,
    private _router: Router,
    private _localization: LocalizationService,
    private _colDef: SpecialOffersListColDef,
    private _labPermission: LabPermissionService,
  ) {}

  get canCreate(): boolean {
    return this._labPermission.can(this.entity, 'create');
  }

  get canUpdate(): boolean {
    return this._labPermission.can(this.entity, 'update');
  }

  get canDelete(): boolean {
    return this._labPermission.can(this.entity, 'delete');
  }

  get statusFilterOptions(): BaseFormSelectOption[] {
    return [
      { value: '', label: this._localization.translate('shared.all') },
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
      this.canUpdate,
      this.canDelete,
    );

    this._colDef.actionClicked
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe((evt: SpecialOfferListAction) => {
        if (!evt?.row) return;
        if (evt.type === 'edit') this.editRow(evt.row);
        if (evt.type === 'requests') this.openRequests(evt.row);
        if (evt.type === 'delete') void this.deleteRow(evt.row);
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
          result.message ??
            this._localization.translate('modules.profile.specialOffers.errors.loadFailed'),
        );
      }
      this._allRows = result.data;
      this.applyClientFilters();
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.specialOffers.errors.loadFailed');
      this._allRows = [];
      this.rows = [];
      this.list$.next([]);
    } finally {
      this.loading = false;
    }
  }

  applyFilters(): void {
    this.applyClientFilters();
    void this.filterDrawer?.close();
  }

  private applyClientFilters(): void {
    const title = this.titleFilter.value.trim().toLowerCase();
    const status = this.statusFilter.value;

    this.rows = this._allRows.filter((row) => {
      if (title) {
        const haystack = `${row.title ?? ''} ${row.summary ?? ''}`.toLowerCase();
        if (!haystack.includes(title)) return false;
      }

      if (status === 'expired') return row.isExpired;
      if (status === 'active') return row.isActive && !row.isExpired;
      if (status === 'inactive') return !row.isActive && !row.isExpired;
      return true;
    });

    this.list$.next(this.rows);
  }

  formatDate(value: string): string {
    const parsed = jMoment(value).locale('fa');
    return parsed.isValid() ? parsed.format('jYYYY/jMM/jDD') : '—';
  }

  statusLabel(row: SpecialOfferListItemDto): string {
    if (row.isExpired) {
      return this._localization.translate('modules.profile.specialOffers.status.expired');
    }
    if (row.isActive) {
      return this._localization.translate('modules.profile.specialOffers.status.active');
    }
    return this._localization.translate('modules.profile.specialOffers.status.inactive');
  }

  openRequests(row: SpecialOfferListItemDto): void {
    const ref = this._dialog.open(SpecialOfferRequestsDialogComponent, {
      width: 'min(95vw, 56rem)',
      maxWidth: '95vw',
      autoFocus: false,
      panelClass: 'special-offer-requests-dialog-panel',
      data: { offerId: row.id, offerTitle: row.title },
    });

    ref.afterClosed().subscribe((result: SpecialOfferRequestsDialogCloseResult | undefined) => {
      if (!result?.navigateToAgreementForm || !result.requestId) return;
      void this._router.navigateByUrl(`/profile/agreements/new?requestId=${result.requestId}`);
    });
  }

  editRow(row: SpecialOfferListItemDto): void {
    void this._router.navigate(['/profile/special-offers', row.id], {
      queryParamsHandling: 'preserve',
    });
  }

  async deleteRow(row: SpecialOfferListItemDto): Promise<void> {
    const message = this._localization.translate('modules.profile.specialOffers.confirmDelete', {
      title: row.title,
    });
    if (!confirm(message)) return;

    const result = await this._service.delete(row.id);
    if (!result.success) {
      this.error =
        result.message ??
        this._localization.translate('modules.profile.specialOffers.errors.deleteFailed');
      return;
    }
    await this.load();
  }
}

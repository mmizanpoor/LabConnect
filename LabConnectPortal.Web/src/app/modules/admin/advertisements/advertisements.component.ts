import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { SitePermissionService } from '@core/services/auth/site-permission.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { ColDef } from 'ag-grid-community';
import { BehaviorSubject } from 'rxjs';
import { AdvertisementsColDef } from './advertisements.coldef';
import { AdvertisementsService } from './advertisements.service';
import { AdvertisementDto } from './advertisements.types';

@Component({
  selector: 'app-advertisements',
  standalone: true,
  imports: [TranslocoPipe, BaseButtonComponent, BaseGridComponent],
  templateUrl: './advertisements.component.html',
  styles: `
    :host {
      display: block;
      height: 100%;
      min-height: 0;
    }
  `,
})
export class AdvertisementsComponent implements OnInit {
  readonly entity = SystemEntity.ContentAds;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.ContentAds);
  private readonly _destroyRef = inject(DestroyRef);

  loading = false;
  error = '';
  rows: AdvertisementDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<AdvertisementDto[]>([]);

  constructor(
    private _service: AdvertisementsService,
    private _router: Router,
    private _localization: LocalizationService,
    private _colDef: AdvertisementsColDef,
    private _sitePermission: SitePermissionService,
  ) {}

  get canCreate(): boolean {
    return this._sitePermission.can(this.entity, 'create');
  }

  get canUpdate(): boolean {
    return this._sitePermission.can(this.entity, 'update');
  }

  get canDelete(): boolean {
    return this._sitePermission.can(this.entity, 'delete');
  }

  ngOnInit(): void {
    this.colDef = this._colDef.get();
    this._colDef.actionClicked.pipe(takeUntilDestroyed(this._destroyRef)).subscribe((evt) => {
      if (!evt?.row) return;
      if (evt.type === 'edit') {
        if (!this.canUpdate) return;
        this.openEdit(evt.row);
      }
      if (evt.type === 'delete') {
        if (!this.canDelete) return;
        void this.deleteRow(evt.row);
      }
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
          result.message ?? this._localization.translate('modules.admin.advertisements.errors.loadFailed'),
        );
      }
      this.rows = result.data;
      this.list$.next(this.rows);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.advertisements.errors.loadFailed');
      this.rows = [];
      this.list$.next([]);
    } finally {
      this.loading = false;
    }
  }

  openCreate(): void {
    if (!this.canCreate) return;
    void this._router.navigate(['/admin/ads/new']);
  }

  openEdit(row: AdvertisementDto): void {
    if (!this.canUpdate) return;
    void this._router.navigate(['/admin/ads', row.advertisementId]);
  }

  async deleteRow(row: AdvertisementDto): Promise<void> {
    if (!this.canDelete) return;
    const message = this._localization.translate('modules.admin.advertisements.confirmDelete', {
      title: row.title,
    });
    if (!confirm(message)) return;

    const result = await this._service.delete(row.advertisementId);
    if (!result.success) {
      this.error =
        result.message ?? this._localization.translate('modules.admin.advertisements.errors.deleteFailed');
      return;
    }
    await this.load();
  }
}

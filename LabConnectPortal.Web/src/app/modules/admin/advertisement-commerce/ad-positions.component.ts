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
import { AdPositionsColDef } from './ad-positions.coldef';
import { AdPositionsService } from './ad-positions.service';
import { AdvertisementPositionDto } from './advertisement-commerce.types';

@Component({
  selector: 'app-ad-positions',
  standalone: true,
  imports: [TranslocoPipe, BaseButtonComponent, BaseGridComponent],
  templateUrl: './ad-positions.component.html',
  styles: `:host { display: block; height: 100%; min-height: 0; }`,
})
export class AdPositionsComponent implements OnInit {
  readonly entity = SystemEntity.ContentAds;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.ContentAds);
  private readonly _destroyRef = inject(DestroyRef);

  loading = false;
  error = '';
  rows: AdvertisementPositionDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<AdvertisementPositionDto[]>([]);

  constructor(
    private _service: AdPositionsService,
    private _router: Router,
    private _localization: LocalizationService,
    private _colDef: AdPositionsColDef,
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
        void this._router.navigate(['/admin/ad-positions', evt.row.id]);
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
          result.message ??
            this._localization.translate('modules.admin.adCommerce.positions.errors.loadFailed'),
        );
      }
      this.rows = result.data;
      this.list$.next(this.rows);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.adCommerce.positions.errors.loadFailed');
      this.rows = [];
      this.list$.next([]);
    } finally {
      this.loading = false;
    }
  }

  openCreate(): void {
    if (!this.canCreate) return;
    void this._router.navigate(['/admin/ad-positions/new']);
  }

  async deleteRow(row: AdvertisementPositionDto): Promise<void> {
    const message = this._localization.translate('modules.admin.adCommerce.positions.confirmDelete', {
      title: row.title,
    });
    if (!confirm(message)) return;

    const result = await this._service.delete(row.id);
    if (!result.success) {
      this.error = result.message ?? this._localization.translate('shared.error');
      return;
    }
    await this.load();
  }
}

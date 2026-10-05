import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { SitePermissionService } from '@core/services/auth/site-permission.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';
import { ColDef } from 'ag-grid-community';
import { BehaviorSubject } from 'rxjs';
import { SiteServicesColDef } from './site-services.coldef';
import { SiteServicesService } from '../dashboard/site-services.service';
import { SiteServiceDto } from '../dashboard/site-services.types';
import { SiteServiceFormDialogComponent } from './site-service-form-dialog.component';

@Component({
  selector: 'app-site-services',
  standalone: true,
  imports: [
    TranslocoPipe,
    MatDialogModule,
    BaseButtonComponent,
    BaseGridComponent,
  ],
  templateUrl: './site-services.component.html',
  styles: `
    :host {
      display: block;
      height: 100%;
      min-height: 0;
    }
  `,
})
export class SiteServicesComponent implements OnInit {
  readonly entity = SystemEntity.SiteService;
  private readonly _destroyRef = inject(DestroyRef);

  loading = false;
  error = '';
  rows: SiteServiceDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<SiteServiceDto[]>([]);

  constructor(
    private _service: SiteServicesService,
    private _dialog: MatDialog,
    private _localization: LocalizationService,
    private _colDef: SiteServicesColDef,
    private _sitePermission: SitePermissionService
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
    this._colDef.actionClicked
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe((evt) => {
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
          result.message ??
            this._localization.translate(
              'modules.admin.dashboard.services.loadFailed'
            )
        );
      }
      this.rows = result.data;
      this.list$.next(this.rows);
    } catch (e: unknown) {
      console.error(e);
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.admin.dashboard.services.loadFailed'
            );
      this.rows = [];
      this.list$.next([]);
    } finally {
      this.loading = false;
    }
  }

  openCreate(): void {
    if (!this.canCreate) return;
    const ref = this._dialog.open(SiteServiceFormDialogComponent, {
      width: '460px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: {},
    });
    ref.afterClosed().subscribe((saved) => {
      if (saved) void this.load();
    });
  }

  openEdit(row: SiteServiceDto): void {
    if (!this.canUpdate) return;
    const ref = this._dialog.open(SiteServiceFormDialogComponent, {
      width: '460px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: { service: row },
    });
    ref.afterClosed().subscribe((saved) => {
      if (saved) void this.load();
    });
  }

  async deleteRow(row: SiteServiceDto): Promise<void> {
    if (!this.canDelete) return;
    const message = this._localization.translate(
      'modules.admin.dashboard.services.confirmDelete',
      { title: row.title }
    );
    if (!confirm(message)) return;
    const result = await this._service.delete(row.siteServiceId);
    if (!result.success) {
      this.error =
        result.message ??
        this._localization.translate(
          'modules.admin.dashboard.services.deleteFailed'
        );
      return;
    }
    await this.load();
  }
}

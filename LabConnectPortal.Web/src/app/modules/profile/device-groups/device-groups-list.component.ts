import { Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { DeviceGroupsService } from './device-groups.service';
import { DeviceGroupDto } from './device-groups.types';
import { DeviceGroupFormDialogComponent } from './device-group-form-dialog/device-group-form-dialog.component';
import { DeviceGroupsColDef, DeviceGroupAction } from './device-groups.coldef';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';

@Component({
  selector: 'app-device-groups-list',
  standalone: true,
  imports: [
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseGridComponent,
  ],
  templateUrl: './device-groups-list.component.html',
  styleUrl: './device-groups-list.component.scss',
})
export class DeviceGroupsListComponent implements OnInit {
  readonly entity = SystemEntity.DeviceGroup;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.DeviceGroup);

  loading = false;
  error = '';
  rows: DeviceGroupDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<DeviceGroupDto[]>([]);

  constructor(
    private _service: DeviceGroupsService,
    private _dialog: MatDialog,
    private _localization: LocalizationService,
    private _colDef: DeviceGroupsColDef,
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

  ngOnInit(): void {
    this.colDef = this._colDef.get(this.canUpdate, this.canDelete);
    this._colDef.actionClicked.subscribe((evt: DeviceGroupAction) => {
      if (!evt?.row) return;
      if (evt.type === 'edit') this.openEdit(evt.row);
      if (evt.type === 'delete') void this.deleteRow(evt.row);
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
          result.message ?? this._localization.translate('modules.profile.deviceGroups.errors.loadFailed'),
        );
      }
      this.rows = result.data;
      this.list$.next(this.rows);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.deviceGroups.errors.loadFailed');
      this.rows = [];
      this.list$.next([]);
    } finally {
      this.loading = false;
    }
  }

  openCreate(): void {
    const ref = this._dialog.open(DeviceGroupFormDialogComponent, {
      width: '420px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: {},
    });
    ref.afterClosed().subscribe((saved) => {
      if (saved) void this.load();
    });
  }

  openEdit(row: DeviceGroupDto): void {
    const ref = this._dialog.open(DeviceGroupFormDialogComponent, {
      width: '420px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: { group: row },
    });
    ref.afterClosed().subscribe((saved) => {
      if (saved) void this.load();
    });
  }

  async deleteRow(row: DeviceGroupDto): Promise<void> {
    const message = this._localization.translate('modules.profile.deviceGroups.confirmDelete', {
      title: row.title,
    });
    if (!confirm(message)) return;

    const result = await this._service.delete(row.id);
    if (!result.success) {
      this.error =
        result.message ?? this._localization.translate('modules.profile.deviceGroups.errors.deleteFailed');
      return;
    }
    await this.load();
  }
}

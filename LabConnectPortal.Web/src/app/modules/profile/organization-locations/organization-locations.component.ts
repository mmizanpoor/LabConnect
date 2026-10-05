import { Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { ReferenceItemDto } from '../resume/resume.types';
import { ResumeService } from '../resume/resume.service';
import { OrganizationLocationsService } from './organization-locations.service';
import { OrganizationLocationDto } from './organization-locations.types';
import { LocationFormDialogComponent } from './location-form-dialog/location-form-dialog.component';
import { OrganizationLocationsColDef, LocationAction } from './organization-locations.coldef';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';

@Component({
  selector: 'app-organization-locations',
  standalone: true,
  imports: [
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatDialogModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseGridComponent,
  ],
  templateUrl: './organization-locations.component.html',
  styleUrl: './organization-locations.component.scss',
})
export class OrganizationLocationsComponent implements OnInit {
  readonly entity = SystemEntity.Location;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.Location);

  loading = false;
  error = '';
  locations: OrganizationLocationDto[] = [];
  provinces: ReferenceItemDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<OrganizationLocationDto[]>([]);

  constructor(
    private _locationsService: OrganizationLocationsService,
    private _resumeService: ResumeService,
    private _dialog: MatDialog,
    private _localization: LocalizationService,
    private _colDef: OrganizationLocationsColDef,
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
    this._colDef.actionClicked.subscribe((evt: LocationAction) => {
      if (!evt?.row) return;
      if (evt.type === 'edit') this.openEdit(evt.row);
      if (evt.type === 'delete') void this.deleteLocation(evt.row);
    });
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const [locationsResult, refData] = await Promise.all([
        this._locationsService.getMyLocations(),
        this._resumeService.loadReferenceData(),
      ]);
      if (!locationsResult.success || !locationsResult.data) {
        throw new Error(locationsResult.message ?? this._localization.translate('modules.profile.locations.errors.loadFailed'));
      }
      this.locations = locationsResult.data;
      this.list$.next(this.locations);
      this.provinces = refData.provinces;
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.locations.errors.loadFailed');
      this.locations = [];
      this.list$.next([]);
    } finally {
      this.loading = false;
    }
  }

  openCreate(): void {
    const ref = this._dialog.open(LocationFormDialogComponent, {
      width: '520px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: { provinces: this.provinces },
    });
    ref.afterClosed().subscribe((saved) => {
      if (saved) void this.load();
    });
  }

  openEdit(location: OrganizationLocationDto): void {
    const ref = this._dialog.open(LocationFormDialogComponent, {
      width: '520px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: { provinces: this.provinces, location },
    });
    ref.afterClosed().subscribe((saved) => {
      if (saved) void this.load();
    });
  }

  async deleteLocation(location: OrganizationLocationDto): Promise<void> {
    const message = this._localization.translate('modules.profile.locations.confirmDelete', {
      name: location.locationName,
    });
    if (!confirm(message)) return;

    const result = await this._locationsService.delete(location.locationId);
    if (!result.success) {
      this.error = result.message ?? this._localization.translate('modules.profile.locations.errors.deleteFailed');
      return;
    }
    await this.load();
  }
}

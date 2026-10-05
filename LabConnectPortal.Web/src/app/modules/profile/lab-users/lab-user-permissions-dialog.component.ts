import { Component, Inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { LabUserPermissionDto } from '@core/services/auth/lab-permission.types';
import { SystemEntity } from '@core/system-entity/system-entity';
import { LabUsersService } from './lab-users.service';
import { LabMemberDto } from './lab-users.types';

export interface LabUserPermissionsDialogData {
  member: LabMemberDto;
}

type PermissionRow = LabUserPermissionDto & {
  labelKey: string;
};

@Component({
  selector: 'app-lab-user-permissions-dialog',
  standalone: true,
  imports: [
    FormsModule,
    MatDialogModule,
    BaseCheckboxComponent,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseDialogComponent,
  ],
  templateUrl: './lab-user-permissions-dialog.component.html',
})
export class LabUserPermissionsDialogComponent implements OnInit {
  loading = true;
  saving = false;
  error = '';
  rows: PermissionRow[] = [];

  private readonly _entityLabelKeys: Record<string, string> = {
    [SystemEntity.CenterProfile]: 'enum.systemEntity.CenterProfile',
    [SystemEntity.Location]: 'enum.systemEntity.Location',
    [SystemEntity.JobPosting]: 'enum.systemEntity.JobPosting',
    [SystemEntity.Product]: 'enum.systemEntity.Product',
    [SystemEntity.LabUser]: 'enum.systemEntity.LabUser',
    [SystemEntity.DeviceGroup]: 'enum.systemEntity.DeviceGroup',
    [SystemEntity.KitGroup]: 'enum.systemEntity.KitGroup',
    [SystemEntity.TestInfo]: 'enum.systemEntity.TestInfo',
    [SystemEntity.SpecialOffer]: 'enum.systemEntity.SpecialOffer',
    [SystemEntity.Reception]: 'enum.systemEntity.Reception',
    [SystemEntity.LabAgreement]: 'enum.systemEntity.LabAgreement',
    [SystemEntity.Message]: 'enum.systemEntity.Message',
    [SystemEntity.ActivityLog]: 'enum.systemEntity.ActivityLog',
  };

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: LabUserPermissionsDialogData,
    private _dialogRef: MatDialogRef<LabUserPermissionsDialogComponent, boolean>,
    private _labUsersService: LabUsersService,
    private _localization: LocalizationService
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      const permissions = await this._labUsersService.getMemberPermissions(
        this.data.member.id
      );
      this.rows = permissions.map((permission) => ({
        ...permission,
        labelKey:
          this._entityLabelKeys[permission.systemEntityId] ??
          permission.systemEntityId,
      }));
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.profile.labUsers.permissions.errors.loadFailed'
            );
    } finally {
      this.loading = false;
    }
  }

  async save(): Promise<void> {
    this.saving = true;
    this.error = '';
    try {
      await this._labUsersService.setMemberPermissions({
        memberId: this.data.member.id,
        permissions: this.rows.map(
          ({ systemEntityId, canView, canCreate, canUpdate, canDelete }) => ({
            systemEntityId,
            canView,
            canCreate,
            canUpdate,
            canDelete,
          })
        ),
      });
      this._dialogRef.close(true);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.profile.labUsers.permissions.errors.saveFailed'
            );
    } finally {
      this.saving = false;
    }
  }

  cancel(): void {
    this._dialogRef.close(false);
  }

  selectAllColumn(
    field: 'canView' | 'canCreate' | 'canUpdate' | 'canDelete'
  ): void {
    if (!this.rows.length) return;
    const allSelected = this.rows.every((row) => row[field]);
    for (const row of this.rows) {
      row[field] = !allSelected;
    }
  }
}

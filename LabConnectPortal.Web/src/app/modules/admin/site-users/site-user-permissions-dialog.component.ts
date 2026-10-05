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
import { SiteUserPermissionDto } from '@core/services/auth/site-permission.types';
import { SystemEntity } from '@core/system-entity/system-entity';
import { SiteUsersService } from './site-users.service';
import { SiteMemberDto } from './site-users.types';

export interface SiteUserPermissionsDialogData {
  member: SiteMemberDto;
}

type PermissionRow = SiteUserPermissionDto & {
  labelKey: string;
};

@Component({
  selector: 'app-site-user-permissions-dialog',
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
  templateUrl: './site-user-permissions-dialog.component.html',
})
export class SiteUserPermissionsDialogComponent implements OnInit {
  loading = true;
  saving = false;
  error = '';
  rows: PermissionRow[] = [];

  private readonly _entityLabelKeys: Record<string, string> = {
    [SystemEntity.User]: 'enum.systemEntity.User',
    [SystemEntity.Shop]: 'enum.systemEntity.Shop',
    [SystemEntity.Laboratory]: 'enum.systemEntity.Laboratory',
    [SystemEntity.ProductCategory]: 'enum.systemEntity.ProductCategory',
    [SystemEntity.ProductAttribute]: 'enum.systemEntity.ProductAttribute',
    [SystemEntity.Brand]: 'enum.systemEntity.Brand',
    [SystemEntity.Product]: 'enum.systemEntity.Product',
    [SystemEntity.ContentGroup]: 'enum.systemEntity.ContentGroup',
    [SystemEntity.Post]: 'enum.systemEntity.Post',
    [SystemEntity.ContentNews]: 'enum.systemEntity.ContentNews',
    [SystemEntity.ContentArticles]: 'enum.systemEntity.ContentArticles',
    [SystemEntity.ContentDocuments]: 'enum.systemEntity.ContentDocuments',
    [SystemEntity.ContentAds]: 'enum.systemEntity.ContentAds',
    [SystemEntity.SliderGroup]: 'enum.systemEntity.SliderGroup',
    [SystemEntity.CompanyRegulation]: 'enum.systemEntity.CompanyRegulation',
    [SystemEntity.SpecialOffer]: 'enum.systemEntity.SpecialOffer',
    [SystemEntity.Message]: 'enum.systemEntity.Message',
    [SystemEntity.Settings]: 'enum.systemEntity.Settings',
    [SystemEntity.ActivityLog]: 'enum.systemEntity.ActivityLog',
    [SystemEntity.LoginReport]: 'enum.systemEntity.LoginReport',
  };

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: SiteUserPermissionsDialogData,
    private _dialogRef: MatDialogRef<SiteUserPermissionsDialogComponent, boolean>,
    private _siteUsersService: SiteUsersService,
    private _localization: LocalizationService
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      const permissions = await this._siteUsersService.getMemberPermissions(
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
              'modules.admin.siteUsers.permissions.errors.loadFailed'
            );
    } finally {
      this.loading = false;
    }
  }

  async save(): Promise<void> {
    this.saving = true;
    this.error = '';
    try {
      await this._siteUsersService.setMemberPermissions({
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
              'modules.admin.siteUsers.permissions.errors.saveFailed'
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

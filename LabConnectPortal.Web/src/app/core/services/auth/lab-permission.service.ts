import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { AuthUtils } from './auth.utils';
import {
  LabPermissionAction,
  LabUserPermissionDto,
} from './lab-permission.types';

@Injectable({ providedIn: 'root' })
export class LabPermissionService {
  private static readonly StoreEntities: readonly string[] = [
    SystemEntity.CenterProfile,
    SystemEntity.Location,
    SystemEntity.JobPosting,
    SystemEntity.Product,
    SystemEntity.ProductOrder,
  ];

  private readonly _permissions = new Map<string, LabUserPermissionDto>();
  private _loaded = false;

  constructor(
    private _http: ApiHttpService,
    private _authUtils: AuthUtils
  ) {}

  get isLoaded(): boolean {
    return this._loaded;
  }

  async load(): Promise<void> {
    this._permissions.clear();
    this._loaded = false;

    if (!this._authUtils.isUserLab()) {
      this._loaded = true;
      return;
    }

    const result = await this._http.get<LabUserPermissionDto[]>(
      'LabUser',
      'GetMyPermissions'
    );
    if (result.success && result.data) {
      for (const permission of result.data) {
        this._permissions.set(
          permission.systemEntityId.toLowerCase(),
          permission
        );
      }
    }
    this._loaded = true;
  }

  clear(): void {
    this._permissions.clear();
    this._loaded = false;
  }

  can(entity: string, action: LabPermissionAction): boolean {
    if (this._authUtils.isAdminLab() || this._authUtils.isAdministrator()) {
      return SystemEntity.LabPortalAll.includes(entity);
    }

    if (this._authUtils.isStore()) {
      return LabPermissionService.StoreEntities.includes(entity);
    }

    if (!this._authUtils.isUserLab()) {
      return false;
    }

    const permission = this._permissions.get(entity.toLowerCase());
    if (!permission) {
      return false;
    }

    switch (action) {
      case 'view':
        return permission.canView;
      case 'create':
        return permission.canCreate;
      case 'update':
        return permission.canUpdate;
      case 'delete':
        return permission.canDelete;
      default:
        return false;
    }
  }

  canView(entity: string): boolean {
    return this.can(entity, 'view');
  }

  canExport(entity: string): boolean {
    return this.canView(entity);
  }

  canReport(entity: string): boolean {
    return this.canView(entity);
  }

  canPrint(entity: string): boolean {
    return this.canView(entity);
  }

  hasAnyLabPortalAccess(): boolean {
    if (this._authUtils.isAdminLab()) {
      return true;
    }
    if (!this._authUtils.isUserLab()) {
      return false;
    }
    return SystemEntity.LabPortalAll.some((entity) => this.can(entity, 'view'));
  }
}
